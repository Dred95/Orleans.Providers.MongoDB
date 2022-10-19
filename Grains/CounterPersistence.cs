using GrainInterfaces;
using Orleans;

namespace Grains;

[GenerateSerializer]
public class CounterPersistence
{
	[Id(0)]
	public int Counter;
}

[GenerateSerializer]
public class CounterSecondPersistence
{
	[Id(0)]
	public int ReadOperations;

	[Id(1)]
	public ICounterGrain? JustAnotherGrain;
}