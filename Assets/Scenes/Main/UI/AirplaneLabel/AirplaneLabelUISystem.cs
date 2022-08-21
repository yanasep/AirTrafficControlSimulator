using ECS;
using UnityEngine;
using VContainer;

namespace AirTraffic.Main.UI
{
    [DisallowMultipleComponent]
    public class AirplaneLabelUISystem : MonoBehaviour, IGameSystem
    {
        [SerializeField] private AirplaneLabelUI labelUI;
        private MessageHub messageHub;
        private MainObjectState objectState;
        private readonly CompositeDisposable disposables = new();

        [Inject]
        public void Init(MessageHub messageHub, MainObjectState objectState)
        {
            this.messageHub = messageHub;
            this.objectState = objectState;

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

        private void OnAddAirplane(Airplane airplane)
        {
            labelUI.AddLabel(airplane.transform, airplane.name);
        }
    }
}