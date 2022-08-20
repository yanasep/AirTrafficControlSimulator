using System;

public class ReactiveProperty<T> where T : IEquatable<T>
{
    private T value;

    public T Value
    {
        get => value;
        set
        {
            if (this.value.Equals(value)) return;
            this.value = value;
            communicator.Invoke(value);
        }
    }

    private readonly Communicator<T> communicator = new();

    public IDisposable Subscribe(Action<T> action)
    {
        communicator.AddListener(action);
        return Disposable.Create(() => { communicator.RemoveListener(action); });
    }
}