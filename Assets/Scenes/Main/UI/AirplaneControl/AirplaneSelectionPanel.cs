using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AirTraffic.Main.UI
{
    public class AirplaneSelectionPanel : MonoBehaviour
    {
        [SerializeField] private CommonButton buttonPrefab;
        [SerializeField] private Transform buttonParent;
        [SerializeField] private CanvasGroup canvasGroup;

        public event Action<uint> OnAirplaneSelected;

        public void AddAirplane(uint id, string airplaneName)
        {
            var button = GameObject.Instantiate(buttonPrefab, buttonParent);
            button.SetLabel(airplaneName);
            button.OnClick.AddListener(() => OnAirplaneSelected?.Invoke(id));
        }

        public void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}