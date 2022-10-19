using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.Serialization;
using Orleans.Storage;

namespace Orleans.Providers.MongoDB.StorageProviders.Serializers
{
	public class JsonGrainStateSerializer : IGrainStateSerializer
	{
		private readonly JsonSerializerSettings _jsonSettings;

		public JsonGrainStateSerializer(IOptions<JsonGrainStorageSerializerOptions> options, IServiceProvider services)
		{
			_jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(
				OrleansJsonSerializer.GetDefaultSerializerSettings(services),
				options.Value.UseFullAssemblyNames,
				options.Value.IndentJson,
				options.Value.TypeNameHandling);

			options.Value.ConfigureJsonSerializerSettings?.Invoke(_jsonSettings);
		}

		public string Serialize<TState>(IGrainState<TState> grainState)
		{
			var data = JsonConvert.SerializeObject(grainState.State, _jsonSettings);
			Console.WriteLine(data);
			return data;
		}

		public TState Deserialize<TState>(string serialized)
		{
			TState? deserializeObject = JsonConvert.DeserializeObject<TState>(serialized, _jsonSettings);
			if (deserializeObject == null)
			{
				throw new InvalidOperationException("Deserialized empty state");
			}

			return deserializeObject;
		}
	}
}