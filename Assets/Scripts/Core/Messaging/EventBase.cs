using System;
using System.Collections.Generic;

public class EventBase<T> : IGameEvent, IDisposable where T : class, new()
{
    private static readonly Stack<T> Pool = new();

    public void Dispose()
    {
        Pool.Push(this as T);
    }

    public static T GetPooled()
    {
        return Pool.Count > 0 ? Pool.Pop() : new T();
    }
}

public class AsyncEventBase<T> : IAsyncGameEvent, IDisposable where T : class, new()
{
    private static readonly Stack<T> Pool = new();

    public void Dispose()
    {
        Pool.Push(this as T);
    }

    public static T GetPooled()
    {
        return Pool.Count > 0 ? Pool.Pop() : new T();
    }
}