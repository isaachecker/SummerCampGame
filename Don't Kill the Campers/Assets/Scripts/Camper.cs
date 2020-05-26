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
    #region Desires
    #region Core Desires
    const int numCoreDesires = 5;
    Desire bathroom, eat, drink, health, bathe;
    [SerializeField]
    Desire[] coreDesires = new Desire[numCoreDesires];

    private void initializeCoreDesires()
    {
        eat = new Desire(DesireType.eat, 1);
        bathe = new Desire(DesireType.bathe, 1);
        drink = new Desire(DesireType.drink, 1);
        health = new Desire(DesireType.health, 1);
        bathroom = new Desire(DesireType.bathroom, 1);
        
        coreDesires = new Desire[numCoreDesires] { eat, drink, health, bathe, bathroom };
    }
    #endregion

    #region Ancillary Desires
    //Ancillary desires
    const int numAncDesires = 4;
    [SerializeField]
    Desire[] ancDesires = new Desire[numAncDesires];

    private int chillDesireIdx = -1, musicDesireIdx = -1, communityDesireIdx = -1;
    private int natureDesireIdx = -1, exerciseDesireIdx = -1, creativeDesireIdx = -1;
    private int scienceDesireIdx = -1, educationDesireIdx = -1, spiritualityDesireIdx = -1;

    private void initializeAncDesires()
    {
        List<DesireType> desireTypes = Desire.GetRandomAncDesireTypes(numAncDesires);
        for (int i = 0; i < desireTypes.Count; i++)
        {
            DesireType type = desireTypes[i];
            switch (type)
            {
                case DesireType.chill: chillDesireIdx = i; break;
                case DesireType.music: musicDesireIdx = i; break;
                case DesireType.nature: natureDesireIdx = i; break;
                case DesireType.science: scienceDesireIdx = i; break;
                case DesireType.exercise: exerciseDesireIdx = i; break;
                case DesireType.creative: creativeDesireIdx = i; break;
                case DesireType.community: communityDesireIdx = i; break;
                case DesireType.education: educationDesireIdx = i; break;
                case DesireType.spirituality: spiritualityDesireIdx = i; break;
            }
            ancDesires[i] = new Desire(type, 1);
        }
    }
    #endregion
    private void updateCoreDesires()
    {
        for (int i = 0; i < coreDesires.Length; i++)
        {
            coreDesires[i].IncrementAndDecrement();
        }
    }
    private void updateAncDesires()
    {
        for (int i = 0; i < ancDesires.Length; i++)
        {
            ancDesires[i].IncrementAndDecrement();
        }
    }
    private void updateDesires()
    {
        updateCoreDesires();
        updateAncDesires();
    }

    private Desire GetAncDesire(DesireType type)
    {
        int idx = -1;
        switch (type)
        {
            case DesireType.chill: idx = chillDesireIdx; break;
            case DesireType.music: idx = musicDesireIdx; break;
            case DesireType.community: idx = communityDesireIdx; break;
            case DesireType.nature: idx = natureDesireIdx; break;
            case DesireType.exercise: idx = exerciseDesireIdx; break;
            case DesireType.creative: idx = creativeDesireIdx; break;
            case DesireType.science: idx = scienceDesireIdx; break;
            case DesireType.education: idx = educationDesireIdx; break;
            case DesireType.spirituality: idx = spiritualityDesireIdx; break;
        }
        return idx > -1 ? ancDesires[idx] : null;
    }
    #endregion

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
        initializeCoreDesires();
        initializeAncDesires();
    }

    protected void Update()
    {
        pathUpdate();
        updateDesires();
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
