using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<List<Room>> allRooms;
    private List<Room> cabins;
    private RoomEditor editor;

    void Start()
    {
        allRooms = new List<List<Room>>();
        cabins = new List<Room>();
        editor = GameObject.Find("RoomEditor").GetComponent<RoomEditor>();

        allRooms.Add(cabins);
    }

    public bool AddRoom(Room room)
    {
        switch (room)
        {
            case Cabin c: return _AddRoom(cabins, c);
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
        else return null;
    }

    public List<Room> GetRoomsOfType(Room.Type type)
    {
        switch(type)
        {
            case Room.Type.Cabin: return cabins;
        }
        return null;
    }

    public T CreateRoom<T>(BoundsInt bounds) where T : Room
    {
        Debug.Log(typeof(T));
        T room = Activator.CreateInstance(typeof(T), bounds) as T;
        AddRoom(room);
        return room;
    }

    public Room GetRoomWithPoint(Vector3Int point)
    {
        BoundsInt bounds;
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

    public List<Room> GetRoomsWithAvailableObjectType<T>() where T : RoomObject
    {
        List<Room.Type> roomTypes = RoomObject.GetAllowedRoomTypes<T>();
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
}
