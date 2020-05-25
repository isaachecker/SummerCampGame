using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamperState
{
    None,
    Sleeping,
    Changing,
    Toilet
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
            actionState = ActionState.PreAction;
            switch (value)
            {
                case CamperState.Sleeping:
                case CamperState.Changing:
                case CamperState.Toilet:
                    break;
            }
        }
    }

    private int sleepCount = 0, toiletCount = 0;

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
            case CamperState.Changing:
                break;
            case CamperState.Toilet:
                ToiletState();
                break;
        }
    }

    #region Sleeping
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

    private void MidSleepingState()
    {
        if (++sleepCount > 500)
        {
            sleepCount = 0;
            actionState = ActionState.PostAction;
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
    #endregion

    #region Toilet
    private void ToiletState()
    {
        switch (actionState)
        {
            case ActionState.PreAction:
                PreToiletState();
                break;
            case ActionState.MidAction:
                MidToiletState();
                break;
            case ActionState.PostAction:
                PostToiletState();
                break;
        }
    }

    private void PreToiletState()
    {
        Vector3 objInteractionPos = roomTarget.obj.GetInteractionPointLocation(roomTarget.objectInteractionIndex);
        Vector3 direction = (objInteractionPos - transform.position).normalized;
        transform.position += (direction * 3 * Time.deltaTime);
        Vector2 delta = objInteractionPos - transform.position;
        if (delta.magnitude <= .1f)
        {
            actionState = ActionState.MidAction;
        }
    }

    private void MidToiletState()
    {
        if (++toiletCount > 500)
        {
            toiletCount = 0;
            actionState = ActionState.PostAction;
        }
    }

    private void PostToiletState()
    {
        //do whatever you need to get off the toilet
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
    #endregion

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
        else if(Input.GetKeyDown(KeyCode.M))
        {
            CreatePathToObject<Toilet>();
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
