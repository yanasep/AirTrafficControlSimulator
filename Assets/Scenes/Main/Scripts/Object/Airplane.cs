using UnityEngine;

namespace AirTraffic.Main
{
    public class Airplane : MonoBehaviour
    {
        [SerializeField] private float speed = 0.15f;
        [SerializeField] private float turnBrakeRate = 0.5f;
        [SerializeField] private float angleSpeed = 180;

        public bool IsMoving { get; private set; }
        private bool isTurning;
        private Quaternion targetRotation;
        private TurnDirection turnDir;
        private float turnRemainingAngle;

        private readonly Vector3 upward = Vector3.back;

        public float Speed => isTurning ? speed * turnBrakeRate : speed;

        private void Update()
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
            transform.position += speed * deltaTime * transform.forward;
        }

        private void TurnMovementUpdate(float deltaTime)
        {
            float angleDelta = angleSpeed * deltaTime * (int)turnDir;

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

            transform.position += speed * turnBrakeRate * deltaTime * transform.forward;
        }
    }
}