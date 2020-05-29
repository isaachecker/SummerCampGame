using System;
using System.Collections;
using System.Collections.Generic;
using SimplePF2D;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    private class MultiRoomPathFinder : IDisposable
    {
        Camper camper;
        Vector3 startPoint;
        SimplePathFinding2D simpPF2D;
        Dictionary<Room, Path> roomPathMap;

        //Constructor
        public MultiRoomPathFinder(Camper _camper, List<Room> _rooms)
        {
            Inizialize(_camper, _rooms);
        }

        public void Inizialize(Camper _camper, List<Room> _rooms)
        {
            camper = _camper;
            startPoint = camper.transform.position;

            if (simpPF2D == null) simpPF2D = GameObject.Find("Grid").GetComponent<SimplePathFinding2D>();
            if (roomPathMap == null) roomPathMap = new Dictionary<Room, Path>();
            for (int i = 0; i < _rooms.Count; i++)
            {
                Path testPath = new Path(simpPF2D);
                roomPathMap[_rooms[i]] = testPath;
            }
        }

        /// <summary>
        /// Starts calculating all paths to rooms in roomPathMap
        /// </summary>
        public void CalculatePaths()
        {
            foreach (KeyValuePair<Room, Path> roomPath in roomPathMap)
            {
                Vector3 position = roomPath.Key.doorPos;
                if (position != Vector3.zero && position != null)
                {
                    roomPath.Value.CreatePath(startPoint, position);
                }
            }
        }

        public void Clear()
        {
            camper = null;
            startPoint = Vector3.zero;
            roomPathMap.Clear();
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Checks if all the paths trying to generate have generated
        /// </summary>
        /// <returns>True if all paths are generated. False otherwise</returns>
        public bool PathsAreGenerated()
        {
            foreach (KeyValuePair<Room, Path> roomPath in roomPathMap)
            {
                if (!roomPath.Value.IsGenerated()) return false;
            }
            return true;
        }

        public void SetPathFollowerData()
        {
            camper.SetRoomPathMap(roomPathMap);
        }
    }

    private List<MultiRoomPathFinder> multiRoomPathFinders;
    private List<MultiRoomPathFinder> toRemove;
    private Queue<MultiRoomPathFinder> availableMRPFs;

    // Start is called before the first frame update
    void Start()
    {
        multiRoomPathFinders = new List<MultiRoomPathFinder>();
        toRemove = new List<MultiRoomPathFinder>();
        availableMRPFs = new Queue<MultiRoomPathFinder>();
    }

    // Update is called once per frame
    void Update()
    {
        if (toRemove.Count > 0) toRemove.Clear();

        foreach (MultiRoomPathFinder MRPF in multiRoomPathFinders)
        {
            if (MRPF.PathsAreGenerated())
            {
                MRPF.SetPathFollowerData();
                toRemove.Add(MRPF);
            }
        }
        foreach(MultiRoomPathFinder MRPF in toRemove)
        {
            multiRoomPathFinders.Remove(MRPF);
            availableMRPFs.Enqueue(MRPF);
            //MRPF.Dispose(); keep commented for now before testing occurs
        }
    }

    private MultiRoomPathFinder getAvailableMRPF()
    {
        if (availableMRPFs.Count == 0) return null;
        return availableMRPFs.Dequeue();
    }

    public void GetPathsToRooms(Camper camper, List<Room> rooms)
    {
        MultiRoomPathFinder MRPF = getAvailableMRPF();
        if (MRPF == null) MRPF = new MultiRoomPathFinder(camper, rooms);
        MRPF.CalculatePaths();
        multiRoomPathFinders.Add(MRPF);
    }
}
