using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS
{
    public interface IGameSystem : IDisposable
    {
    }

    public interface IUpdateSystem
    {
        void OnUpdate();
    }
}