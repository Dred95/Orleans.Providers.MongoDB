using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Providers.MongoDB;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Providers.MongoDB.StorageProviders;
using Orleans.Providers.MongoDB.StorageProviders.Serializers;
using Orleans.Providers.MongoDB.Utils;
using Orleans.Runtime;
using Orleans.Storage;

// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable CheckNamespace

namespace Orleans.Hosting
{
    /// <summary>
    /// Extension methods for configuration classes specific to OrleansMongoUtils.dll 
    /// </summary>
    public static class MongoDBSiloExtensions
    {
        public static IServiceCollection AddMongoDBClient(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IMongoClient>(c => new MongoClient(connectionString));
            services.AddSingleton<IMongoClientFactory, DefaultMongoClientFactory>();

            return services;
        }

        /// <summary>
        /// Configure silo to use MongoDb with a passed in connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static ISiloBuilder UseMongoDBClient(this ISiloBuilder builder, string connectionString)
        {
            return builder.ConfigureServices(services => services.AddMongoDBClient(connectionString));
        }


        /// <summary>
        /// Configure silo to use MongoDB as the default grain storage.
        /// </summary>
        public static ISiloBuilder AddMongoDBGrainStorageAsDefault(this ISiloBuilder builder,
            Action<OptionsBuilder<MongoDBGrainStorageOptions>> configureOptions = null)
        {
            return builder.AddMongoDBGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
        }

        /// <summary>
        /// Configure silo to use MongoDB for grain storage.
        /// </summary>
        public static ISiloBuilder AddMongoDBGrainStorage(this ISiloBuilder builder, string name,
            Action<OptionsBuilder<MongoDBGrainStorageOptions>> configureOptions = null)
        {
            return builder.ConfigureServices(services => services.AddMongoDBGrainStorage(name, configureOptions));
        }

        /// <summary>
        /// Configure silo to use MongoDB for grain storage.
        /// </summary>
        public static IServiceCollection AddMongoDBGrainStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<MongoDBGrainStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<MongoDBGrainStorageOptions>(name));
            services.AddSingleton<IGrainStateSerializer, JsonGrainStateSerializer>();
            services.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));/*
            services.TryAddSingleton<IGrainStateSerializer>(sp => new JsonGrainStateSerializer(sp.GetService<ITypeResolver>(), 
                sp.GetService<IGrainFactory>(), sp.GetService<IOptionsMonitor<MongoDBGrainStorageOptions>>().Get(name)));*/

            services.ConfigureNamedOptionForLogging<MongoDBGrainStorageOptions>(name);
            services.AddSingletonNamedService(name, MongoGrainStorageFactory.Create);
            return services;
        }
    }
}