﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trunk : RoomObject
{
    public static List<Room.Type> GetAllowedRoomTypesSub()
    {
        return new List<Room.Type> { Room.Type.Cabin };
    }
}
