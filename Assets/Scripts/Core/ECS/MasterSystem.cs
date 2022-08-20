using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS
{
    /// <summary>
    /// Systemを管理する
    /// </summary>
    public class MasterSystem : IDisposable
    {
        private readonly List<IGameSystem> systems = new();
        private readonly List<IUpdateSystem> updateSystems = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MasterSystem(params IGameSystem[] systems)
        {
            foreach (var sys in systems)
            {
#if UNITY_EDITOR
                if (this.systems.Contains(sys))
                {
                    Debug.LogWarning($"すでに存在します : {sys.GetType()}");
                }
#endif
                this.systems.Add(sys);
                if (sys is IUpdateSystem us) updateSystems.Add(us);
            }
        }

        /// <summary>
        /// System更新
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < updateSystems.Count; i++)
            {
                updateSystems[i].OnUpdate();
            }
        }

        /// <summary>
        /// System全部破棄
        /// </summary>
        public void Dispose()
        {
            foreach (var sys in systems)
            {
                sys.Dispose();
            }
            systems.Clear();
            updateSystems.Clear();
        }
    }
}
