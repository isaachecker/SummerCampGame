using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplePF2D;

public class InteractionPoint : MonoBehaviour
{
    [SerializeField]
    Vector2 entryOffsetFromCenter;
    Camper isLocked = null;
    bool? canBeAccessed;
    Path path;
    public float defaultTimeOfInteraction = 30; //arbitrary for testing
    public float startTime;
    private Queue<Camper> campersQueued;
    private List<Camper> campersEnRoute;
    private RoomObject parentRoomObject;
    [SerializeField]
    private float timeRemainingScoreMultiplier = 6;

    private void Start()
    {
        campersQueued = new Queue<Camper>();
        campersEnRoute = new List<Camper>();
        parentRoomObject = transform.parent.GetComponent<RoomObject>();
        path = new Path(GameObject.Find("Grid").GetComponent<SimplePathFinding2D>());
    }

    public RoomObject GetRoomObject()
    {
        return parentRoomObject;
    }

    /// <summary>
    /// Generates a path from some start point to the entry point of this interaction point
    /// </summary>
    /// <param name="end">Where the path should end</param>
    public void GeneratePathToEntryPoint(Vector3 start)
    {
        path.CreatePath(start, GetEntryPointLocation());
    }

    /// <summary>
    /// Gets the length of the IPs path
    /// </summary>
    /// <returns>The length of the IPs path</returns>
    public float GetPathLength()
    {
        if (path == null) return -1;
        return path.GetPathLength();
    }

    /// <summary>
    /// Gets the Path from the entrance of the room to the entry point
    /// </summary>
    /// <returns>The IPs Path</returns>
    public Path GetPath()
    {
        return path;
    }

    public bool Lock(Camper camper)
    {
        if (isLocked == camper) return true;
        if (isLocked != null) return false;
        isLocked = camper;

        //TODO make all campers enroute reconsider their life choices
        recalculateEnRouteCamperPaths();

        return true;
    }
    public bool Unlock(Camper camper)
    {
        if (isLocked == null) return false;
        if (isLocked != camper) return false;
        isLocked = null;
        startTime = 0;
        DequeueCamper();
        return true;
    }
    public bool IsLocked()
    {
        return isLocked != null;
    }

    public bool IsAccessible()
    {
        if (canBeAccessed == null)
        {
            if (path != null && path.IsGenerated())
            {
                canBeAccessed = path.GetPathLength() > -1;
                return (bool)canBeAccessed;
            }
            //generate a path to door. If path exists, set canBeAccessed to true
        }
        return (bool)canBeAccessed;
    }

    public Vector3 GetEntryPointLocation()
    {
        Vector3 pos = transform.position;
        pos.x += entryOffsetFromCenter.x;
        pos.y += entryOffsetFromCenter.y;
        return pos;
    }

    private void recalculateEnRouteCamperPaths()
    {
        List<Camper> tempList = new List<Camper>();
        foreach (Camper camper in campersEnRoute)
        {
            tempList.Add(camper);
        }
        campersEnRoute.Clear();
        foreach(Camper camper in tempList)
        {
            camper.RecalculatePotentialPaths();
        }
    }

    /// <summary>
    /// Calculates the score for this interaction point to determine its availability
    /// </summary>
    /// <returns>The IP Score or -1 if inaccessible</returns>
    public float GetIPScore()
    {
        float IPpathLength = GetPathLength();
        if (IPpathLength == -1) return -1;
        return IPpathLength + GetWaitScore();
    }

    /// <summary>
    /// Gets the score for how long a camper would need to wait for this interaction point
    /// </summary>
    /// <returns>The Wait Score</returns>
    public float GetWaitScore()
    {
        float remainingTimeAdjusted = GetRemainingTime() * timeRemainingScoreMultiplier;
        float timeToCompAdjusted = defaultTimeOfInteraction * timeRemainingScoreMultiplier;
        float queuedTime = timeToCompAdjusted + GetPathLength();
        float queuedScore = queuedTime * campersQueued.Count;
        if (IsLocked() && startTime == 0)
        {
            //a camper is on the way but isn't here yet
            queuedScore += queuedTime;
        }
        return remainingTimeAdjusted + queuedScore;
    }

    /// <summary>
    /// Adds a camper to a list of campers heading for this interaction point
    /// </summary>
    /// <param name="camper">The camper</param>
    public void AddCamperEnRoute(Camper camper)
    {
        if (!campersEnRoute.Contains(camper)) campersEnRoute.Add(camper);
    }

    /// <summary>
    /// Removes a camper from the list of campers heading to this IP
    /// </summary>
    /// <param name="camper">The camper to remove</param>
    public void RemoveCamperEnRoute(Camper camper)
    {
        campersEnRoute.Remove(camper);
    }

    /// <summary>
    /// Determines if a camper should be queued to interact with this IP
    /// </summary>
    /// <returns>True if there are other campers queued or if this IP is locked. False otherwise.</returns>
    public bool ShouldQueueCamper()
    {
        if (campersQueued.Count > 0) return true;
        return IsLocked();
    }

    /// <summary>
    /// Queues a camper to use this IP
    /// </summary>
    /// <param name="camper">The camper to add</param>
    public void QueueCamper(Camper camper)
    {
        campersQueued.Enqueue(camper);
        //TODO make all campers enroute reconsider their life choices
    }

    /// <summary>
    /// Pulls campers off the queue until one is found who is still targeting this IP
    /// </summary>
    /// <returns>The next camper to interact with this IP</returns>
    public Camper DequeueCamper()
    {
        if (campersQueued.Count == 0) return null;
        Camper camper = null;
        do
        {
            camper = campersQueued.Dequeue();
            if (camper.GetTargetedInteractionPoint() == this) break;
        } while (campersQueued.Count > 0);

        if (camper == null) return null;

        camper.CreatePathToTargetedInteractionPoint();
        return camper;
    }

    /// <summary>
    /// Sets that start time of a camper using an interaction point
    /// </summary>
    public void StartTimeWithIP()
    {
        startTime = Time.time;
    }

    /// <summary>
    /// Gets the remaining time a camper has with an interaction point
    /// </summary>
    /// <returns>The remaining time as a float</returns>
    public float GetRemainingTime()
    {
        if (startTime == 0) return 0;
        return Mathf.Max(startTime + defaultTimeOfInteraction - Time.time, 0);
    }
}
