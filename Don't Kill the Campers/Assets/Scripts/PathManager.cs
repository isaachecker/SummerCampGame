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
        Dictionary<InteractionPoint, Path> IPpathMap;

        //Constructor
        public MultiRoomPathFinder(Camper _camper, List<Room> _rooms, List<RoomObject> _objects)
        {
            Inizialize(_camper, _rooms, _objects);
        }

        /// <summary>
        /// Sets up the MRPF
        /// </summary>
        /// <param name="_camper">The camper</param>
        /// <param name="_rooms">The rooms to find paths to</param>
        /// <param name="_objects">The _objects to find paths to</param>
        public void Inizialize(Camper _camper, List<Room> _rooms, List<RoomObject> _objects)
        {
            camper = _camper;
            startPoint = camper.transform.position;
            if (simpPF2D == null) simpPF2D = GameObject.Find("Grid").GetComponent<SimplePathFinding2D>();

            //set up the room paths
            if (roomPathMap == null) roomPathMap = new Dictionary<Room, Path>();
            foreach (Room room in _rooms)
            {
                roomPathMap[room] = new Path(simpPF2D);
            }

            //set up the IP paths
            if (IPpathMap == null) IPpathMap = new Dictionary<InteractionPoint, Path>();
            foreach (RoomObject obj in _objects)
            {
                foreach (InteractionPoint IP in obj.interactionPoints)
                {
                    IPpathMap[IP] = new Path(simpPF2D);
                }
            }
        }

        /// <summary>
        /// Starts calculating all paths to rooms in roomPathMap and paths to IPs in IPpathMap
        /// </summary>
        public void CalculatePaths()
        {
            foreach (KeyValuePair<Room, Path> roomPath in roomPathMap)
            {
                Vector3 position = roomPath.Key.doorPos;
                if (position != null && position != Vector3.zero)
                {
                    roomPath.Value.CreatePath(startPoint, position);
                }
            }
            foreach (KeyValuePair<InteractionPoint, Path> IPpath in IPpathMap)
            {
                Vector3 position = IPpath.Key.GetEntryPointLocation();
                if (position != null && position != Vector3.zero)
                {
                    IPpath.Value.CreatePath(startPoint, position);
                }
            }
        }

        public void Dispose()
        {
            camper = null;
            startPoint = Vector3.zero;
            roomPathMap = null;
            IPpathMap = null;
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
            foreach (KeyValuePair<InteractionPoint, Path> IPpath in IPpathMap)
            {
                if (!IPpath.Value.IsGenerated()) return false;
            }
            return true;
        }

        public void SetPathFollowerData()
        {
            camper.SetPathMaps(roomPathMap, IPpathMap);
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
        //for each MRFP, check if all paths have been generated
        foreach (MultiRoomPathFinder MRPF in multiRoomPathFinders)
        {
            if (MRPF.PathsAreGenerated())
            {
                MRPF.SetPathFollowerData();
                toRemove.Add(MRPF);
            }
        }
        //Stop checking values for MRPFs that are no longer being used
        if (toRemove.Count > 0)
        {
            foreach (MultiRoomPathFinder MRPF in toRemove)
            {
                multiRoomPathFinders.Remove(MRPF);
                MRPF.Dispose();
                availableMRPFs.Enqueue(MRPF);
            }
            toRemove.Clear();
        }
    }

    /// <summary>
    /// Retrieves an MRPF that is no longer in use, if any are available
    /// </summary>
    /// <returns></returns>
    private MultiRoomPathFinder getAvailableMRPF()
    {
        if (availableMRPFs.Count == 0) return null;
        return availableMRPFs.Dequeue();
    }


    public void GetPathsToRoomsAndObjects(Camper camper, List<Room> rooms, List<RoomObject> objects)
    {
        MultiRoomPathFinder MRPF = getAvailableMRPF();

        if (MRPF == null)
        {
            MRPF = new MultiRoomPathFinder(camper, rooms, objects);
        }
        else
        {
            MRPF.Inizialize(camper, rooms, objects);
        }

        MRPF.CalculatePaths();
        multiRoomPathFinders.Add(MRPF);
    }
}
