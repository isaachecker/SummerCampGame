using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
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
}
