using System.Collections.Generic;

namespace AirTraffic.Main
{
    public class MainObjectState
    {
        private readonly MessageHub messageHub;

        public Dictionary<uint, Airplane> Airplanes => airplanes;
        private readonly Dictionary<uint, Airplane> airplanes = new();

        private uint nextAirplaneID = 0;

        public MainObjectState(MessageHub messageHub)
        {
            this.messageHub = messageHub;
        }

        public void AddAirplane(Airplane airplane)
        {
            var id = nextAirplaneID;
            nextAirplaneID++;
            airplanes.Add(id, airplane);
            messageHub.Event.Publish(new OnAddAirplaneEvent
            {
                AirplaneID = id,
                AirplaneName = airplane.name
            });
        }
    }
}