#if UNITY_EDITOR || UNITY_WSA
using System;

namespace AirTraffic.Main
{
    public class CommandRecognizer : IDisposable
    {
        private readonly AirplaneNameRecognizer nameRecognizer;
        private readonly TurnDirectionRecognizer dirRecognizer;
        private readonly AngleRecognizer angleRecognizer;
        private bool isRunning;

        public event Action<CommandData> OnRecognize;

        public struct CommandData
        {
            public uint AirplaneID;
            public TurnDirection Direction;
            public int Angle;
        }

        private CommandData commandData;

        public CommandRecognizer()
        {
            nameRecognizer = new AirplaneNameRecognizer();
            dirRecognizer = new TurnDirectionRecognizer();
            angleRecognizer = new AngleRecognizer();

            nameRecognizer.OnRecognized += OnNameRecognized;
            dirRecognizer.OnRecognized += OnDirRecognized;
            angleRecognizer.OnRecognized += OnAngleRecognized;
        }

        public void Start()
        {
            if (isRunning) return;

            isRunning = true;
            nameRecognizer.Start();
        }

        public void Stop()
        {
            isRunning = false;
            nameRecognizer.Stop();
            dirRecognizer.Stop();
            angleRecognizer.Stop();
        }

        public void Dispose()
        {
            nameRecognizer.Dispose();
            dirRecognizer.Dispose();
            angleRecognizer.Dispose();
        }

        public void AddAirplane(uint airplaneID, string airplaneName)
        {
            nameRecognizer.AddRecognizer(airplaneID, airplaneName);
        }

        private void OnNameRecognized(uint id)
        {
            commandData.AirplaneID = id;
            nameRecognizer.Stop();
            dirRecognizer.Start();
        }

        private void OnDirRecognized(TurnDirection dir)
        {
            commandData.Direction = dir;
            dirRecognizer.Stop();
            angleRecognizer.Start();
        }

        private void OnAngleRecognized(int angle)
        {
            commandData.Angle = angle;
            angleRecognizer.Stop();

            var data = commandData;

            nameRecognizer.Start();

            OnRecognize?.Invoke(data);
        }
    }
}
#endif
