using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomObjectType
{
    none,
    bed,
    toilet,
    trunk
}

public abstract class RoomObject : MonoBehaviour
{
    #region New Room Object Methods

    public static List<Room.Type> GetAllowedRoomTypes(RoomObjectType objType)
    {
        switch (objType)
        {
            case RoomObjectType.bed: return Bed.SGetAllowedRoomTypes();
            case RoomObjectType.toilet: return Toilet.SGetAllowedRoomTypes();
            case RoomObjectType.trunk: return Trunk.GetAllowedRoomTypesSub();
        }
        return new List<Room.Type>();
    }

    /// <summary>
    /// Gets a list of tuples containing the Desires and Desire Impacts that a Room Object affects
    /// </summary>
    /// <param name="type">The type of Room Object</param>
    /// <returns>a list of tuples containing the Desires and Desire Impacts that a Room Object affects</returns>
    public static List<Tuple<DesireType, int>> GetObjectDesireImpact(RoomObjectType type)
    {
        switch (type)
        {
            case RoomObjectType.bed: return Bed.SGetObjectDesireImpact();
            case RoomObjectType.toilet: return Toilet.SGetObjectDesireImpact();
        }
        return new List<Tuple<DesireType, int>>();
    }

    public static List<RoomObjectType> GetObjectsImpactingDesireType(DesireType desireType)
    {
        switch (desireType)
        {
            case DesireType.chill:
                return new List<RoomObjectType>
            {
                RoomObjectType.bed
            };
            case DesireType.bathroom:
                return new List<RoomObjectType>
            {
                RoomObjectType.toilet
            };
        }
        return new List<RoomObjectType>();
    }

    #endregion
    
    [SerializeField]
    public List<InteractionPoint> interactionPoints { get; private set; }
    [SerializeField] //Serialize so can choose in the Inspector
    protected CamperState toCamperState;
    [SerializeField]
    protected RoomObjectType type;
    protected List<Tuple<DesireType, int>> desires;
    protected Room room;

    protected static Dictionary<DesireType, List<RoomObject>> desireRoomObjectMap;

    protected virtual void Start()
    {
        if (desireRoomObjectMap == null)
        {
            desireRoomObjectMap = new Dictionary<DesireType, List<RoomObject>>();
        }
        for (int i = 0; i < desires.Count; i++)
        {
            DesireType desireType = desires[i].Item1;
            if (!desireRoomObjectMap.ContainsKey(desireType))
            {
                desireRoomObjectMap[desireType] = new List<RoomObject>();
            }
            if (desireRoomObjectMap[desireType].Contains(this)) return;
            desireRoomObjectMap[desireType].Add(this);
        }

        interactionPoints = new List<InteractionPoint>();
        InteractionPoint[] IPs = transform.GetComponentsInChildren<InteractionPoint>();
        for (int i = 0; i < IPs.Length; i++)
        {
            interactionPoints.Add(IPs[i]);
        }
    }

    public void InitializeRoomObject(Room _room)
    {
        room = _room;
        
    }

    public Room GetRoom()
    {
        return room;
    }

    public void GenerateAllIPPaths(Vector3 start)
    {
        foreach (InteractionPoint IP in interactionPoints)
        {
            IP.GeneratePathToEntryPoint(start);
        }
    }

    protected virtual void OnDestroy()
    {
        for (int i = 0; i < desires.Count; i++)
        {
            DesireType desireType = desires[i].Item1;
            if (desireRoomObjectMap[desireType].Contains(this)) desireRoomObjectMap[desireType].Remove(this);
        }
    }

    public static List<RoomObject> GetRoomObjectsForDesireType(DesireType desireType)
    {
        if (desireRoomObjectMap == null) return new List<RoomObject>();
        else if (desireRoomObjectMap[desireType] == null) return new List<RoomObject>();
        return desireRoomObjectMap[desireType];
    }

    public static List<Room> GetRoomsOfRoomObjects(List<RoomObject> objs)
    {
        List<Room> rooms = new List<Room>();
        foreach(RoomObject obj in objs)
        {
            Room room = obj.GetRoom();
            if (!rooms.Contains(room)) rooms.Add(room);
        }
        return rooms;
    }

    public bool HasOpenInteractionPoint()
    {
        foreach (InteractionPoint IP in interactionPoints)
        {
            if (!IP.IsLocked()) return true;
        }
        return false;
    }

    public int LockOpenInteractionPoint()
    {
        for (int i = 0; i < interactionPoints.Count; i++)
        {
            InteractionPoint IP = interactionPoints[i];
            if (!IP.IsLocked() && IP.Lock())
            {
                return i;
            }
        }
        return -1;
    }

    public int LockOpenInteractionPoint(ref Vector3 entryPosition)
    {
        int i;
        for (i = 0; i < interactionPoints.Count; i++)
        {
            InteractionPoint IP = interactionPoints[i];
            if (!IP.IsLocked() && IP.Lock())
            {
                break;
            }
        }
        entryPosition = GetInteractionPointEntryLocation(i);
        return i >= 0 ? i : -1;
    }

    public bool UnlockInteractionPoint(int index)
    {
        if (index >= interactionPoints.Count) return false;
        return interactionPoints[index].Unlock();
    }

    public Vector3 GetInteractionPointEntryLocation(int index)
    {
        if (index >= interactionPoints.Count || index < 0)
        {
            Debug.LogError("Bad Interaction Point input");
            return Vector3.zero;
        }
        return interactionPoints[index].GetEntryPointLocation();
    }

    public Vector3 GetInteractionPointLocation(int index)
    {
        if (index >= interactionPoints.Count || index < 0)
        {
            Debug.LogError("Bad Interaction Point input");
            return Vector3.zero;
        }
        return interactionPoints[index].transform.position;
    }

    public bool AreInteractionPointsAvailable()
    {
        foreach (InteractionPoint IP in interactionPoints)
        {
            if (!IP.IsLocked()) return true;
        }
        return false;
    }

    public CamperState GetCamperStateOnUse()
    {
        return toCamperState;
    }

    public virtual RoomObjectType GetRoomObjectType()
    {
        return RoomObjectType.none;
    }
}
