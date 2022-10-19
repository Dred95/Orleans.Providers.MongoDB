﻿using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Serialization;
using Orleans.Storage;
using TestOrleansMongo;

public class Program
{
	public static async Task Main()
	{
		var silo = await StartSilo();
	}

	private static async Task<IHost> StartSilo()
	{
		IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
		hostBuilder.UseConsoleLifetime();
		hostBuilder.ConfigureLogging(logging => logging.AddConsole());
		IHost host = hostBuilder.UseOrleans((_, siloBuilder) =>
		{
			siloBuilder.UseLocalhostClustering();

			siloBuilder.UseMongoDBClient(
				"mongodb+srv://dredstd:RbvEnbZ2Bc6JM8xK@experimental01.eshlc7z.mongodb.net/basic");
			siloBuilder.AddStartupTask<TestCounterStartupTask>();
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