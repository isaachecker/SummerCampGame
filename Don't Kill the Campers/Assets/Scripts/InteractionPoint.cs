using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplePF2D;

public class InteractionPoint : MonoBehaviour
{
    [SerializeField]
    Vector2 entryOffsetFromCenter;
    bool isLocked = false;
    bool? canBeAccessed;
    Path path;
    public float defaultTimeOfInteraction = 5; //arbitrary for testing
    public float startTime;
    private Queue<Camper> campersQueued;
    private List<Camper> campersEnRoute;
    private RoomObject parentRoomObject;

    private void Update()
    {
        Debug.Log(transform.ToString() + " " + IsLocked());
    }

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

    public bool Lock()
    {
        if (isLocked) return false;
        isLocked = true;
        return true;
    }
    public bool Unlock()
    {
        if (!isLocked) return false;
        isLocked = false;
        DequeueCamper();
        return true;
    }
    public bool IsLocked()
    {
        return isLocked;
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
        float remainingTimeAdjusted = GetRemainingTime() * 2; //2 is arbitrary for now
        float timeToCompAdjusted = defaultTimeOfInteraction * 2; //2 is arbitrary for now
        float queuedScore = (timeToCompAdjusted + GetPathLength()) * campersQueued.Count;
        return remainingTimeAdjusted + queuedScore;
    }

    public void AddCamperEnRoute(Camper camper)
    {
        if (!campersEnRoute.Contains(camper)) campersEnRoute.Add(camper);
    }

    public void RemoveCamperEnRoute(Camper camper)
    {
        campersEnRoute.Remove(camper);
    }

    public bool ShouldQueueCamper()
    {
        if (campersQueued.Count > 0) return true;
        return IsLocked();
    }

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

    public void StartTimeWithIP()
    {
        startTime = Time.time;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(startTime + defaultTimeOfInteraction - Time.time, 0);
    }
}
