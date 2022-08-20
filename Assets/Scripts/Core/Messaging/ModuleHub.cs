using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 返り値ありのPubSub
/// </summary>
public class ModuleHub
{
    private readonly ModuleBroker moduleBroker = new();
    private readonly AsyncModuleBroker asyncModuleBroker = new();

    /// <summary>
    /// 関数呼び出し
    /// </summary>
    public TResult Call<TArg, TResult>(TArg arg) where TArg : IGameModule
    {
        return moduleBroker.Call<TArg, TResult>(arg);
    }

    /// <summary>
    /// 呼び出される関数をセット
    /// </summary>
    public IDisposable Register<TArg, TResult>(Func<TArg, TResult> func) where TArg : IGameModule
    {
        return moduleBroker.Register(func);
    }
    
    /// <summary>
    /// 関数呼び出し (非同期)
    /// </summary>
    public UniTask<TResult> CallAsync<TArg, TResult>(TArg arg) where  TArg : IAsyncGameModule
    {
        return asyncModuleBroker.CallAsync<TArg, TResult>(arg);
    }

    /// <summary>
    /// 呼び出される関数をセット
    /// </summary>
    public IDisposable Register<TArg, TResult>(Func<TArg, UniTask<TResult>> func) where TArg : IAsyncGameModule
    {
        return asyncModuleBroker.Register(func);
    }

    /// <summary>
    /// 破棄
    /// </summary>
    public void Dispose()
    {
        moduleBroker.Dispose();
        asyncModuleBroker.Dispose();
    }
}

/// <summary>
/// ModuleHubで呼ぶ引数のinterface
/// </summary>
public interface IGameModule
{
}

/// <summary>
/// ModuleHubで呼ぶ引数のinterface (非同期)
/// </summary>
public interface IAsyncGameModule
{
}