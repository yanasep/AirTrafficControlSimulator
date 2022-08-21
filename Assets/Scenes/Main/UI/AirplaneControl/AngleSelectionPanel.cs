using System;
using UnityEngine;

namespace AirTraffic.Main.UI
{
    public class AngleSelectionPanel : MonoBehaviour
    {
        [SerializeField] private CommonButton buttonBase;
        [SerializeField] private Transform buttonParent;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private int angleStep = 20;

        public event Action<int> OnAngleSelected;

        public void Awake()
        {
            Debug.Assert(360 % angleStep == 0, "angleStep is not divisor of 360");
            int divisions = 360 / angleStep;
            for (int i = 1; i <= divisions; i++)
            {
                int angle = i * angleStep;
                var button = GameObject.Instantiate(buttonBase, buttonParent);
                button.SetLabel($"{angle}");
                button.OnClick.AddListener(() => OnAngleSelected?.Invoke(angle));
            }

            buttonBase.gameObject.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}