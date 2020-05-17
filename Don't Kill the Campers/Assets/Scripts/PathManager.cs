using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    private RoomManager roomMan;

    // Start is called before the first frame update
    void Start()
    {
        roomMan = GameObject.Find("RoomManager").GetComponent<RoomManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GetShortestPathToRoom<T>(Vector3 start) where T : Room
    {

    }
}
