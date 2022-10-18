namespace Orleans.Providers.MongoDB.StorageProviders
{
	public interface IGrainStateSerializer
	{
		string Serialize<TState>(IGrainState<TState> grainState);

		TState Deserialize<TState>(string serialized);
	}
}