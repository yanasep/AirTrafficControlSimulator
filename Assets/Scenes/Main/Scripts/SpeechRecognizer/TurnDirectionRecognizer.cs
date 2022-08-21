using System;
using UnityEngine.Windows.Speech;

namespace AirTraffic.Main
{
    public class TurnDirectionRecognizer : IDisposable
    {
        private static readonly string[] Keywords = new[] { "Left", "Right" };
        private static readonly TurnDirection[] Directions = new[] { TurnDirection.Left, TurnDirection.Right };

        public event Action<TurnDirection> OnRecognized;

        private readonly KeywordRecognizer recognizer;

        public TurnDirectionRecognizer()
        {
            recognizer = new KeywordRecognizer(Keywords);
            recognizer.OnPhraseRecognized += OnPhraseRecognized;
        }

        public void Start()
        {
            recognizer.Start();
        }

        public void Stop()
        {
            recognizer.Stop();
        }

        private void OnPhraseRecognized(PhraseRecognizedEventArgs evt)
        {
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (evt.text == Keywords[i])
                {
                    OnRecognized?.Invoke(Directions[i]);
                    break;
                }
            }
        }

        public void Dispose()
        {
            recognizer.Dispose();
        }
    }
}