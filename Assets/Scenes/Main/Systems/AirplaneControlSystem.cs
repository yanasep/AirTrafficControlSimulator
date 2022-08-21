using ECS;
using UnityEngine;
using VContainer;

namespace AirTraffic.Main
{
    [DisallowMultipleComponent]
    public class AirplaneControlSystem : MonoBehaviour, IGameSystem, IUpdateSystem
    {
        private readonly CompositeDisposable disposables = new();
        private MainObjectState objectState;
        private bool isStarted;

        [Inject]
        public void Init(MainObjectState objectState, MessageHub messageHub)
        {
            this.objectState = objectState;
            messageHub.Event.Subscribe<OnGameStartEvent>(_ => OnGameStart()).AddTo(disposables);
            messageHub.Event.Subscribe<OnControlInputEvent>(OnControlInput).AddTo(disposables);

            foreach (var airplane in objectState.Airplanes.Values)
            {
                OnAddAirplane(airplane);
            }

            messageHub.Event.Subscribe<OnAddAirplaneEvent>(evt =>
            {
                var airplane = objectState.Airplanes[evt.AirplaneID];
                OnAddAirplane(airplane);
            }).AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        public void OnUpdate()
        {
            foreach (var airplane in objectState.Airplanes.Values)
            {
                airplane.OnUpdate();
            }
        }

        private void OnAddAirplane(Airplane airplane)
        {
            if (isStarted)
            {
                airplane.MoveStart(airplane.transform.forward);
            }
        }

        private void OnGameStart()
        {
            isStarted = true;

            foreach (var airplane in objectState.Airplanes.Values)
            {
                airplane.MoveStart(airplane.transform.forward);
            }
        }

        private void OnControlInput(OnControlInputEvent evt)
        {
            if (objectState.Airplanes.TryGetValue(evt.AirplaneID, out var airplane))
            {
                var targetDir = Quaternion.AngleAxis(evt.TargetAngle, Vector3.back) * Vector3.up;
                airplane.Turn(targetDir, evt.TurnDirection);
            }
        }
    }
}