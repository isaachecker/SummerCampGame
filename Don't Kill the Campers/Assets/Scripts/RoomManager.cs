﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<List<Room>> allRooms;
    private List<Room> cabins;
    private List<Room> bathhouses;
    private RoomEditor editor;

    void Start()
    {
        allRooms = new List<List<Room>>();
        cabins = new List<Room>();
        bathhouses = new List<Room>();
        editor = GameObject.Find("RoomEditor").GetComponent<RoomEditor>();

        allRooms.Add(cabins);
        allRooms.Add(bathhouses);
    }

    public bool AddRoom(Room room)
    {
        switch (room)
        {
            case Cabin c: return _AddRoom(cabins, c);
            case Bathhouse b: return _AddRoom(bathhouses, b);
        }
        return false;
    }

    private bool _AddRoom<T>(List<T> roomList, T room) where T : Room
    {
        if (roomList.Contains(room)) return false;
        roomList.Add(room);
        return true;
    }

    public bool RemoveRoom<T>(T room) where T : Room
    {
        List<T> roomList = GetRoomsOfType<T>();
        return _RemoveRoom(roomList, room);
    }

    private bool _RemoveRoom<T>(List<T> roomList, T room) where T : Room
    {
        if (!roomList.Contains(room)) return false;
        roomList.Remove(room);
        return true;
    }

    public List<T> GetRoomsOfType<T>() where T : Room
    {
        Type genericType = typeof(T);
        if (genericType == typeof(Cabin)) return cabins as List<T>;
        else if (genericType == typeof(Bathhouse)) return bathhouses as List<T>;
        else return null;
    }

    public List<Room> GetRoomsOfType(Room.Type type)
    {
        switch(type)
        {
            case Room.Type.Cabin: return cabins;
            case Room.Type.Bathhouse: return bathhouses;
        }
        return null;
    }

    public Room CreateRoom(Type roomType, BoundsInt bounds)
    {
        Room room = Activator.CreateInstance(roomType, bounds) as Room;
        AddRoom(room);
        return room;
    }

    public Room GetRoomWithPoint(Vector3Int point)
    {
        BoundsInt bounds;
        point.z = Controls.GetTileZ();
        foreach (List<Room> roomList in allRooms)
        {
            foreach (Room room in roomList)
            {
                bounds = room.GetBounds();
                if (bounds.Contains(point))
                {
                    return room;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Given a RoomObject, this returns a list of existing Rooms that have
    /// an open interaction point for that RoomObject type
    /// </summary>
    /// <typeparam name="T">The RoomObject object type</typeparam>
    /// <returns>A list of Rooms with the available RoomObject type</returns>
    public List<Room> GetRoomsWithAvailableObjectType<T>(RoomObjectType objType) where T : RoomObject
    {
        List<Room.Type> roomTypes = RoomObject.GetAllowedRoomTypes(objType);
        List<Room> roomList = new List<Room>();
        List<Room> returnList = new List<Room>();
        foreach (Room.Type type in roomTypes)
        {
            roomList = GetRoomsOfType(type);
            foreach (Room room in roomList)
            {
                if (room.HasAvailableObjectType<T>())
                {
                    returnList.Add(room);
                }
            }
        }
        return returnList;
    }

    public bool CanDesireBeAddressed(DesireType desireType)
    {
        //TODO more intelligently loop through rooms, i.e. only rooms that could possibly contain the desire/object type
        List<RoomObjectType> objTypes = RoomObject.GetObjectsImpactingDesireType(desireType);
        for (int i = 0; i < allRooms.Count; i++)
        {
            for (int j = 0; j < allRooms[i].Count; j++)
            {
                for (int k = 0; k < objTypes.Count; k++)
                {
                    if (allRooms[i][j].ContainsObjectOfRoomObjectType(objTypes[k])) return true;
                }   
            }
        }
        return false;
    }
}
