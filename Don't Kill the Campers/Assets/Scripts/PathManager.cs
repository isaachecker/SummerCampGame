using System.Collections;
using System.Collections.Generic;
using SimplePF2D;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    private class RoomPath
    {
        public Room room;
        public Path path;
        public RoomPath(Room _room, Path _path)
        {
            room = _room;
            path = _path;
        }
    }
    private class MultiRoomPathFinder
    {
        Path[] paths;
        List<Room> rooms;
        Vector3 startPoint;
        SimplePathFinding2D simpPF2D;

        //Constructor
        public MultiRoomPathFinder(Vector3 start, List<Room> roomList)
        {
            paths = new Path[roomList.Count];
            simpPF2D = GameObject.Find("Grid").GetComponent<SimplePathFinding2D>();
            startPoint = start;
            rooms = roomList;
            for (int i = 0; i < roomList.Count; i++)
            {
                Path path = new Path(simpPF2D);
                paths[i] = path;
            }
        }

        public void CalculatePaths()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i].CreatePath(startPoint, rooms[i].doorPos);
            }
        }

        public bool PathsAreGenerated()
        {
            foreach(Path path in paths)
            {
                if (!path.IsGenerated()) return false;
            }
            return true;
        }

        public RoomPath GetRoomWithShortestPath()
        {
            float shortestPath = float.MaxValue;
            float length;
            int indexOfShortest = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                length = paths[i].GetPathLength();
                if (length < shortestPath)
                {
                    shortestPath = length;
                    indexOfShortest = i;
                }
            }
            return new RoomPath(rooms[indexOfShortest], paths[indexOfShortest]);
        }
    }

    private RoomManager roomMan;
    private List<MultiRoomPathFinder> multiRoomPathFinders;

    // Start is called before the first frame update
    void Start()
    {
        roomMan = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        multiRoomPathFinders = new List<MultiRoomPathFinder>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (MultiRoomPathFinder MRPF in multiRoomPathFinders)
        {
            if (MRPF.PathsAreGenerated())
            {
                //
            }
        }
    }

    private void GetShortestPathToRoom<T>(Vector3 start) where T : Room
    {

    }

    public void GetPathToRoomObject<T>(Vector3 start, Path path) where T : RoomObject
    {
        List<Room> availableRoomList = roomMan.GetRoomsWithAvailableObjectType<T>();
        MultiRoomPathFinder MRPF = new MultiRoomPathFinder(start, availableRoomList);
        MRPF.CalculatePaths();
    }
}
