using System.Text.Json;

namespace Orleans.Providers.MongoDB.StorageProviders.Serializers
{
	public class JsonGrainStateSerializer : IGrainStateSerializer
	{
		private IServiceProvider _serviceProvider;

		private readonly JsonSerializerOptions _options = new()
		{
			IncludeFields = true
		};

		public JsonGrainStateSerializer(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public string Serialize<TState>(IGrainState<TState> grainState)
		{
			return JsonSerializer.Serialize(grainState.State, _options);
		}

		public TState Deserialize<TState>(string serialized)
		{
			TState? stored = JsonSerializer.Deserialize<TState>(serialized, _options);
			if (stored != null)
			{
				return stored;
			}

			throw new Exception("null deserialized");
		}
	}
}