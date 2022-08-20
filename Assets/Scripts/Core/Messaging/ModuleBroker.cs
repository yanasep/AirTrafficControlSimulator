using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 関数管理 (返り値ありのPubSubのようなもの)
/// NOTE : UniRx.MessageBrokerの実装を真似ている
/// </summary>
public class ModuleBroker
{
    private readonly Dictionary<Type, object> funcs = new();

    private bool isDisposed;

    /// <summary>
    /// 関数呼び出し
    /// </summary>
    public TResult Call<TArg, TResult>(TArg arg) 
    {
        object func;
        lock (funcs)
        {
            if (isDisposed) throw new ObjectDisposedException("GameModuleBroker");

            // 対応するfuncを検索
            if (!funcs.TryGetValue(typeof(TArg), out func))
            {
                Debug.LogWarning("GameModuleBroker デフォルトを使用");
                return default;
            }
        }

        return ((Func<TArg, TResult>)func).Invoke(arg);
    }

    /// <summary>
    /// 呼び出される関数をセット
    /// </summary>
    public IDisposable Register<TArg, TResult>(Func<TArg, TResult> func) where TArg : IGameModule
    {
        lock (funcs)
        {
            if (isDisposed) throw new ObjectDisposedException("GameModuleBroker");
            funcs[typeof(TArg)] = func;
        }

        return Disposable.Create(() =>
        {
            lock (funcs)
            {
                funcs.Remove(typeof(TArg));
            }
        });
    }

    /// <summary>
    /// 破棄
    /// </summary>
    public void Dispose()
    {
        lock (funcs)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                funcs.Clear();
            }
        }
    }
}