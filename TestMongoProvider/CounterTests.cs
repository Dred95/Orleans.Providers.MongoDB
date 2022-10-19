using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace TestMongoProvider;

public class CounterTests : MongoTest
{
	[Test]
	public async Task TestCountGrows()
	{
		const string counterId = "test_counter_id_0";
		var counterGrain = ClusterClient.GetGrain<ICounterGrain>(counterId);
		var before = await counterGrain.GetCount();
		await counterGrain.Increment();
		var after = await counterGrain.GetCount();
		Assert.That(before + 1 == after);
	}

	[Test]
	public async Task TestCountNotZero()
	{
		const string counterId = "test_counter_id_0";
		var counterGrain = ClusterClient.GetGrain<ICounterGrain>(counterId);
		var count = await counterGrain.GetCount();
		Assert.That(count > 0);
	}

	[Test]
	public async Task TestReferenceDeserialized()
	{
		const string counterId = "test_counter_id_0";
		var counterGrain = ClusterClient.GetGrain<ICounterGrain>(counterId);
		var reference = await counterGrain.GetSavedReference();
		Assert.That(reference != null);
		Assert.That(reference!.Equals(counterGrain));
	}

	protected override void ConfigureServices(IServiceCollection serviceCollection)
	{
	}
}