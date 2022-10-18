using Orleans.Hosting;
using TestOrleansMongo;

public class Program
{
	public static async Task Main()
	{
		var startSilo = await StartSilo();
		Console.ReadKey();
	}


	private static async Task<IHost> StartSilo()
	{
		IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
		hostBuilder.UseConsoleLifetime();
		hostBuilder.ConfigureLogging(logging => logging.AddConsole());
		IHost host = hostBuilder.UseOrleans((a, siloBuilder) =>
		{
			siloBuilder.UseLocalhostClustering();
			siloBuilder.AddStartupTask<TestCounterStartupTask>();
		}).Build();

		await host.StartAsync();
		return host;
	}
}