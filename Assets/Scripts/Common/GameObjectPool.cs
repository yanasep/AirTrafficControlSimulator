using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool<T> : IDisposable where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Stack<T> stack = new();

    public GameObjectPool(T prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public T Rent()
    {
        var ins = stack.Count > 0 ? stack.Pop() : GameObject.Instantiate(prefab, parent);
        ins.gameObject.SetActive(true);
        return ins;
    }

    public void Return(T instance)
    {
        instance.gameObject.SetActive(false);
        stack.Push(instance);
    }

    public void Dispose()
    {
        while (stack.Count > 0)
        {
            var ins = stack.Pop();
            if (stack != null) GameObject.Destroy(ins.gameObject);
        }
    }
}