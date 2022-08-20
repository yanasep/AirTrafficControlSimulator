using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 関数管理 (返り値ありのPubSubのようなもの)
/// NOTE : UniRx.MessageBrokerの実装を真似ている
/// </summary>
public class AsyncModuleBroker
{
    private readonly Dictionary<Type, object> funcs = new();

    private bool isDisposed;

    /// <summary>
    /// 関数呼び出し
    /// </summary>
    public UniTask<TResult> CallAsync<TArg, TResult>(TArg arg) 
    {
        object func;
        lock (funcs)
        {
            if (isDisposed) throw new ObjectDisposedException("AsyncModuleBroker");

            // 対応するfuncを検索
            if (!funcs.TryGetValue(typeof(TArg), out func))
            {
                Debug.LogWarning("AsyncModuleBroker デフォルトを使用");
                return default;
            }
        }

        return ((Func<TArg, UniTask<TResult>>)func).Invoke(arg);
    }

    /// <summary>
    /// 呼び出される関数をセット
    /// </summary>
    public IDisposable Register<TArg, TResult>(Func<TArg, UniTask<TResult>> func)
    {
        lock (funcs)
        {
            if (isDisposed) throw new ObjectDisposedException("AsyncModuleBroker");
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