using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AirTraffic.Main.UI
{
    public class CommonButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;

        public Button.ButtonClickedEvent OnClick => button.onClick;

        public void SetLabel(string msg)
        {
            label.SetText(msg);
        }
    }
}