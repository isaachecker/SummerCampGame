using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplePF2D;
using System;

public abstract class PathFollower : MonoBehaviour
{
    public enum PathState
    {
        None,
        WaitingForPath,
        TravelingOnPath,
        Arrived,
        Idle,
        UniqueAction
    }

    private PathState _state;
    protected PathState pathState
    {
        get { return _state; }
        set
        {
            if (_state == value) return;
            switch (_state)
            {
                case PathState.None:
                    EndNoState();
                    break;
                case PathState.WaitingForPath:
                    EndWaitingForPath();
                    break;
                case PathState.TravelingOnPath:
                    EndTravelingOnPath();
                    break;
                case PathState.Arrived:
                    EndArrived();
                    break;
                case PathState.Idle:
                    EndIdle();
                    break;
                case PathState.UniqueAction:
                    EndUniqueAction();
                    break;
            }
            _state = value;
            switch (_state)
            {
                case PathState.None:
                    StartNoState();
                    break;
                case PathState.WaitingForPath:
                    StartWaitingForPath();
                    break;
                case PathState.TravelingOnPath:
                    StartTravelingOnPath();
                    break;
                case PathState.Arrived:
                    StartArrived();
                    break;
                case PathState.Idle:
                    StartIdle();
                    break;
                case PathState.UniqueAction:
                    StartUniqueAction();
                    break;
            }
        }
    }

    public float speed = 1.0f;
    public float distToTargetNextPoint = 0.1f;

    protected Path path;
    private Vector3 nextPoint;
    protected PathManager pathMan;

    protected virtual void Start()
    {
        path = new Path(GameObject.Find("Grid").GetComponent<SimplePathFinding2D>());
        pathMan = GameObject.Find("PathManager").GetComponent<PathManager>();
    }

    #region State Functions
    //None
    protected virtual void StartNoState() { }
    protected virtual void ContinueNoState() { }
    protected virtual void EndNoState() { }

    //Waiting for Path
    protected virtual void StartWaitingForPath() { }
    protected virtual void ContinueWaitingForPath()
    {
        if (path.IsGenerated()) pathState = PathState.TravelingOnPath;
    }
    protected virtual void EndWaitingForPath() { }

    //Traveling on Path
    protected virtual void StartTravelingOnPath()
    {
        nextPoint = Vector3.zero;
    }
    protected virtual void ContinueTravelingOnPath()
    {
        MoveAlongPath();
    }
    protected virtual void EndTravelingOnPath() { }

    //Arrived
    protected virtual void StartArrived()
    {
        NormalizePosition();
        SetPathStateNone();
    }
    protected virtual void ContinueArrived() { }
    protected virtual void EndArrived() { }

    protected virtual void StartIdle() { }
    protected virtual void ContinueIdle() { }
    protected virtual void EndIdle() { }

    //Unique Action
    protected virtual void StartUniqueAction() { }
    protected virtual void ContinueUniqueAction() { }
    protected virtual void EndUniqueAction() { }
    #endregion

    Vector3 MoveTowardsNextPoint()
    {
        Vector3 direction = (nextPoint - transform.position).normalized;
        return transform.position + (direction * speed * Time.deltaTime);
    }

    protected void NormalizePosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Floor(pos.x);
        pos.y = Mathf.Floor(pos.y);
        pos.x += .5f;
        pos.y += .5f;
        transform.position = pos;
    }

    /// <summary>
    /// Move the path follower along the Path. Set PathState to Arrived once the follower
    /// has no more points to move toward
    /// </summary>
    protected virtual void MoveAlongPath()
    {
        if (nextPoint == Vector3.zero && !path.GetNextPoint(ref nextPoint))
        {
            pathState = PathState.Arrived;
            return;
        }

        Vector2 delta = nextPoint - transform.position;
        if (delta.magnitude <= distToTargetNextPoint && !path.GetNextPoint(ref nextPoint))
        {
            pathState = PathState.Arrived;
            return;
        }

        transform.position = MoveTowardsNextPoint();
    }

    /// <summary>
    /// Creates a path for the follower to follow
    /// </summary>
    /// <param name="start">The start position of the path</param>
    /// <param name="end">The end position of the path</param>
    public virtual void CreatePath(Vector3 start, Vector3 end)
    {
        path.CreatePath(start, end);
    }

    /// <summary>
    /// Sets this path follower's path to one that was created externally
    /// </summary>
    /// <param name="p">The Path</param>
    /// <returns>True if it was set. False otherwise</returns>
    public bool SetPath(Path p)
    {
        if (!p.IsGenerated()) return false;
        path = p;
        return true;
    }

    public void CouldNotSetTarget()
    {
        Debug.Log("Could not find or create path.");
        SetPathStateNone();
    }

    /// <summary>
    /// Sets the pathState to None
    /// </summary>
    protected void SetPathStateNone()
    {
        pathState = PathState.None;
    }
}
