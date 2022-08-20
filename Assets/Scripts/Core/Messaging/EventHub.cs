using System;
using Cysharp.Threading.Tasks;

/// <summary>
/// イベント管理 (PubSub)
/// NOTE : UniRx.MessageBrokerの実装を真似ている
/// </summary>
public class EventHub
{
    private readonly AsyncMessageBroker asyncMessageBroker = new();
    private readonly MessageBroker messageBroker = new();

    /// <summary>
    /// イベント発信
    /// </summary>
    public void Publish<T>(T message) where T : IGameEvent
    {
        messageBroker.Publish(message);
    }

    /// <summary>
    /// イベント購読
    /// </summary>
    public IDisposable Subscribe<T>(Action<T> action) where T : IGameEvent
    {
        return messageBroker.Subscribe(action);
    }

    /// <summary>
    /// メッセージ発信
    /// </summary>
    public UniTask PublishAsync<T>(T message) where T : IAsyncGameEvent
    {
        return asyncMessageBroker.PublishAsync(message);
    }

    /// <summary>
    /// メッセージ購読
    /// </summary>
    public IDisposable Subscribe<T>(Func<T, UniTask> asyncMessageReceiver) where T : IAsyncGameEvent
    {
        return asyncMessageBroker.Subscribe(asyncMessageReceiver);
    }
}

/// <summary>
///     同期送信するEventのinterface
/// </summary>
public interface IGameEvent
{
}


/// <summary>
///     非同期送信するEventのinterface
/// </summary>
public interface IAsyncGameEvent
{
}