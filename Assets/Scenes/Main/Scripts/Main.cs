using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AirTraffic.Main.UI;
using Cysharp.Threading.Tasks;
using ECS;
using MeshGeneration;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace AirTraffic.Main
{
    public class Main : LifetimeScope
    {
        [SerializeField] private Airplane airplaneBase;
        [SerializeField] private AirplaneSpawnSetting[] spawnSettings;

        private MainObjectState mainObjectState;
        private MessageHub messageHub;
        private MasterSystem masterSystem;

        [Serializable]
        public class AirplaneSpawnSetting
        {
            public string AirplaneName;
            public float Delay;
        }

        [Serializable]
        public class PathSettings
        {
            public BezierPath[] Paths;
            public float RestartDelay;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(messageHub);
            builder.RegisterInstance(mainObjectState);
        }

        private void Start()
        {
            messageHub = new MessageHub();
            mainObjectState = new MainObjectState(messageHub);

            Build();

            masterSystem = new MasterSystem(GetComponents<IGameSystem>());

            messageHub.Event.Publish(new OnGameStartEvent());

            airplaneBase.gameObject.SetActive(false);

            foreach (var setting in spawnSettings)
            {
                SpawnAsync(setting).Forget();
            }
        }

        private void Update()
        {
            masterSystem.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            masterSystem.Dispose();
        }

        private async UniTask SpawnAsync(AirplaneSpawnSetting setting)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(setting.Delay));
            if (this == null) return;
            var airplane = GameObject.Instantiate(airplaneBase);
            airplane.gameObject.SetActive(true);
            airplane.name = setting.AirplaneName;
            mainObjectState.AddAirplane(airplane);
        }
    }
}