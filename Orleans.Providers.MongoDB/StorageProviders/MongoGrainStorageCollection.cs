using MongoDB.Bson;
using MongoDB.Driver;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Providers.MongoDB.Utils;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Providers.MongoDB.StorageProviders
{
	internal sealed class MongoGrainStorageCollection : CollectionBase<BsonDocument>
	{
		private const string FieldId = "_id";
		private const string FieldState = "_state";
		private const string FieldEtag = "_etag";
		private readonly string collectionName;
		private readonly IGrainStateSerializer serializer;
		private readonly GrainStorageKeyGenerator keyGenerator;

		public MongoGrainStorageCollection(
			IMongoClient mongoClient,
			string databaseName,
			string collectionName,
			Action<MongoCollectionSettings>? collectionConfigurator,
			bool createShardKey,
			IGrainStateSerializer serializer,
			GrainStorageKeyGenerator keyGenerator)
			: base(mongoClient, databaseName, collectionConfigurator, createShardKey)
		{
			this.collectionName = collectionName;
			this.serializer = serializer;
			this.keyGenerator = keyGenerator;
		}

		protected override string CollectionName()
		{
			return collectionName;
		}

		public async Task ReadAsync<T>(GrainReference grainReference, IGrainState<T> grainState)
		{
			var grainKey = keyGenerator(grainReference);

			var existing =
				await Collection.Find(Filter.Eq(FieldId, grainKey))
					.FirstOrDefaultAsync();

			if (existing != null)
			{
				grainState.RecordExists = true;
				grainState.ETag = existing[FieldEtag].AsString;
				var deserialized = serializer.Deserialize<T>(existing[FieldState].AsString);
				grainState.State = deserialized;
			}
		}

		public async Task WriteAsync<T>(GrainReference grainReference, IGrainState<T> grainState)
		{
			var grainKey = keyGenerator(grainReference);

			var grainData = serializer.Serialize(grainState);

			var etag = grainState.ETag;

			var newETag = Guid.NewGuid().ToString();

			grainState.RecordExists = true;

			try
			{
				await Collection.UpdateOneAsync(
					Filter.And(
						Filter.Eq(FieldId, grainKey),
						Filter.Eq(FieldEtag, grainState.ETag)),
					Update
						.Set(FieldEtag, newETag)
						.Set(FieldState, grainData),
					Upsert);
			}
			catch (MongoException ex)
			{
				if (IsDuplicateKey(ex))
				{
					await ThrowForOtherEtag(grainKey, etag, ex);

					var document = new BsonDocument
					{
						[FieldId] = grainKey,
						[FieldEtag] = grainKey,
						[FieldState] = grainData
					};

					try
					{
						await Collection.ReplaceOneAsync(Filter.Eq(FieldId, grainKey), document, UpsertReplace);
					}
					catch (MongoException ex2)
					{
						if (IsDuplicateKey(ex2))
						{
							await ThrowForOtherEtag(grainKey, etag, ex2);
						}
						else
						{
							throw;
						}
					}
				}
				else
				{
					throw;
				}
			}

			grainState.ETag = newETag;
		}

		public Task ClearAsync<T>(GrainReference grainReference, IGrainState<T> grainState)
		{
			var grainKey = keyGenerator(grainReference);

			grainState.RecordExists = false;

			return Collection.DeleteManyAsync(Filter.Eq(FieldId, grainKey));
		}

		private async Task ThrowForOtherEtag(string key, string etag, Exception ex)
		{
			var existingEtag =
				await Collection.Find(Filter.Eq(FieldId, key))
					.Project<BsonDocument>(Project.Exclude(FieldState)).FirstOrDefaultAsync();

			if (existingEtag != null && existingEtag.Contains(FieldEtag))
			{
				throw new InconsistentStateException(existingEtag[FieldEtag].AsString, etag, ex);
			}
		}

		public static bool IsDuplicateKey(MongoException ex)
		{
			switch (ex)
			{
				case MongoCommandException { Code: 11000 }:
				case MongoWriteException w when w.WriteError.Category == ServerErrorCategory.DuplicateKey:
					return true;
				default:
					return false;
			}
		}
	}
}