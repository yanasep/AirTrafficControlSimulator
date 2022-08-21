using System;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace AirTraffic.Main
{
    public class KeywordRecognizerTester : MonoBehaviour
    {
        [SerializeField] private string[] keywords;

        private KeywordRecognizer recognizer;

        private void Start()
        {
            recognizer = new KeywordRecognizer(keywords);
            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();
        }

        private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Debug.Log(
                $"Recognized : {args.text}, Confidence : {args.confidence}, Phrase Duration : {args.phraseDuration}");
        }

        private void OnDestroy()
        {
            recognizer.Dispose();
        }
    }
}