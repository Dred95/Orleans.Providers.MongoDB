using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using TestOrleansMongo;

public class Program
{
	public static async Task Main()
	{
		var startSilo = await StartSilo();
	}


	private static async Task<IHost> StartSilo()
	{
		IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
		hostBuilder.UseConsoleLifetime();
		hostBuilder.ConfigureLogging(logging => logging.AddConsole());
		IHost host = hostBuilder.UseOrleans((a, siloBuilder) =>
		{
			siloBuilder.UseLocalhostClustering();
			siloBuilder.UseMongoDBClient("mongodb+srv://dredstd:RbvEnbZ2Bc6JM8xK@experimental01.eshlc7z.mongodb.net/basic");
			siloBuilder.AddMongoDBGrainStorage("Basic", ConfigureMongoOptions);
			siloBuilder.AddStartupTask<TestCounterStartupTask>();
		}).Build();

		await host.StartAsync();
		return host;
	}

	private static void ConfigureMongoOptions(OptionsBuilder<MongoDBGrainStorageOptions> builder)
	{
		builder.Configure(options =>
		{
			options.DatabaseName = "basic";
			options.ClientName = "hello";
		});
	}
}