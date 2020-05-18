using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camper : PathFollower
{
    public enum State
    {
        None,
        WaitingForPath,
        TravelingOnPath
    }

    private State state;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FindBed()
    {
        pathMan.GetPathToRoomObject<Bed>(path);
        state = State.WaitingForPath;
    }

    void CreatePath()
    {

    }
}
