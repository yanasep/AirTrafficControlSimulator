using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirTraffic.Main
{
    public class Airplane : MonoBehaviour
    {
        [SerializeField] private float defaultSpeed = 1f;
        [SerializeField] private float turnBrakeRate = 0.5f;
        [SerializeField] private float angleSpeed = 180;
        
        private float speed;
        private bool isMoving;
        private bool isTurning;
        private Vector3 targetDir;
        private TurnDirection turnDir;

        private void Awake()
        {
            speed = defaultSpeed;
        }

        private void Update()
        {
            if (isMoving)
            {
                if (isTurning) TurnMovementUpdate(Time.deltaTime);
                else ForwardMovementUpdate(Time.deltaTime);
            }
        }

        public void SetSpeed(float speed)
        {
            this.speed = speed;
        }

        public void MoveStart(Vector3 direction)
        {
            isMoving = true;
            targetDir = direction;
            isTurning = false;
            transform.forward = direction;
        }

        public void Stop()
        {
            isMoving = false;
        }

        public void Turn(Vector3 targetDir, TurnDirection turnDir)
        {
            this.targetDir = targetDir;
            this.turnDir = turnDir;
            isTurning = true;
        }

        private void ForwardMovementUpdate(float deltaTime)
        {
            transform.position += speed * deltaTime * transform.forward;
        }

        private void TurnMovementUpdate(float deltaTime)
        {
            float prevAngleDiff = Vector3.SignedAngle(transform.forward, targetDir, Vector3.forward);
            transform.rotation *= Quaternion.AngleAxis(angleSpeed * deltaTime * (int)turnDir, Vector3.forward);
            float newAngleDiff = Vector3.SignedAngle(transform.forward, targetDir, Vector3.forward);
            if (prevAngleDiff * newAngleDiff < 0)
            {
                transform.forward = targetDir;
                isTurning = false;
            }

            transform.position += speed * turnBrakeRate * deltaTime * transform.forward;
        }
    }
}