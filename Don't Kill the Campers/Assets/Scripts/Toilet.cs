using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toilet : RoomObject
{
    public static List<Room.Type> GetAllowedRoomTypesSub()
    {
        return new List<Room.Type> { Room.Type.Bathhouse };
    }
}
