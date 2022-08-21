using UnityEngine;

namespace AirTraffic.Main
{
    public class Airplane : MonoBehaviour
    {
        [SerializeField] public float Speed = 0.2f;
        [SerializeField] private float turnBrakeRate = 0.5f;
        [SerializeField] public float AngleSpeed = 20;

        public bool IsMoving { get; set; }
        private bool isTurning;
        private Quaternion targetRotation;
        private TurnDirection turnDir;
        private float turnRemainingAngle;

        private readonly Vector3 upward = Vector3.back;

        public float CurrentSpeed => isTurning ? Speed * turnBrakeRate : Speed;

        public void OnUpdate()
        {
            if (IsMoving)
            {
                if (isTurning) TurnMovementUpdate(Time.deltaTime);
                else ForwardMovementUpdate(Time.deltaTime);
            }
        }

        public void MoveStart(Vector3 direction)
        {
            IsMoving = true;
            isTurning = false;
            targetRotation = Quaternion.LookRotation(direction, upward);
            transform.rotation = targetRotation;
        }

        public void Stop()
        {
            IsMoving = false;
        }

        public void Turn(Vector3 targetDir)
        {
            var rot = Quaternion.LookRotation(targetDir, upward);
            if (rot == targetRotation) return;

            this.targetRotation = rot;

            float signeAngleDiff = Vector3.SignedAngle(transform.forward, targetDir, upward);

            this.turnDir = signeAngleDiff > 0 ? (TurnDirection)1 : (TurnDirection)(-1);

            turnRemainingAngle = Mathf.Repeat(signeAngleDiff * (int)turnDir, 360);

            if (turnRemainingAngle == 0) return;

            isTurning = true;
        }

        public void Turn(Vector3 targetDir, TurnDirection turnDir)
        {
            var rot = Quaternion.LookRotation(targetDir, upward);
            if (rot == targetRotation) return;

            this.targetRotation = rot;
            this.turnDir = turnDir;

            float signeAngleDiff = Vector3.SignedAngle(transform.forward, targetDir, upward);
            turnRemainingAngle = Mathf.Repeat(signeAngleDiff * (int)turnDir, 360);

            if (turnRemainingAngle == 0) return;

            isTurning = true;
        }

        private void ForwardMovementUpdate(float deltaTime)
        {
            transform.position += Speed * deltaTime * transform.forward;
        }

        private void TurnMovementUpdate(float deltaTime)
        {
            float angleDelta = AngleSpeed * deltaTime * (int)turnDir;

            if (Mathf.Abs(angleDelta) >= turnRemainingAngle)
            {
                transform.rotation = targetRotation;
                isTurning = false;
            }
            else
            {
                turnRemainingAngle -= Mathf.Abs(angleDelta);
                transform.rotation = Quaternion.AngleAxis(-(int)turnDir * turnRemainingAngle, upward) * targetRotation;
            }

            transform.position += Speed * turnBrakeRate * deltaTime * transform.forward;
        }
    }
}