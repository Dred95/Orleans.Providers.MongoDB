using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Serialization;
using Orleans.Storage;

namespace TestMongoProvider;

public abstract class MongoTest
{
	protected IClusterClient ClusterClient { get; private set; } = null!;
	private IHost _host = null!;

	[SetUp]
	public async Task SetupOrleansCluster()
	{
		IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
		hostBuilder.UseConsoleLifetime();
		hostBuilder.ConfigureLogging(logging => logging.AddConsole());
		_host = hostBuilder.UseOrleans((_, siloBuilder) =>
		{
			siloBuilder.UseLocalhostClustering();

			siloBuilder.UseMongoDBClient(
				"mongodb+srv://dredstd:RbvEnbZ2Bc6JM8xK@experimental01.eshlc7z.mongodb.net/basic");
			siloBuilder.Services.AddSerializer(builder =>
			{
				builder.AddJsonSerializer(configureOptions:
					optionsBuilder =>
					{
						optionsBuilder.Configure(
							options =>
							{
								options.SerializerOptions.IncludeFields = true;
								options.SerializerOptions.WriteIndented = true;
							});
					});
			});
			siloBuilder.Services.AddSingleton<IGrainStorageSerializer, JsonGrainStorageSerializer>();
			siloBuilder.AddMongoDBGrainStorage("Basic", ConfigureMongoOptions);
		}).Build();

		await _host.StartAsync();

		ClusterClient = _host.Services.GetService<IClusterClient>()!;
	}


	private static void ConfigureMongoOptions(OptionsBuilder<MongoDBGrainStorageOptions> builder)
	{
		builder.Configure(options =>
		{
			options.DatabaseName = "basic";
			options.ClientName = "hello";
		});
	}

	protected abstract void ConfigureServices(IServiceCollection serviceCollection);
}