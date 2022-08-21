using UnityEngine;

namespace AirTraffic.Main
{
    public struct OnControlInputEvent : IGameEvent
    {
        public uint AirplaneID;
        public int TargetAngle;
        public TurnDirection TurnDirection;
    }

    public struct OnAddAirplaneEvent : IGameEvent
    {
        public uint AirplaneID;
        public string AirplaneName;
    }

    public struct OnGameStartEvent : IGameEvent
    {
    }
}