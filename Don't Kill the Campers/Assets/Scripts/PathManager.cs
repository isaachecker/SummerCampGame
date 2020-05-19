using System;
using System.Collections;
using System.Collections.Generic;
using SimplePF2D;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    private class MultiRoomTargetPathFinder
    {
        Path[] paths;
        PathFollower pathFollower;
        Type roomObjectType;
        List<Room> rooms;
        Vector3 startPoint;
        SimplePathFinding2D simpPF2D;

        //Constructor
        public MultiRoomTargetPathFinder(PathFollower _pathFollower, List<Room> roomList, Type t)
        {
            pathFollower = _pathFollower;
            startPoint = pathFollower.transform.position;
            rooms = roomList;
            roomObjectType = t;

            simpPF2D = GameObject.Find("Grid").GetComponent<SimplePathFinding2D>();
            paths = new Path[roomList.Count];
            for (int i = 0; i < roomList.Count; i++)
            {
                paths[i] = new Path(simpPF2D);
            }
        }

        /// <summary>
        /// Starts calculating all paths in paths
        /// </summary>
        public void CalculatePaths()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                Vector3 doorPos = rooms[i].doorPos;
                if (doorPos != Vector3.zero && doorPos != null)
                {
                    paths[i].CreatePath(startPoint, rooms[i].doorPos);
                }
            }
        }

        /// <summary>
        /// Checks if all the paths trying to generate have generated
        /// </summary>
        /// <returns>True if all paths are generated. False otherwise</returns>
        public bool PathsAreGenerated()
        {
            foreach(Path path in paths)
            {
                if (!path.IsGenerated()) return false;
            }
            return true;
        }

        /// <summary>
        /// Will find the shortest path of all paths generated, and assign that
        /// path and its related room and room object to the path follower.
        /// </summary>
        public void SetShortestPathData()
        {
            float shortestPath = float.MaxValue;
            float length;
            int indexOfShortest = 0, lockIndex = -1;
            for (int i = 0; i < paths.Length; i++)
            {
                length = paths[i].GetPathLength();
                if (length < shortestPath)
                {
                    shortestPath = length;
                    indexOfShortest = i;
                }
            }
            pathFollower.SetPath(paths[indexOfShortest]);

            Room nearestRoom = rooms[indexOfShortest];
            RoomObject obj = nearestRoom.LockAvailablePointOnObjectType(roomObjectType, ref lockIndex);
            Room.RoomTarget roomTarget = new Room.RoomTarget(nearestRoom, obj, lockIndex);
            pathFollower.SetRoomTarget(roomTarget);
        }
    }

    private RoomManager roomMan;
    private List<MultiRoomTargetPathFinder> multiRoomPathFinders;

    // Start is called before the first frame update
    void Start()
    {
        roomMan = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        multiRoomPathFinders = new List<MultiRoomTargetPathFinder>();
    }

    // Update is called once per frame
    void Update()
    {
        List<MultiRoomTargetPathFinder> toRemove = new List<MultiRoomTargetPathFinder>();
        foreach (MultiRoomTargetPathFinder MRPF in multiRoomPathFinders)
        {
            if (MRPF.PathsAreGenerated())
            {
                MRPF.SetShortestPathData();
                toRemove.Add(MRPF);
            }
        }
        foreach(MultiRoomTargetPathFinder MRPF in toRemove)
        {
            multiRoomPathFinders.Remove(MRPF);
        }
    }

    /// <summary>
    /// Passed a PathFollower and given a generic RoomObject type, this will find and assign
    /// the path to a room object of that type in the nearest room that has one available.
    /// </summary>
    /// <typeparam name="T">RoomObject type</typeparam>
    /// <param name="pathFollower">PathFollower object that will receive the shortest path</param>
    public void GetPathToRoomObject<T>(PathFollower pathFollower) where T : RoomObject
    {
        List<Room> availableRoomList = roomMan.GetRoomsWithAvailableObjectType<T>();
        MultiRoomTargetPathFinder MRPF =
            new MultiRoomTargetPathFinder(pathFollower, availableRoomList, typeof(T));
        MRPF.CalculatePaths();
        multiRoomPathFinders.Add(MRPF);
    }
}
