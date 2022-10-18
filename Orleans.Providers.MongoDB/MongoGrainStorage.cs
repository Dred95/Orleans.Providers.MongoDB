using System.Collections.Concurrent;
using System.Diagnostics;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orleans.Providers.MongoDB.StorageProviders;
using Orleans.Providers.MongoDB.Utils;

namespace Orleans.Providers.MongoDB
{
	[DebuggerDisplay("MongoStore:{" + nameof(_name) + "}")]
	public class MongoGrainStorage : IGrainStorage, IDisposable
	{
		private readonly ConcurrentDictionary<string, MongoGrainStorageCollection> collections = new();
		private readonly string _name;
		private readonly IMongoClient _mongoClient;


		private readonly MongoDBGrainStorageOptions _options;
		private readonly IGrainStateSerializer _serializer;

		public MongoGrainStorage(string name, MongoDBGrainStorageOptions options,
			IMongoClientFactory mongoClientFactory, IGrainStateSerializer serializer)
		{
			_serializer = serializer;
			_options = options;
			_mongoClient = mongoClientFactory.Create(name);
			_name = name;
		}

		protected virtual string ReturnGrainName(string grainType)
		{
			return grainType.Split('.', '+').Last();
		}

		private MongoGrainStorageCollection GetCollection(string grainType)
		{
			var collectionName = $"{_options.CollectionPrefix}{ReturnGrainName(grainType)}";

			return collections.GetOrAdd(grainType, _ =>
				new MongoGrainStorageCollection(
					_mongoClient,
					_options.DatabaseName,
					collectionName,
					_options.CollectionConfigurator,
					_options.CreateShardKeyForCosmos,
					_serializer,
					_options.KeyGenerator));
		}

		public virtual async Task ReadStateAsync<T>(string grainType, GrainReference grainReference,
			IGrainState<T> grainState)
		{
			MongoGrainStorageCollection grainStorageCollection = GetCollection(grainType);
			await grainStorageCollection.ReadAsync(grainReference, grainState);
		}

		public virtual async Task WriteStateAsync<T>(string grainType, GrainReference grainReference,
			IGrainState<T> grainState)
		{
			MongoGrainStorageCollection grainStorageCollection = GetCollection(grainType);
			await grainStorageCollection.WriteAsync(grainReference, grainState);
		}

		public virtual async Task ClearStateAsync<T>(string grainType, GrainReference grainReference,
			IGrainState<T> grainState)
		{
			MongoGrainStorageCollection grainStorageCollection = GetCollection(grainType);
			await grainStorageCollection.ClearAsync(grainReference, grainState);
		}

		public void Dispose() => collections.Clear();
	}

	public static class MongoGrainStorageFactory
	{
		public static IGrainStorage Create(IServiceProvider services, string name)
		{
			return ActivatorUtilities.CreateInstance<MongoGrainStorage>(services,
				services.GetRequiredService<IOptionsMonitor<MongoDBGrainStorageOptions>>().Get(name), name);
		}
	}
}