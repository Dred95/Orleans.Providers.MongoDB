using GrainInterfaces;
using Orleans;

namespace Grains;

public class CounterGrain : Grain, ICounterGrain
{
	private int _count;

	public Task Increment()
	{
		_count++;
		return Task.CompletedTask;
	}

	public Task<int> GetCount()
	{
		return Task.FromResult(_count);
	}
}