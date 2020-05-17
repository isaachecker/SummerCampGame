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
    private Vector3Int doorPos;

    private List<RoomObject> objectList;

    public Room(BoundsInt _bounds)
    {
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
}
