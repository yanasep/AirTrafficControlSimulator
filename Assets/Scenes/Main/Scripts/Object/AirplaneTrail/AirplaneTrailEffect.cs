using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AirTraffic.Main
{
    [RequireComponent(typeof(Airplane))]
    public class AirplaneTrailEffect : MonoBehaviour
    {
        [SerializeField] private AirplaneTrailLine trailLinePrefab;
        [SerializeField] private float trailIntervalSec = 4;
        [SerializeField] private float showDuration = 20;

        private Airplane airplane;
        private GameObjectPool<AirplaneTrailLine> trailLinePool;

        private float nextTrailCountdown;
        private int nextTrailIndex;

        private void Start()
        {
            airplane = GetComponent<Airplane>();
            trailLinePool = new GameObjectPool<AirplaneTrailLine>(trailLinePrefab, null);
            nextTrailCountdown = trailIntervalSec;
        }

        private void Update()
        {
            if (!airplane.IsMoving) return;

            nextTrailCountdown -= Time.deltaTime;
            if (nextTrailCountdown <= 0)
            {
                nextTrailCountdown = trailIntervalSec;
                ShowLineAsync().Forget();
            }
        }

        private async UniTask ShowLineAsync()
        {
            var pos = airplane.transform.position;
            var rot = Quaternion.LookRotation(Vector3.forward, airplane.transform.right);

            var line = trailLinePool.Rent();
            line.transform.SetPositionAndRotation(pos, rot);
            await line.ShowAsync(showDuration);
            if (this == null) return;
            trailLinePool.Return(line);
        }

        private void OnDestroy()
        {
            trailLinePool?.Dispose();
        }
    }
}