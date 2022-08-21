using UnityEngine;

namespace AirTraffic.Main
{
    public class Airplane : MonoBehaviour
    {
        [field: SerializeField] public float Speed { get; private set; } = 0.2f;
        [field: SerializeField] public float TurnBrakeRate { get; private set; } = 0.5f;
        [field: SerializeField] public float AngleSpeed { get; private set; } = 20;

        public bool IsMoving { get; set; }
    }
}