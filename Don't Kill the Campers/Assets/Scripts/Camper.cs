using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamperState
{
    None,
    Sleeping,
    Changing
}

public class Camper : PathFollower
{
    public enum ActionState
    {
        None,
        PreAction,
        MidAction,
        PostAction
    }

    private CamperState _camperState;
    public CamperState camperState
    {
        get { return _camperState; }
        set
        {
            if (_camperState == value) return;
            _camperState = value;
            switch(value)
            {
                case CamperState.Sleeping:
                case CamperState.Changing:
                    actionState = ActionState.PreAction;
                    break;
            }
        }
    }

    private int sleepCount = 0;

    private ActionState _actionState;
    public ActionState actionState
    {
        get { return _actionState; }
        set { _actionState = value; }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected void Update()
    {
        pathUpdate();
        camperUpdate();
    }

    private void pathUpdate()
    {
        switch (pathState)
        {
            case PathState.None:
                ContinueNoState();
                break;
            case PathState.WaitingForPath:
                ContinueWaitingForPath();
                break;
            case PathState.TravelingOnPath:
                ContinueTravelingOnPath();
                break;
            case PathState.Arrived: break;
            case PathState.UniqueAction:
                ContinueUniqueAction();
                break;
        }
    }

    private void camperUpdate()
    {
        switch(camperState)
        {
            case CamperState.Sleeping:
                SleepingState();
                break;
        }
    }

    private void SleepingState()
    {
        switch(actionState)
        {
            case ActionState.PreAction:
                PreSleepingState();
                break;
            case ActionState.MidAction:
                MidSleepingState();
                break;
            case ActionState.PostAction:
                PostSleepingState();
                break;
        }
    }

    private void PostSleepingState()
    {
        //do whatever you need to get out of bed
        Vector3 objInteractionPos = roomTarget.obj.GetInteractionPointEntryLocation(roomTarget.objectInteractionIndex);
        Vector3 direction = (objInteractionPos - transform.position).normalized;
        transform.position += (direction * 3 * Time.deltaTime);
        Vector2 delta = objInteractionPos - transform.position;
        if (delta.magnitude <= .1f)
        {
            actionState = ActionState.None;
            camperState = CamperState.None;
            pathState = PathState.None;
        }
    }

    private void MidSleepingState()
    {
        if (++sleepCount > 120)
        {
            sleepCount = 0;
            actionState = ActionState.PostAction;
        }
    }

    private void PreSleepingState()
    {
        //do whatever needed to get ready for bed
        Vector3 objInteractionPos = roomTarget.obj.GetInteractionPointLocation(roomTarget.objectInteractionIndex);
        Vector3 direction = (objInteractionPos - transform.position).normalized;
        transform.position += (direction * 3 * Time.deltaTime);
        Vector2 delta = objInteractionPos - transform.position;
        if (delta.magnitude <= .1f)
        {
            actionState = ActionState.MidAction;
        }
    }

    protected override void ContinueNoState()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CreatePathToObject<Trunk>();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            CreatePathToObject<Bed>();
        }
    }

    protected override void StartArrived()
    {
        isStationary = true;
        NormalizePosition();
        pathState = PathState.UniqueAction;
    }

    protected override void CreatePathToObject<T>()
    {
        pathState = PathState.WaitingForPath; //set this first to clear out roomTarget locks.
        //could have problem if camper cannot find anywhere to go and has to stay here
        pathMan.GetPathToRoomObject<T>(this);
    }

    protected override void StartUniqueAction()
    {
        if (roomTarget == null) return;
        camperState = roomTarget.obj.GetCamperStateOnUse();
    }
    protected override void EndUniqueAction() { }
    protected override void ContinueUniqueAction() { }
}
