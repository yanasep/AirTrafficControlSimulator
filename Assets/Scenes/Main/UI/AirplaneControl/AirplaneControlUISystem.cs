using System;
using Cysharp.Threading.Tasks;
using ECS;
using UnityEngine;
using VContainer;

namespace AirTraffic.Main.UI
{
    [DisallowMultipleComponent]
    public class AirplaneControlUISystem : MonoBehaviour, IGameSystem
    {
        [SerializeField] private AirplaneControlUI airplaneControlUI;
        private MessageHub messageHub;
        private MainObjectState objectState;
        private readonly CompositeDisposable disposables = new();

        private const float CommandDelay = 1f;

        private OnControlInputEvent controlInputEventData;

        [Inject]
        public void Init(MessageHub messageHub, MainObjectState objectState)
        {
            this.messageHub = messageHub;
            this.objectState = objectState;

            foreach (var (id, airplane) in objectState.Airplanes)
            {
                OnAddAirplane(id, airplane.name);
            }

            messageHub.Event.Subscribe<OnGameStartEvent>(_ => StartUI()).AddTo(disposables);
            messageHub.Event.Subscribe<OnAddAirplaneEvent>(evt => OnAddAirplane(evt.AirplaneID, evt.AirplaneName))
                .AddTo(disposables);

            airplaneControlUI.PlaneSelection.OnAirplaneSelected += OnAirplaneSelected;
            airplaneControlUI.DirSelection.OnDirectionSelected += OnDirSelected;
            airplaneControlUI.AngleSelection.OnAngleSelected += OnAngleSelected;
        }

        public void Dispose()
        {
            disposables.Dispose();
            airplaneControlUI.PlaneSelection.OnAirplaneSelected -= OnAirplaneSelected;
            airplaneControlUI.DirSelection.OnDirectionSelected -= OnDirSelected;
            airplaneControlUI.AngleSelection.OnAngleSelected -= OnAngleSelected;
        }

        private void StartUI()
        {
            airplaneControlUI.PlaneSelection.SetVisible(true);
        }

        private void OnAddAirplane(uint id, string name)
        {
            airplaneControlUI.AddPlaneData(id, name);
        }

        private void OnAirplaneSelected(uint id)
        {
            controlInputEventData.AirplaneID = id;
            airplaneControlUI.PlaneSelection.SetVisible(false);
            airplaneControlUI.DirSelection.SetVisible(true);
        }

        private void OnDirSelected(TurnDirection dir)
        {
            controlInputEventData.TurnDirection = dir;
            airplaneControlUI.DirSelection.SetVisible(false);
            airplaneControlUI.AngleSelection.SetVisible(true);
        }

        private void OnAngleSelected(int angle)
        {
            controlInputEventData.TargetAngle = angle;
            airplaneControlUI.AngleSelection.SetVisible(false);
            WaitForNextAsync().Forget();

            var airplane = objectState.Airplanes[controlInputEventData.AirplaneID];
            string msg = $"{airplane.name} Turn {controlInputEventData.TurnDirection} {angle}";
            airplaneControlUI.ShowCommandLabelAsync(msg).Forget();

            SendCommandAsync().Forget();

            async UniTask WaitForNextAsync()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(airplaneControlUI.NextControlDelay));
                airplaneControlUI.PlaneSelection.SetVisible(true);
            }

            async UniTask SendCommandAsync()
            {
                await UniTask.Delay(TimeSpan.FromSeconds(CommandDelay));
                messageHub.Event.Publish(controlInputEventData);
            }
        }
    }
}