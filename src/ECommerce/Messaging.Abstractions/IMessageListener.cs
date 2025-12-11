namespace Messaging.Abstractions
{
	public interface IMessageListener
	{
		Task ListenAsync<T>(string queue, Func<T, Task> messageHandler);
	}
}
