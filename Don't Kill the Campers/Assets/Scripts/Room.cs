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
        public bool targetingRoom;
        private bool isCleared;

        public RoomTarget(Room _room, RoomObject _obj, int OII)
        {
            Initialize(_room, _obj, OII);
        }

        public void Initialize(Room _room, RoomObject _obj, int OII)
        {
            room = _room;
            obj = _obj;
            objectInteractionIndex = OII;
            targetingRoom = true;
            isCleared = false;
        }

        public InteractionPoint GetInteractionPoint()
        {
            if (isCleared) return null;
            return obj.interactionPoints[objectInteractionIndex];
        }

        public void Clear()
        {
            room = null;
            obj = null;
            objectInteractionIndex = -1;
            isCleared = true;
        }
    }

    public enum Type
    {
        Cabin,
        Bathhouse
    }

    private BoundsInt bounds;
    public Vector3Int doorPos { get; private set; }
    protected bool needsDoor;

    private List<RoomObject> objectList;

    public Room(BoundsInt _bounds)
    {
        bounds = _bounds;
        objectList = new List<RoomObject>();
    }

    public void GenerateAllRoomObjectIPPaths()
    {
        foreach(RoomObject obj in objectList)
        {
            obj.GenerateAllIPPaths(doorPos);
        }
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

    public bool ContainsObjectOfRoomObjectType(RoomObjectType objType)
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            if (objectList[i].GetRoomObjectType() == objType) return true;
        }
        return false;
    }
}
