using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
///     イベント管理 (非同期PubSub).
///     publish時、すべてのSubscriberの非同期処理の終了待ちができる。
///     NOTE : UniRx.AsyncMessageBrokerをUniTask版に改変している。
/// </summary>
public class AsyncMessageBroker : IDisposable
{
    private readonly Dictionary<Type, object> notifiers = new();
    private bool isDisposed;

    /// <inheritdoc />
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

    /// <summary>
    /// メッセージ発信
    /// </summary>
    public UniTask PublishAsync<T>(T message)
    {
        object _notifier;
        lock (notifiers)
        {
            if (isDisposed) throw new ObjectDisposedException("AsyncMessageBroker");

            if (!notifiers.TryGetValue(typeof(T), out _notifier))
                return UniTask.CompletedTask;
        }

        var notifier = (List<Func<T, UniTask>>)_notifier;
        var awaiter = new UniTask[notifier.Count];
        for (var i = 0; i < notifier.Count; i++) awaiter[i] = notifier[i].Invoke(message);
        return UniTask.WhenAll(awaiter);
    }

    /// <summary>
    /// メッセージ購読
    /// </summary>
    public IDisposable Subscribe<T>(Func<T, UniTask> asyncMessageReceiver)
    {
        lock (notifiers)
        {
            if (isDisposed) throw new ObjectDisposedException("AsyncMessageBroker");

            object _notifier;
            if (!notifiers.TryGetValue(typeof(T), out _notifier))
            {
                var notifier = new List<Func<T, UniTask>>();
                notifier.Add(asyncMessageReceiver);
                notifiers.Add(typeof(T), notifier);
            }
            else
            {
                var notifier = (List<Func<T, UniTask>>)_notifier;
                notifier.Add(asyncMessageReceiver);
            }
        }

        return new Subscription<T>(this, asyncMessageReceiver);
    }

    private class Subscription<T> : IDisposable
    {
        private readonly Func<T, UniTask> asyncMessageReceiver;
        private readonly AsyncMessageBroker parent;

        public Subscription(AsyncMessageBroker parent, Func<T, UniTask> asyncMessageReceiver)
        {
            this.parent = parent;
            this.asyncMessageReceiver = asyncMessageReceiver;
        }

        public void Dispose()
        {
            lock (parent.notifiers)
            {
                if (parent.notifiers.TryGetValue(typeof(T), out object _notifier))
                {
                    var notifier = (List<Func<T, UniTask>>)_notifier;
                    notifier.Remove(asyncMessageReceiver);
                }
            }
        }
    }
}