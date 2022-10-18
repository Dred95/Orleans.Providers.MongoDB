using GrainInterfaces;
using Orleans;
using Orleans.Runtime;

namespace TestOrleansMongo;

public class TestCounterStartupTask : IStartupTask
{
	private IClusterClient _clusterClient;

	public TestCounterStartupTask(IClusterClient clusterClient)
	{
		_clusterClient = clusterClient;
	}

	public async Task Execute(CancellationToken cancellationToken)
	{
		const string counterId = "test_counter_id_0";
		var counterGrain = _clusterClient.GetGrain<ICounterGrain>(counterId);
		await counterGrain.Increment();
		await counterGrain.Increment();
		await counterGrain.Increment();
		await counterGrain.Increment();
		var count = await counterGrain.GetCount();
		Console.WriteLine($"{counterId} count is {count}");
	}
}