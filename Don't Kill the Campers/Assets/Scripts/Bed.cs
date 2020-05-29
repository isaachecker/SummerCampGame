using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bed : RoomObject
{
    protected override void Start()
    {
        desires = SGetObjectDesireImpact();
        base.Start();
    }

    public static List<Room.Type> SGetAllowedRoomTypes()
    {
        return new List<Room.Type> { Room.Type.Cabin };
    }

    public static List<Tuple<DesireType, int>> SGetObjectDesireImpact()
    {
        return new List<Tuple<DesireType, int>>
        {
            new Tuple<DesireType, int>(DesireType.chill, 3) //using this for testing
        };
    }

    public override RoomObjectType GetRoomObjectType()
    {
        return RoomObjectType.bed;
    }
}
