using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    private BoundsInt bounds;
    private Vector3Int doorPos;

    private List<RoomObject> objectList;

    public void Initialize(BoundsInt _bounds)
    {
        bounds = _bounds;
    }

    // Start is called before the first frame update
    void Start()
    {
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

    public void OnDestroy()
    {
        //Destroy inside objects
    }
}
