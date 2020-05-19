using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    public class RoomTarget
    {
        public Room room;
        public RoomObject obj;
        public int objectInteractionIndex;

        public RoomTarget(Room _room, RoomObject _obj, int OII)
        {
            room = _room;
            obj = _obj;
            objectInteractionIndex = OII;
        }
    }

    public enum Type
    {
        Cabin,
        ShowerHouse
    }

    private BoundsInt bounds;
    public Vector3Int doorPos { get; private set; }
    protected bool needsDoor;
    public string ID { get; private set; }

    private List<RoomObject> objectList;

    public Room(BoundsInt _bounds)
    {
        ID = Controls.MakeRandomID(6);
        bounds = _bounds;
        objectList = new List<RoomObject>();
    }

    public void AddDoor(Vector3Int newDoorPosition)
    {
        doorPos = newDoorPosition;
    }

    public BoundsInt GetBounds()
    {
        return bounds;
    }

    public bool AddRoomObject(RoomObject obj)
    {
        if (objectList.Contains(obj)) return false;
        objectList.Add(obj);
        return true;
    }

    public bool RemoveRoomObject(RoomObject obj)
    {
        if (!objectList.Contains(obj)) return false;
        objectList.Remove(obj);
        return true;
    }

    public bool CanCamperEnterRoom(Camper camper)
    {
        return true;
    }

    public bool HasAvailableObjectType<T>() where T : RoomObject
    {
        foreach (RoomObject obj in objectList)
        {
            if (obj.GetType() == typeof(T) && obj.AreInteractionPointsAvailable())
            {
                return true;
            }
        }
        return false;
    }

    public int LockAvailableObjectType<T>()
    {
        int indexVal = -1;
        foreach (RoomObject obj in objectList)
        {
            if (obj.GetType() == typeof(T) && obj.AreInteractionPointsAvailable())
            {
                indexVal = obj.LockOpenInteractionPoint();
                if (indexVal > -1) break;
            }
        }
        return indexVal;
    }

    public RoomObject LockAvailablePointOnObjectType(System.Type t, ref int lockIndex)
    {
        lockIndex = -1;
        foreach (RoomObject obj in objectList)
        {
            if (obj.GetType() == t && obj.AreInteractionPointsAvailable())
            {
                lockIndex = obj.LockOpenInteractionPoint();
                if (lockIndex > -1)
                {
                    return obj;
                }
            }
        }
        return null;
    }
}
