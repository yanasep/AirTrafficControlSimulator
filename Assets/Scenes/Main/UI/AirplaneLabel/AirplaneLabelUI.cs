using System;
using UnityEngine;

namespace AirTraffic.Main.UI
{
    public class AirplaneLabelUI : MonoBehaviour
    {
        [SerializeField] private AirplaneLabel labelBase;

        private GameObjectPool<AirplaneLabel> labelPool;

        private void Awake()
        {
            labelBase.gameObject.SetActive(false);
            labelPool = new GameObjectPool<AirplaneLabel>(labelBase, transform);
        }

        public void AddLabel(Transform followTransformWorld, string airplaneName)
        {
            var label = labelPool.Rent();
            label.SetName(airplaneName);
            label.SetFollow(followTransformWorld);
        }
    }
}