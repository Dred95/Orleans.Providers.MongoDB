using GrainInterfaces;
using Orleans;
using Orleans.Runtime;

namespace Grains;

public class CounterGrain : Grain, ICounterGrain
{
	private readonly IPersistentState<CounterPersistence> _sheepState;

	public CounterGrain([PersistentState("CounterState", "Basic")]IPersistentState<CounterPersistence> sheepState)
	{
		_sheepState = sheepState;
	}

	public async Task Increment()
	{
		_sheepState.State.Counter++;
		await _sheepState.WriteStateAsync();
	}

	public Task<int> GetCount()
	{
		return Task.FromResult(_sheepState.State.Counter);
	}
}