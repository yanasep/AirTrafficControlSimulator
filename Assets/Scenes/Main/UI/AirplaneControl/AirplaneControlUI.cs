using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace AirTraffic.Main.UI
{
    public class AirplaneControlUI : MonoBehaviour
    {
        [field: SerializeField] public AirplaneSelectionPanel PlaneSelection { get; private set; }
        [field: SerializeField] public TurnDirectionSelectionPanel DirSelection { get; private set; }
        [field: SerializeField] public AngleSelectionPanel AngleSelection { get; private set; }
        [field: SerializeField] public float NextControlDelay;
        [SerializeField] private TextMeshProUGUI commandText;
        [SerializeField] private float commandTextShowDuration = 1f;

        private void Awake()
        {
            PlaneSelection.SetVisible(false);
            DirSelection.SetVisible(false);
            AngleSelection.SetVisible(false);
            commandText.gameObject.SetActive(false);
        }

        public void AddPlaneData(uint id, string airplaneName)
        {
            PlaneSelection.AddAirplane(id, airplaneName);
        }

        public async UniTask ShowCommandLabelAsync(string msg)
        {
            commandText.SetText(msg);
            commandText.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(commandTextShowDuration));
            if (this == null) return;
            commandText.gameObject.SetActive(false);
        }
    }
}