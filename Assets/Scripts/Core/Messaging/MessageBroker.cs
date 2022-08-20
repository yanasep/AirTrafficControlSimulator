using System;
using System.Collections.Generic;

/// <summary>
/// イベント管理 (PubSub)
/// NOTE : UniRx.MessageBrokerの実装を真似ている
/// </summary>
public class MessageBroker
{
    private bool isDisposed = false;
    private readonly Dictionary<Type, object> notifiers = new();

    /// <summary>
    /// イベント発信
    /// </summary>
    public void Publish<T>(T message)
    {
        object notifier;
        lock (notifiers)
        {
            if (isDisposed) return;

            if (!notifiers.TryGetValue(typeof(T), out notifier))
            {
                return;
            }
        }
        ((Communicator<T>)notifier).Invoke(message);
    }

    /// <summary>
    /// イベント購読
    /// </summary>
    public IDisposable Subscribe<T>(Action<T> action)
    {
        object notifier;
        lock (notifiers)
        {
            if (isDisposed) throw new ObjectDisposedException("MessageBroker");

            if (!notifiers.TryGetValue(typeof(T), out notifier))
            {
                notifier = new Communicator<T>();
                notifiers.Add(typeof(T), notifier);
            }
        }

        var communicator = (Communicator<T>)notifier;
        return communicator.AddListener(action);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (notifiers)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                notifiers.Clear();
            }
        }
    }
}