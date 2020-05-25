using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camper : PathFollower
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch (state)
        {
            case State.None:
                ContinueNoState();
                break;
            case State.WaitingForPath:
                ContinueWaitingForPath();
                break;
            case State.TravelingOnPath:
                ContinueTravelingOnPath();
                break;
        }
    }

    protected override void ContinueNoState()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CreatePathToObject<Trunk>();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            CreatePathToObject<Bed>();
        }
    }

    protected override void CreatePathToObject<T>()
    {
        state = State.WaitingForPath; //set this first to clear out roomTarget locks.
        //could have problem if camper cannot find anywhere to go and has to stay here
        pathMan.GetPathToRoomObject<T>(this);
    }
}
