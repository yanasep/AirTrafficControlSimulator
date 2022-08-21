using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MeshGeneration;
using ECS;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AirTraffic.Main
{
    public class AirplaneSplineFollowSystem : IGameSystem, IUpdateSystem
    {
        private readonly Main.PathSettings pathSettings;

        private readonly Dictionary<Airplane, BezierPathTracker> pathTracers = new();

        private readonly CompositeDisposable disposables = new();
        private bool isStarted;
        private BezierPath prevPath;

        public AirplaneSplineFollowSystem(Main.PathSettings pathSettings, MainObjectState objectState,
            MessageHub messageHub)
        {
            this.pathSettings = pathSettings;

            foreach (var airplane in objectState.Airplanes.Values)
            {
                OnAddAirplane(airplane);
            }

            messageHub.Event.Subscribe<OnAddAirplaneEvent>(evt =>
            {
                var airplane = objectState.Airplanes[evt.AirplaneID];
                OnAddAirplane(airplane);
            }).AddTo(disposables);

            messageHub.Event.Subscribe<OnGameStartEvent>(evt => { OnGameStart(); }).AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        public void OnUpdate()
        {
            var dt = Time.deltaTime;

            foreach (var (airplane, tracker) in pathTracers)
            {
                if (!airplane.IsMoving) continue;

                var (nextPos, nextDir) = tracker.Step(airplane.Speed * dt);
                var forward = Vector3.RotateTowards(airplane.transform.forward,
                    (nextPos - airplane.transform.position).normalized, airplane.AngleSpeed * Mathf.Deg2Rad * dt, 0);
                var rot = Quaternion.LookRotation(forward, Vector3.back);
                airplane.transform.SetPositionAndRotation(nextPos, rot);

                if (tracker.IsEnd)
                {
                    airplane.IsMoving = false;
                    RestartPathAsync(airplane, tracker).Forget();
                }
            }
        }

        private void OnGameStart()
        {
            isStarted = true;

            foreach (var (airplane, tracker) in pathTracers)
            {
                StartPath(airplane, tracker);
            }
        }

        private void OnAddAirplane(Airplane airplane)
        {
            var pathTracer = new BezierPathTracker();
            pathTracers.Add(airplane, pathTracer);

            if (isStarted)
            {
                StartPath(airplane, pathTracer);
            }
        }

        private async UniTask RestartPathAsync(Airplane airplane, BezierPathTracker tracker)
        {
            airplane.Stop();

            await UniTask.Delay(TimeSpan.FromSeconds(pathSettings.RestartDelay));
            if (disposables.IsDisposed) return;

            StartPath(airplane, tracker);
        }

        private void StartPath(Airplane airplane, BezierPathTracker tracker)
        {
            BezierPath path;
            do
            {
                path = pathSettings.Pathes[Random.Range(0, pathSettings.Pathes.Length)];
            } while (path == prevPath);

            prevPath = path;

            tracker.Init(path, reverse: Random.value >= 0.5f);
            airplane.transform.position = tracker.StartPosition;
            airplane.transform.rotation = Quaternion.LookRotation(tracker.StartTangent, Vector3.back);
            airplane.IsMoving = true;
        }
    }
}