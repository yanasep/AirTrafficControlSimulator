using System;
using System.Collections.Generic;

public class Communicator
{
    private readonly List<Action> listeners = new();

    public IDisposable AddListener(Action listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);

            return Disposable.Create(() => { RemoveListener(listener); });
        }

        return Disposable.Empty;
    }

    public void RemoveListener(Action listener)
    {
        listeners.Remove(listener);
    }

    public void Invoke()
    {
        for (var i = 0; i < listeners.Count; i++) listeners[i].Invoke();
    }
}

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

public class Communicator<T0, T1>
{
    private readonly List<Action<T0, T1>> listeners = new();

    public IDisposable AddListener(Action<T0, T1> listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);

            return Disposable.Create(() => { RemoveListener(listener); });
        }

        return Disposable.Empty;
    }

    public void RemoveListener(Action<T0, T1> listener)
    {
        listeners.Remove(listener);
    }

    public void Invoke(T0 arg0, T1 arg1)
    {
        for (var i = 0; i < listeners.Count; i++) listeners[i].Invoke(arg0, arg1);
    }
}