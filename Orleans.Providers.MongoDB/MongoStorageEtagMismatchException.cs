namespace Orleans.Providers.MongoDB;

public class MongoStorageEtagMismatchException : Exception
{
	public string CurrentETag { get; }
	public string ReceivedEtag { get; }

	public MongoStorageEtagMismatchException(string currentETag, string receivedEtag)
	{
		CurrentETag = currentETag;
		ReceivedEtag = receivedEtag;
	}
}