using System;
using System.Collections;
using System.Collections.Generic;
using AirTraffic.Main.UI;
using Cysharp.Threading.Tasks;
using ECS;
using MeshGeneration;
using UnityEngine;

namespace AirTraffic.Main
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private Airplane airplaneBase;
        [SerializeField] private AirplaneControlUI airplaneControlUI;
        [SerializeField] private BezierPath splinePath;
        [SerializeField] private AirplaneLabelUI labelUI;
        [SerializeField] private AirplaneSpawnSetting[] spawnSettings;
        [SerializeField] private PathSettings pathSettings;

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
            public BezierPath[] Pathes;
            public float RestartDelay;
        }

        private void Start()
        {
            messageHub = new MessageHub();
            mainObjectState = new MainObjectState(messageHub);
            masterSystem = new MasterSystem(
                new AirplaneSplineFollowSystem(pathSettings, mainObjectState, messageHub),
                new AirplaneLabelUISystem(labelUI, messageHub, mainObjectState)
                // new AirplaneControlSystem(mainObjectState, messageHub)
                // new AirplaneControlUISystem(airplaneControlUI, messageHub, mainObjectState)
            );

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

        private void OnDestroy()
        {
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