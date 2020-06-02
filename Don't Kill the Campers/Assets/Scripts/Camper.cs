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
    Toilet,
    Showering
}

public class Camper : PathFollower
{
    #region Desires
    DesireType nextDesireType;
    #region Core Desires
    const int numCoreDesires = 5;
    Desire bathroom, eat, drink, health, bathe;
    [SerializeField]
    Desire[] coreDesires = new Desire[numCoreDesires];

    private void initializeCoreDesires()
    {
        eat = new Desire(DesireType.eat, 1);
        bathe = new Desire(DesireType.bathe, 2);
        drink = new Desire(DesireType.drink, 1);
        health = new Desire(DesireType.health, 1);
        bathroom = new Desire(DesireType.bathroom, 2.1f);
        
        coreDesires = new Desire[numCoreDesires] { eat, drink, health, bathe, bathroom };
    }
    private void updateCoreDesires()
    {
        for (int i = 0; i < coreDesires.Length; i++)
        {
            coreDesires[i].IncrementAndDecrement();
        }
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
                case CamperState.Showering:
                    break;
            }
        }
    }

    private List<RoomObject> objectsToTarget;
    protected Room.RoomTarget roomTarget;
    protected Dictionary<Room, Path> roomPathMap;
    protected Dictionary<InteractionPoint, Path> IPpathMap;
    protected RoomManager roomMan;

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
        IPpathMap = new Dictionary<InteractionPoint, Path>();
        roomMan = GameObject.Find("RoomManager").GetComponent<RoomManager>();
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
            case CamperState.Showering:
                ShoweringState();
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
            startMakeNextPath(true);
        }
    }
    #endregion

    #region Showering
    private void ShoweringState()
    {
        switch (actionState)
        {
            case ActionState.PreAction:
                PreShoweringState();
                break;
            case ActionState.MidAction:
                MidShoweringState();
                break;
            case ActionState.PostAction:
                PostShoweringState();
                break;
        }
    }

    private void PreShoweringState()
    {
        Vector3 objInteractionPos = roomTarget.obj.GetInteractionPointLocation(roomTarget.objectInteractionIndex);
        Vector3 direction = (objInteractionPos - transform.position).normalized;
        transform.position += (direction * 3 * Time.deltaTime);
        Vector2 delta = objInteractionPos - transform.position;
        if (delta.magnitude < 0.1f)
        {
            actionState = ActionState.MidAction;
        }
    }

    private void MidShoweringState()
    {
        if (InteractionTimeIsComplete())
        {
            actionState = ActionState.PostAction;
        }
    }

    private void PostShoweringState()
    {
        //do whatever you need to get off the toilet
        Vector3 objInteractionPos = roomTarget.obj.GetInteractionPointEntryLocation(roomTarget.objectInteractionIndex);
        Vector3 direction = (objInteractionPos - transform.position).normalized;
        transform.position += (direction * 3 * Time.deltaTime);
        Vector2 delta = objInteractionPos - transform.position;
        if (delta.magnitude < 0.1f)
        {
            actionState = ActionState.None;
            camperState = CamperState.None;
            startMakeNextPath(true);
        }
    }
    #endregion

    #region Path State Overrides
    protected override void ContinueNoState()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            startMakeNextPath(true);
        }
    }

    protected override void StartWaitingForPath()
    {
    }

    protected override void ContinueWaitingForPath()
    {
        if (roomPathMap.Count > 0 || IPpathMap.Count > 0)
        {
            pickNextPath();
        }
        else if (path != null && path.IsGenerated())
        {
            pathState = PathState.TravelingOnPath;
        }
    }

    protected override void ContinueTravelingOnPath()
    {
        MoveAlongPath();
    }

    protected override void StartArrived()
    {
        NormalizePosition();

        InteractionPoint IP = roomTarget.GetInteractionPoint();
        IP.RemoveCamperEnRoute(this);
        if (IP.Lock(this))
        {
            pathState = PathState.UniqueAction;
        }
        else if (IP.ShouldQueueCamper())
        {
            IP.QueueCamper(this);
            pathState = PathState.Idle;
        }
        else if (IP.Lock(this))
        {
            pathState = PathState.UniqueAction;
        }
        else
        {
            pathState = PathState.None;
        }
    }

    protected override void StartUniqueAction()
    {
        if (roomTarget == null) return;
        camperState = roomTarget.obj.GetCamperStateOnUse();
    }
    protected override void EndUniqueAction()
    {
        if (roomTarget != null && !roomTarget.isCleared && roomTarget.GetInteractionPoint().IsLocked())
        {
            roomTarget.GetInteractionPoint().Unlock(this);
        }
    }
    protected override void ContinueUniqueAction() { }
    #endregion

    #region Pathfinding
    /// <summary>
    /// Clears out pathfinding data
    /// </summary>
    private void clearPathfindingData()
    {
        if (roomTarget != null)
        {
            roomTarget.GetInteractionPoint().Unlock(this);
            roomTarget.Clear();
        }
        if (objectsToTarget != null)
        {
            //set to null, do not clear since this references a static list
            objectsToTarget = null; 
        }
        path.Reset();
        clearPathMaps();
    }

    /// <summary>
    /// Clears the roomPathMap and IPpathMap
    /// </summary>
    private void clearPathMaps()
    {
        roomPathMap.Clear();
        IPpathMap.Clear();
    }

    /// <summary>
    /// Goes through all possible paths and picks the one with the lowest score to address the next desire.
    /// </summary>
    private void pickNextPath()
    {
        if (roomPathMap.Count == 0 && IPpathMap.Count == 0) return;

        float lowestScore = float.MaxValue;
        int IPidx = -1;
        RoomObject targetObj = null;
        bool useIPmap = false;
        //get the score for each interaction point to determine the final target
        foreach (RoomObject obj in objectsToTarget)
        {
            Room room = obj.GetRoom();
            //The camper is not in the same room as this object
            if (roomPathMap.ContainsKey(room))
            {
                if (getLowestRoomPathScore(room, obj, ref lowestScore, ref IPidx, ref targetObj))
                {
                    useIPmap = false;
                }
            }
            else  //the camper is in the same room as this object
            {
                if (getLowestIPpathScore(obj, ref lowestScore, ref IPidx, ref targetObj))
                {
                    useIPmap = true;
                }
            }
        }
        if (targetObj == null || IPidx == -1)
        {
            SetPathStateNone();
            return; //TODO better handle
        }

        //set up the roomTarget with the lowest scoring path
        if (roomTarget == null) roomTarget = new Room.RoomTarget(targetObj.GetRoom(), targetObj, IPidx);
        else roomTarget.Initialize(targetObj.GetRoom(), targetObj, IPidx);

        //create a path to the room, not the IP
        if (useIPmap)
        {
            path = IPpathMap[targetObj.interactionPoints[IPidx]];
            roomTarget.GetInteractionPoint().AddCamperEnRoute(this);
        }
        //create a path to the IP
        else
        {
            path = roomPathMap[targetObj.GetRoom()];
            //append the IP path so we go straight to that
            path.AppendPathToEnd(roomTarget.GetInteractionPoint().GetPath());
            roomTarget.GetInteractionPoint().AddCamperEnRoute(this);
        }

        pathState = PathState.TravelingOnPath;
    }

    /// <summary>
    /// For all IPs of an object in a room, calculate their travel score. Set values based on the lowest score.
    /// </summary>
    /// <param name="room">The room the object is in</param>
    /// <param name="obj">The object containing the IPs</param>
    /// <param name="lowestScore">Outputting the lowest score</param>
    /// <param name="IPidx">Outputting the IP index of the IP with the lowest score on the obj</param>
    /// <param name="targetObj">Outputting the Room Object with the lowest score</param>
    /// <returns>True if a room path has the lowest score. Flase otherwise.</returns>
    private bool getLowestRoomPathScore(Room room, RoomObject obj, ref float lowestScore, ref int IPidx, ref RoomObject targetObj)
    {
        //get the path size from the camper to the room's entrance
        float baseScore = roomPathMap[room].GetPathLength();
        bool useRoomMap = false;
        for (int i = 0; i < obj.interactionPoints.Count; i++)
        {
            //for each interaction point, get the path length from the IP to the room entrance + wait time
            InteractionPoint ip = obj.interactionPoints[i];
            float score = baseScore + ip.GetIPScore();

            if (score < lowestScore)
            {
                useRoomMap = true;
                IPidx = i;
                targetObj = obj;
                lowestScore = score;
            }
        }
        return useRoomMap;
    }

    /// <summary>
    /// For all IPs of an object, calculate their travel score. Set values based on the lowest score.
    /// We assume the camper is already in the same room as this Room Object.
    /// </summary>
    /// <param name="obj">The Room Object</param>
    /// <param name="lowestScore">Outputting the lowest score so far</param>
    /// <param name="IPidx">Outputting the IP index of the IP with the lowest score</param>
    /// <param name="targetObj">Outputting the Room Object with the lowest score</param>
    /// <returns>True if an IP of a room the camper is alreayd in has the lowest score</returns>
    private bool getLowestIPpathScore(RoomObject obj, ref float lowestScore, ref int IPidx, ref RoomObject targetObj)
    {
        bool useIPmap = false;
        for (int i = 0; i < obj.interactionPoints.Count; i++)
        {
            //we expect that we have calculated a path to each Interaction Point
            InteractionPoint IP = obj.interactionPoints[i];
            if (!IPpathMap.ContainsKey(IP)) continue;

            //score is the length of the path to the IP + the wait time of that IP
            float score = IPpathMap[IP].GetPathLength();
            score += IP.GetWaitScore();

            if (score < lowestScore)
            {
                useIPmap = true;
                IPidx = i;
                targetObj = obj;
                lowestScore = score;
            }
        }
        return useIPmap;
    }

    /// <summary>
    /// Start recalculating paths to something to address the current desire type
    /// </summary>
    public void RecalculatePotentialPaths()
    {
        startMakeNextPath(false);
    }

    /// <summary>
    /// Based on the next DesireType, selects a RoomObject that will address that Desire
    /// </summary>
    /// <param name="selectNewDesire">True if you want to select a new desire type to address.
    ///                               False to use last desire type</param>
    private void startMakeNextPath(bool selectNewDesire = true)
    {
        //call this first to start clean up old path info before making new one
        clearPathfindingData();

        //Get next desire type to address
        if (selectNewDesire || nextDesireType == DesireType.none)
        {
            nextDesireType = getDesireTypeToAddress();
        }

        //Get objects that can address this desire type
        objectsToTarget = getRoomObjectsToTarget(nextDesireType);

        //Get the rooms that house those objects
        List<Room> roomsToTarget = RoomObject.GetRoomsOfRoomObjects(objectsToTarget);
        Room currentRoom = roomMan.GetRoomWithPoint(Controls.GetPosAsVector3Int(transform));

        //If we are already in one of the rooms with those objects, remove them from the room list,
        //and add the object to the object list
        List<RoomObject> objectsToTargetInCurrentRoom = new List<RoomObject>();
        if (currentRoom != null && roomsToTarget.Contains(currentRoom))
        {
            roomsToTarget.Remove(currentRoom);
            foreach (RoomObject obj in objectsToTarget)
            {
                if (obj.GetRoom() == currentRoom) objectsToTargetInCurrentRoom.Add(obj);
            }
        }

        //Create paths to other rooms and to the objects in the current room
        pathMan.GetPathsToRoomsAndObjects(this, roomsToTarget, objectsToTargetInCurrentRoom);

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

    private DesireType getDesireTypeToAddress()
    {
        LinkedList<Desire> desiresSorted = getDesireSortedList();
        LinkedListNode <Desire> desireNode = desiresSorted.First;
        while (desireNode != null)
        {
            DesireType type = desireNode.Value.GetDesireType();
            List<RoomObject> objsToTarget = getRoomObjectsToTarget(type);
            if (objsToTarget.Count > 0) return type;
            else
            {
                Debug.Log("Could not address desire: " + type.ToString());
                desireNode = desireNode.Next;
            }
        }
        return DesireType.none;
    }

    private LinkedList<Desire> getDesireSortedList()
    {
        LinkedList<Desire> desiresSorted = new LinkedList<Desire>();
        foreach (Desire desire in coreDesires)
        {
            AddDesireToLinkedList(desire, ref desiresSorted);
        }
        foreach (Desire desire in ancDesires)
        {
            AddDesireToLinkedList(desire, ref desiresSorted);
        }
        return desiresSorted;
    }

    private void AddDesireToLinkedList(Desire desire, ref LinkedList<Desire> desiresSorted)
    {
        if (desiresSorted.Count == 0) desiresSorted.AddFirst(desire);
        else
        {
            LinkedListNode<Desire> nextDesire = desiresSorted.First;
            while (nextDesire != null)
            {
                if (desire.value > nextDesire.Value.value)
                {
                    desiresSorted.AddBefore(nextDesire, desire);
                    break;
                }
                nextDesire = nextDesire.Next;
            }
            if (nextDesire == null)
            {
                desiresSorted.AddLast(desire);
            }
        }
    }

    /// <summary>
    /// Given an array of desires, gets the one of the highest value
    /// </summary>
    /// <param name="desireArr">The array of desires to pick from</param>
    /// <returns>The Desire that should next be addressed</returns>
    private Desire getDesireToAddress(Desire[] desireArr, List<DesireType> doNotSelect)
    {
        int largestIdx = -1;
        float largestVal = 0;
        bool wanted = false, needed = false;
        for (int i = 0; i < desireArr.Length; i++)
        {
            Desire desire = desireArr[i];
            if (doNotSelect.Contains(desire.GetDesireType())) continue;
            else if (needed && !desire.IsNeeded()) continue;
            else if (wanted && !desire.IsWanted()) continue;
            wanted = desire.IsWanted();
            needed = desire.IsNeeded();
            if (desire.value >= largestVal)
            {
                largestIdx = i;
                largestVal = desire.value;
            }
        }
        if (largestIdx == -1) return null;
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

    public void SetPathMaps(Dictionary<Room, Path> _roomPathMap, Dictionary<InteractionPoint, Path> _IPpathMap)
    {
        roomPathMap = _roomPathMap;
        IPpathMap = _IPpathMap;
        if (roomPathMap.Count == 0 && IPpathMap.Count == 0)
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
        //clear out so other IPs are not inadvertently targeted
        clearPathMaps();

        if (roomTarget != null && !roomTarget.GetInteractionPoint().Lock(this))
        {
            return;
        }

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
