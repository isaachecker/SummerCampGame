using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplePF2D;

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
    private void updateCoreDesires()
    {
        for (int i = 0; i < coreDesires.Length; i++)
        {
            coreDesires[i].IncrementAndDecrement();
        }
        //Debug.Log(bathroom.value);
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
    private void updateAncDesires()
    {
        for (int i = 0; i < ancDesires.Length; i++)
        {
            ancDesires[i].IncrementAndDecrement();
        }
    }
    #endregion

    private void updateDesires()
    {
        updateCoreDesires();
        updateAncDesires();
    }

    private Desire GetDesire(DesireType type)
    {
        switch (type)
        {
            case DesireType.bathe: return bathe;
            case DesireType.bathroom: return bathroom;
            case DesireType.drink: return drink;
            case DesireType.health: return health;
            case DesireType.eat: return eat;
        }
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

    private List<RoomObject> objectsToTarget;
    protected Room.RoomTarget roomTarget;
    protected Dictionary<Room, Path> roomPathMap;

    private ActionState _actionState;
    public ActionState actionState
    {
        get { return _actionState; }
        set
        {
            if (_actionState == ActionState.MidAction)
            {
                EndRoomTargetDesireDecrement();
            }
            _actionState = value;
            if (value == ActionState.MidAction)
            {
                roomTarget.GetInteractionPoint().StartTimeWithIP();
                StartRoomTargetDesireDecrement();
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        objectsToTarget = new List<RoomObject>();
        roomPathMap = new Dictionary<Room, Path>();
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
            case PathState.Arrived:
                ContinueArrived();
                break;
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
        if (InteractionTimeIsComplete())
        {
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
            SetPathStateNone();
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
        if (InteractionTimeIsComplete())
        {
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
            SetPathStateNone();
        }
    }
    #endregion

    #region Path State Overrides
    protected override void ContinueNoState()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            startMakeNextPath();
        }
    }

    protected override void StartWaitingForPath()
    {
    }

    protected override void ContinueWaitingForPath()
    {
        if (roomPathMap.Count > 0)
        {
            makeNextPath();
        }
    }

    protected override void ContinueTravelingOnPath()
    {
        MoveAlongPath();
    }

    protected override void StartArrived()
    {
        NormalizePosition();

        if (roomTarget.targetingRoom)
        {
            InteractionPoint IP = roomTarget.GetInteractionPoint();
            roomTarget.targetingRoom = false;
            if (IP.ShouldQueueCamper())
            {
                IP.QueueCamper(this);
                pathState = PathState.Idle;
            }
            else
            {
                IP.Lock();
                path = IP.GetPath();
                pathState = PathState.TravelingOnPath;
            }
        }
        else
        {
            pathState = PathState.UniqueAction;
        }
    }

    protected override void StartUniqueAction()
    {
        if (roomTarget == null) return;
        camperState = roomTarget.obj.GetCamperStateOnUse();
    }
    protected override void EndUniqueAction()
    {
        roomTarget.GetInteractionPoint().Unlock();
    }
    protected override void ContinueUniqueAction() { }
    #endregion

    #region Pathfinding
    private void clearPathfindingData()
    {
        if (roomTarget != null)
        {
            roomTarget.Clear();
        }
        path = null;
        objectsToTarget.Clear();
        roomPathMap.Clear();
    }

    /// <summary>
    /// Starts a path to a RoomObject that will address the next Desire
    /// </summary>
    private void makeNextPath()
    {
        if (roomPathMap.Count == 0) return;
        
        float lowestScore = float.MaxValue;
        int IPidx = -1;
        RoomObject roomObj = null;
        //get the score for each interaction point to determine the final target
        foreach (RoomObject obj in objectsToTarget)
        {
            float baseScore = roomPathMap[obj.GetRoom()].GetPathLength();
            for (int i = 0; i < obj.interactionPoints.Count; i++)
            {
                InteractionPoint ip = obj.interactionPoints[i];
                float score = baseScore + ip.GetIPScore();

                if (score < lowestScore)
                {
                    IPidx = i;
                    roomObj = obj;
                    lowestScore = score;
                }
            }
        }
        if (roomObj == null)
        {
            SetPathStateNone();
            return; //TODO better handle
        }
        if (roomTarget == null) roomTarget = new Room.RoomTarget(roomObj.GetRoom(), roomObj, IPidx);
        else roomTarget.Initialize(roomObj.GetRoom(), roomObj, IPidx);
        path = roomPathMap[roomObj.GetRoom()]; //create path to the room, not to the IP

        pathState = PathState.TravelingOnPath;
    }

    /// <summary>
    /// Based on the next DesireType, selects a RoomObject that will address that Desire
    /// </summary>
    private void startMakeNextPath()
    {
        //call this first to start clean up old path info before making new one
        clearPathfindingData();

        DesireType desireType = getDesireTypeToAddress();
        objectsToTarget = getRoomObjectsToTarget(desireType);
        List<Room> roomsToTarget = RoomObject.GetRoomsOfRoomObjects(objectsToTarget);
        pathMan.GetPathsToRooms(this, roomsToTarget);

        pathState = PathState.WaitingForPath;
    }

    /// <summary>
    /// Gets a list of the RoomObjects that will affect the current desireType
    /// </summary>
    /// <param name="desireType">The desire type that needs addressing</param>
    /// <returns>A list of RoomObjects</returns>
    private List<RoomObject> getRoomObjectsToTarget(DesireType desireType)
    {
        List<RoomObject> objs = RoomObject.GetRoomObjectsForDesireType(desireType);
        //TODO filter RoomObjects as appropriate
        return objs;
    }

    /// <summary>
    /// Gets the next desire type that should be addressed
    /// </summary>
    /// <returns>The DesireType to address</returns>
    private DesireType getDesireTypeToAddress()
    {
        Desire coreDesire = getDesireToAddress(coreDesires);
        if (coreDesire.IsWanted()) return coreDesire.GetDesireType();

        Desire ancDesire = getDesireToAddress(ancDesires);
        return ancDesire.GetDesireType();
    }

    /// <summary>
    /// Given an array of desires, gets the one of the highest value
    /// </summary>
    /// <param name="desireArr">The array of desires to pick from</param>
    /// <returns>The Desire that should next be addressed</returns>
    private Desire getDesireToAddress(Desire[] desireArr)
    {
        int largestIdx = -1;
        float largestVal = 0;
        bool wanted = false, needed = false;
        for (int i = 0; i < desireArr.Length; i++)
        {
            if (needed && !desireArr[i].IsNeeded()) continue;
            else if (wanted && !desireArr[i].IsWanted()) continue;
            wanted = desireArr[i].IsWanted();
            needed = desireArr[i].IsNeeded();
            if (desireArr[i].value >= largestVal)
            {
                largestIdx = i;
                largestVal = desireArr[i].value;
            }
        }
        return desireArr[largestIdx];
    }

    /// <summary>
    /// Start decrememnting the Desires that the roomTarget is addressing by setting their decrement speeds
    /// </summary>
    private void StartRoomTargetDesireDecrement()
    {
        RoomObject obj = roomTarget.obj;
        List<Tuple<DesireType, int>> tuples = RoomObject.GetObjectDesireImpact(obj.GetRoomObjectType());
        for (int i = 0; i < tuples.Count; i++)
        {
            Tuple<DesireType, int> tuple = tuples[i];
            Desire desire = GetDesire(tuple.Item1);
            desire.SetDecrementSpeed(tuple.Item2);
        }
    }

    /// <summary>
    /// Stop decrementing the Desires that the roomTarget is addressing
    /// </summary>
    private void EndRoomTargetDesireDecrement()
    {
        //TODO don't repeat StartRoomTargetDesireDecrement
        RoomObject obj = roomTarget.obj;
        List<Tuple<DesireType, int>> tuples = RoomObject.GetObjectDesireImpact(obj.GetRoomObjectType());
        for (int i = 0; i < tuples.Count; i++)
        {
            Tuple<DesireType, int> tuple = tuples[i];
            Desire desire = GetDesire(tuple.Item1);
            desire.SetDecrementSpeed(0);
        }
    }

    public void SetRoomPathMap(Dictionary<Room, Path> _roomPathMap)
    {
        roomPathMap = _roomPathMap;
        if (roomPathMap.Count == 0)
        {
            Debug.Log("Could not make paths");
            SetPathStateNone();
            //TODO more handling if no paths were generated
        }
    }

    public InteractionPoint GetTargetedInteractionPoint()
    {
        return roomTarget.GetInteractionPoint();
    }

    public void CreatePathToTargetedInteractionPoint()
    {
        Vector3 start = transform.position;
        Vector3 end = roomTarget.GetInteractionPoint().GetEntryPointLocation();
        CreatePath(start, end);

        pathState = PathState.WaitingForPath;
    }

    public bool InteractionTimeIsComplete()
    {
        return roomTarget.GetInteractionPoint().GetRemainingTime() <= 0;
    }
    #endregion
}
