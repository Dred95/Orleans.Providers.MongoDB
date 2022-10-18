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
		const string counterId0 = "test_counter_id_0";
		var counterGrain0 = _clusterClient.GetGrain<ICounterGrain>(counterId0);
		await counterGrain0.Increment();
		var count0 = await counterGrain0.GetCount();

		const string counterId1 = "test_counter_id_1";
		var counterGrain1 = _clusterClient.GetGrain<ICounterGrain>(counterId1);
		await counterGrain1.Increment();
		var count1 = await counterGrain1.GetCount();


		Console.WriteLine($"{counterId0} count is {count0}");
		Console.WriteLine($"{counterId1} count is {count1}");
	}
}