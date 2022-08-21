using UnityEngine;

namespace AirTraffic.Main
{
    public class AirplaneMovement
    {
        private readonly Airplane airplane;

        private bool isTurning;
        private Quaternion targetRotation;
        private TurnDirection turnDir;
        private float turnRemainingAngle;

        private readonly Vector3 upward = Vector3.back;

        public AirplaneMovement(Airplane airplane)
        {
            this.airplane = airplane;
        }

        public void OnUpdate()
        {
            if (airplane.IsMoving)
            {
                if (isTurning) TurnMovementUpdate(Time.deltaTime);
                else ForwardMovementUpdate(Time.deltaTime);
            }
        }

        public void MoveStart(Vector3 direction)
        {
            airplane.IsMoving = true;
            isTurning = false;
            targetRotation = Quaternion.LookRotation(direction, upward);
            airplane.transform.rotation = targetRotation;
        }

        public void Stop()
        {
            airplane.IsMoving = false;
        }

        public void Turn(Vector3 targetDir)
        {
            var rot = Quaternion.LookRotation(targetDir, upward);
            if (rot == targetRotation) return;

            this.targetRotation = rot;

            float signeAngleDiff = Vector3.SignedAngle(airplane.transform.forward, targetDir, upward);

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

            float signeAngleDiff = Vector3.SignedAngle(airplane.transform.forward, targetDir, upward);
            turnRemainingAngle = Mathf.Repeat(signeAngleDiff * (int)turnDir, 360);

            if (turnRemainingAngle == 0) return;

            isTurning = true;
        }

        private void ForwardMovementUpdate(float deltaTime)
        {
            airplane.transform.position += airplane.Speed * deltaTime * airplane.transform.forward;
        }

        private void TurnMovementUpdate(float deltaTime)
        {
            float angleDelta = airplane.AngleSpeed * deltaTime * (int)turnDir;

            if (Mathf.Abs(angleDelta) >= turnRemainingAngle)
            {
                airplane.transform.rotation = targetRotation;
                isTurning = false;
            }
            else
            {
                turnRemainingAngle -= Mathf.Abs(angleDelta);
                airplane.transform.rotation =
                    Quaternion.AngleAxis(-(int)turnDir * turnRemainingAngle, upward) * targetRotation;
            }

            airplane.transform.position +=
                airplane.Speed * airplane.TurnBrakeRate * deltaTime * airplane.transform.forward;
        }
    }
}