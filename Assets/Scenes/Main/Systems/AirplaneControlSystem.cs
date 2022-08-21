using System.Collections;
using System.Collections.Generic;
using AirTraffic.Main;
using ECS;
using UnityEngine;

public class AirplaneControlSystem : IGameSystem
{
    private readonly CompositeDisposable disposables = new();
    private readonly MainObjectState objectState;

    public AirplaneControlSystem(MainObjectState objectState, MessageHub messageHub)
    {
        this.objectState = objectState;
        messageHub.Event.Subscribe<OnGameStartEvent>(_ => OnGameStart()).AddTo(disposables);
        messageHub.Event.Subscribe<OnControlInputEvent>(OnControlInput).AddTo(disposables);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }

    private void OnGameStart()
    {
        foreach (var airplane in objectState.Airplanes.Values)
        {
            airplane.MoveStart(airplane.transform.forward);
        }
    }

    private void OnControlInput(OnControlInputEvent evt)
    {
        if (objectState.Airplanes.TryGetValue(evt.AirplaneID, out var airplane))
        {
            var targetDir = Quaternion.AngleAxis(evt.TargetAngle, Vector3.back) * Vector3.up;
            airplane.Turn(targetDir, evt.TurnDirection);
        }
    }
}