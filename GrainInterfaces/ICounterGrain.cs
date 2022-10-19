using Orleans;

namespace GrainInterfaces;

public interface ICounterGrain : IGrainWithStringKey
{
	Task Increment();
	Task<int> GetCount();

	Task<ICounterGrain?> GetSavedReference();
}