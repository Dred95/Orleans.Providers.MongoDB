using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ArrangeAccessorOwnerBody

namespace Orleans.Providers.MongoDB.Utils
{
	public class CollectionBase<TEntity>
	{
		private const string CollectionFormat = "{0}Set";

		protected static readonly UpdateOptions Upsert = new() { IsUpsert = true };
		protected static readonly ReplaceOptions UpsertReplace = new() { IsUpsert = true };
		protected static readonly UpdateDefinitionBuilder<TEntity> Update = Builders<TEntity>.Update;
		protected static readonly FilterDefinitionBuilder<TEntity> Filter = Builders<TEntity>.Filter;
		protected static readonly ProjectionDefinitionBuilder<TEntity> Project = Builders<TEntity>.Projection;

		private readonly IMongoDatabase _mongoDatabase;
		private readonly IMongoClient _mongoClient;
		private readonly Lazy<IMongoCollection<TEntity>> _mongoCollection;
		private readonly bool _createShardKey;

		protected IMongoCollection<TEntity> Collection => _mongoCollection.Value;

		protected IMongoDatabase Database => _mongoDatabase;

		public IMongoClient Client => _mongoClient;

		protected CollectionBase(IMongoClient mongoClient, string databaseName,
			Action<MongoCollectionSettings>? collectionConfigurator, bool createShardKey)
		{
			_mongoClient = mongoClient;

			_mongoDatabase = mongoClient.GetDatabase(databaseName);
			_mongoCollection = CreateCollection(collectionConfigurator);

			_createShardKey = createShardKey;
		}

		protected virtual MongoCollectionSettings CollectionSettings()
		{
			return new MongoCollectionSettings();
		}

		protected virtual string CollectionName()
		{
			return string.Format(CultureInfo.InvariantCulture, CollectionFormat, typeof(TEntity).Name);
		}

		protected virtual void SetupCollection(IMongoCollection<TEntity> collection)
		{
		}

		private Lazy<IMongoCollection<TEntity>> CreateCollection(
			Action<MongoCollectionSettings>? collectionConfigurator)
		{
			return new Lazy<IMongoCollection<TEntity>>(() =>
			{
				var collectionFilter = new ListCollectionNamesOptions
				{
					Filter = Builders<BsonDocument>.Filter.Eq("name", CollectionName())
				};

				if (!_mongoDatabase.ListCollectionNames(collectionFilter).Any())
				{
					_mongoDatabase.CreateCollection(CollectionName());
				}

				var collectionSettings = CollectionSettings();

				collectionConfigurator?.Invoke(collectionSettings);

				var databaseCollection = _mongoDatabase.GetCollection<TEntity>(CollectionName(), collectionSettings);

				if (_createShardKey)
				{
					try
					{
						Database.RunCommand<BsonDocument>(new BsonDocument
						{
							["key"] = new BsonDocument
							{
								["_id"] = "hashed"
							},
							["shardCollection"] = $"{_mongoDatabase.DatabaseNamespace.DatabaseName}.{CollectionName()}"
						});
					}
					catch (MongoException)
					{
						// Shared key probably created already.
					}
				}

				SetupCollection(databaseCollection);

				return databaseCollection;
			});
		}
	}
}