using GrainInterfaces;
using Orleans;
using Orleans.Runtime;

namespace Grains;

public class CounterGrain : Grain, ICounterGrain
{
	private readonly IPersistentState<CounterPersistence> _counterState;
	private readonly IPersistentState<CounterSecondPersistence> _counterSecondState;

	public CounterGrain(
		[PersistentState("CounterState", "Basic")]
		IPersistentState<CounterPersistence> sheepState,
		[PersistentState("CounterSecondState", "Basic")]
		IPersistentState<CounterSecondPersistence> counterSecondState)
	{
		_counterState = sheepState;
		_counterSecondState = counterSecondState;
	}

	public async Task Increment()
	{
		_counterState.State.Counter++;
		await _counterState.WriteStateAsync();
		_counterSecondState.State.JustAnotherGrain = GrainFactory.GetGrain<ICounterGrain>(this.GetPrimaryKeyString());
		await _counterSecondState.WriteStateAsync();
	}

	public async Task<int> GetCount()
	{
		_counterSecondState.State.ReadOperations++;
		await _counterSecondState.WriteStateAsync();
		return _counterState.State.Counter;
	}

	public Task<ICounterGrain?> GetSavedReference()
	{
		return Task.FromResult(_counterSecondState.State.JustAnotherGrain);
	}
}