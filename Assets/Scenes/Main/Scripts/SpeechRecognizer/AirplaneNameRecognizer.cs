#if UNITY_EDITOR || UNITY_WSA
using System;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;

namespace AirTraffic.Main
{
    public class AirplaneNameRecognizer : IDisposable
    {
        public event Action<uint> OnRecognized;

        private readonly List<KeywordRecognizer> recognizers = new();
        private bool isListening;

        public void AddRecognizer(uint airplaneID, string airplaneName)
        {
            var recognizer = new KeywordRecognizer(new[] { airplaneName });
            recognizer.OnPhraseRecognized += _ => OnRecognized?.Invoke(airplaneID);

            recognizers.Add(recognizer);

            if (isListening) recognizer.Start();
        }

        public void Start()
        {
            isListening = true;

            foreach (var recognizer in recognizers)
            {
                recognizer.Start();
            }
        }

        public void Stop()
        {
            isListening = false;

            foreach (var recognizer in recognizers)
            {
                recognizer.Stop();
            }
        }

        public void Dispose()
        {
            foreach (var recognizer in recognizers)
            {
                recognizer.Dispose();
            }
        }
    }
}
#endif
