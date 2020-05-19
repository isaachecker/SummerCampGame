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
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.P))
        {
            CreatePathToObject<Bed>();
        }
    }

    void CreatePathToObject<T>() where T : RoomObject
    {
        pathMan.GetPathToRoomObject<T>(this);
        state = State.WaitingForPath;
    }
}
