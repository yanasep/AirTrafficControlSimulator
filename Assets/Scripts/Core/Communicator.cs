using System;
using System.Collections.Generic;

public class Communicator<T>
{
    private readonly List<Action<T>> listeners = new();

    public IDisposable AddListener(Action<T> listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);

            return Disposable.Create(() => { RemoveListener(listener); });
        }

        return Disposable.Empty;
    }

    public void RemoveListener(Action<T> listener)
    {
        listeners.Remove(listener);
    }

    public void Invoke(T arg)
    {
        for (var i = 0; i < listeners.Count; i++) listeners[i].Invoke(arg);
    }
}