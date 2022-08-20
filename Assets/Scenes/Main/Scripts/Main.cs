using System;
using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

namespace AirTraffic.Main
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private Airplane airplane;
        
        private MainObjectState mainObjectState;
        private MessageHub messageHub;
        private MasterSystem masterSystem;
        
        private void Start()
        {
            messageHub = new MessageHub();
            mainObjectState = new MainObjectState(messageHub);
            masterSystem = new MasterSystem(
                new AirplaneControlSystem(mainObjectState, messageHub)
            );

            mainObjectState.AddAirplane(airplane);
        }

        private void Update()
        {
            masterSystem.Update();
        }

        private void OnDestroy()
        {
            masterSystem.Dispose();
        }
    }
}