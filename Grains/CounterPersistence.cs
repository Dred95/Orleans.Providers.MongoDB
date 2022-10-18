using Orleans;

namespace Grains;

[GenerateSerializer]
public class CounterPersistence
{
	[Id(0)]
	public int Counter;
}