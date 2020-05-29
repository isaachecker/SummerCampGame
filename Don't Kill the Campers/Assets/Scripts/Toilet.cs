using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toilet : RoomObject
{
    protected override void Start()
    {
        desires = SGetObjectDesireImpact();
        base.Start();
    }

    public static List<Room.Type> SGetAllowedRoomTypes()
    {
        return new List<Room.Type> { Room.Type.Bathhouse };
    }

    public static List<Tuple<DesireType, int>> SGetObjectDesireImpact()
    {
        return new List<Tuple<DesireType, int>>
        {
            new Tuple<DesireType, int>(DesireType.bathroom, 3)
        };
    }

    public override RoomObjectType GetRoomObjectType()
    {
        return RoomObjectType.toilet;
    }
}
