using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    private BoundsInt bounds;
    private Vector3Int doorPos;
    private Tilemap tilemap;

    public void Initialize(Tilemap _tilemap, BoundsInt _bounds)
    {
        tilemap = _tilemap;
        bounds = _bounds;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void AddDoor(Vector3Int newDoorPosition)
    {
        doorPos = newDoorPosition;
    }

}
