using System.Collections;
using System.Collections.Generic;
using AirTraffic.Main;
using ECS;
using UnityEngine;

public class AirplaneControlSystem : IGameSystem, IUpdateSystem
{
    private readonly CompositeDisposable disposables = new();
    private readonly MainObjectState objectState;
    private bool isStarted;

    private readonly Dictionary<Airplane, AirplaneMovement> movements = new();

    public AirplaneControlSystem(MainObjectState objectState, MessageHub messageHub)
    {
        this.objectState = objectState;
        messageHub.Event.Subscribe<OnGameStartEvent>(_ => OnGameStart()).AddTo(disposables);
        messageHub.Event.Subscribe<OnControlInputEvent>(OnControlInput).AddTo(disposables);
        
        foreach (var airplane in objectState.Airplanes.Values)
        {
            OnAddAirplane(airplane);
        }

        messageHub.Event.Subscribe<OnAddAirplaneEvent>(evt =>
        {
            var airplane = objectState.Airplanes[evt.AirplaneID];
            OnAddAirplane(airplane);
        }).AddTo(disposables);
    }

    public void Dispose()
    {
        disposables.Dispose();
    }

    public void OnUpdate()
    {
        
    }

    private void OnAddAirplane(Airplane airplane)
    {
        var movement = new AirplaneMovement(airplane);
        movements.Add(airplane, movement);

        if (isStarted)
        {
            movement.MoveStart(airplane.transform.forward);
        }
    }

    private void OnGameStart()
    {
        isStarted = true;
        
        foreach (var (airplane, movement) in movements)
        {
            movement.MoveStart(airplane.transform.forward);
        }
    }

    private void OnControlInput(OnControlInputEvent evt)
    {
        if (objectState.Airplanes.TryGetValue(evt.AirplaneID, out var airplane))
        {
            var movement = movements[airplane];
            var targetDir = Quaternion.AngleAxis(evt.TargetAngle, Vector3.back) * Vector3.up;
            movement.Turn(targetDir, evt.TurnDirection);
        }
    }
}