using ECS;

namespace AirTraffic.Main.UI
{
    public class AirplaneLabelUISystem : IGameSystem
    {
        private readonly AirplaneLabelUI labelUI;
        private readonly MessageHub messageHub;
        private readonly MainObjectState objectState;
        private readonly CompositeDisposable disposables = new();

        public AirplaneLabelUISystem(AirplaneLabelUI labelUI, MessageHub messageHub, MainObjectState objectState)
        {
            this.labelUI = labelUI;
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