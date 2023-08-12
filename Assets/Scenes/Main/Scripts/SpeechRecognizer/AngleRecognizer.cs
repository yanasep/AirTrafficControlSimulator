#if UNITY_EDITOR || UNITY_WSA
using System;
using System.Linq;
using UnityEngine.Windows.Speech;

namespace AirTraffic.Main
{
    public class AngleRecognizer : IDisposable
    {
        public event Action<int> OnRecognized;

        private readonly string[] keywords;
        private readonly KeywordRecognizer recognizer;
        private const int DigitCount = 3;
        private int recognizedDigitCount;
        private int recognizedNum;

        public AngleRecognizer()
        {
            keywords = Enumerable.Range(0, 10).Select(x => x.ToString()).ToArray();
            recognizer = new KeywordRecognizer(keywords);
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
            for (int i = 0; i < keywords.Length; i++)
            {
                if (evt.text == keywords[i])
                {
                    recognizedNum = recognizedNum * 10 + i;
                    recognizedDigitCount++;
                    if (recognizedDigitCount >= DigitCount)
                    {
                        var num = recognizedNum;
                        recognizedNum = 0;
                        recognizedDigitCount = 0;
                        OnRecognized?.Invoke(num);
                    }

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
#endif
