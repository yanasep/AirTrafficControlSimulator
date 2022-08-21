using System;
using UnityEngine;

namespace AirTraffic.Main.UI
{
    public class TurnDirectionSelectionPanel : MonoBehaviour
    {
        [SerializeField] private CommonButton leftButton;
        [SerializeField] private CommonButton rightButton;
        [SerializeField] private CanvasGroup canvasGroup;

        public event Action<TurnDirection> OnDirectionSelected;

        private void Awake()
        {
            leftButton.OnClick.AddListener(() => OnDirectionSelected?.Invoke(TurnDirection.Left));
            rightButton.OnClick.AddListener(() => OnDirectionSelected?.Invoke(TurnDirection.Right));
        }

        public void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}