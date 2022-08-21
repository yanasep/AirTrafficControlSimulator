using System;
using System.Collections;
using System.Collections.Generic;

public class Disposable : IDisposable
{
    private Action callback;
    public static Disposable Empty { get; } = new();

    public static IDisposable Create(Action callback)
    {
        var disposable = new Disposable();
        disposable.callback = callback;
        return disposable;
    }

    public void Dispose()
    {
        callback?.Invoke();
    }
}

public static class DisposableExtensions
{
    public static void AddTo(this IDisposable self, CompositeDisposable disposables)
    {
        disposables.Add(self);
    }
}

public class CompositeDisposable : IDisposable, IEnumerable<IDisposable>
{
    private List<IDisposable> disposables = new();
    public bool IsDisposed => disposables == null;

    public void Add(IDisposable disposable)
    {
        if (disposables == null) throw new Exception("Disposed");

        disposables.Add(disposable);
    }

    public void Dispose()
    {
        if (disposables == null) return;

        for (int i = 0; i < disposables.Count; i++)
        {
            disposables[i].Dispose();
        }
        disposables = null;
    }

    public IEnumerator<IDisposable> GetEnumerator()
    {
        foreach (var disposable in disposables)
        {
            yield return disposable;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var disposable in disposables)
        {
            yield return disposable;
        }
    }
}
