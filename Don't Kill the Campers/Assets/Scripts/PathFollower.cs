using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplePF2D;

public abstract class PathFollower : MonoBehaviour
{
    public enum PathState
    {
        None,
        WaitingForPath,
        TravelingOnPath,
        Arrived,
        UniqueAction
    }

    private PathState _state;
    protected PathState pathState
    {
        get { return _state; }
        set
        {
            if (_state == value) return;
            switch (value)
            {
                case PathState.None: break;
                case PathState.WaitingForPath: break;
                case PathState.TravelingOnPath: break;
                case PathState.Arrived: break;
                case PathState.UniqueAction:
                    EndUniqueAction();
                    break;
            }
            _state = value;
            switch (value)
            {
                case PathState.None: break;
                case PathState.WaitingForPath:
                    StartWaitingForPath();
                    break;
                case PathState.TravelingOnPath: break;
                case PathState.Arrived:
                    StartArrived();
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
    protected bool isStationary = true;
    protected PathManager pathMan;
    protected Room.RoomTarget roomTarget;

    protected virtual void Start()
    {
        path = new Path(GameObject.Find("Grid").GetComponent<SimplePathFinding2D>());
        pathMan = GameObject.Find("PathManager").GetComponent<PathManager>();
        nextPoint = Vector3.zero;
    }

    protected virtual void ContinueNoState()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CreatePathToObject<Trunk>();
        }
    }

    protected virtual void StartWaitingForPath()
    {
        if (roomTarget == null) return;
        roomTarget.UnlockInteractionPoint();
        roomTarget = null;
    }

    protected virtual void ContinueWaitingForPath()
    {
        if (path.IsGenerated()) pathState = PathState.TravelingOnPath;
    }

    protected virtual void ContinueTravelingOnPath()
    {
        MoveAlongPath();
        if (isStationary) pathState = PathState.None;
    }

    protected virtual void StartArrived()
    {
        isStationary = true;
        NormalizePosition();
        pathState = PathState.None;
    }

    protected abstract void StartUniqueAction();
    protected abstract void ContinueUniqueAction();
    protected abstract void EndUniqueAction();

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

    protected virtual void MoveAlongPath()
    {
        if (path.IsGenerated())
        {
            if (isStationary)
            {
                if (path.GetNextPoint(ref nextPoint))
                {
                    transform.position = MoveTowardsNextPoint();
                    isStationary = false;
                }
                else
                {
                    pathState = PathState.Arrived;
                }
            }
            else
            {
                Vector2 delta = nextPoint - transform.position;
                if (delta.magnitude <= distToTargetNextPoint && !path.GetNextPoint(ref nextPoint))
                {
                    pathState = PathState.Arrived;
                }
                if (nextPoint != null)
                {
                    transform.position = MoveTowardsNextPoint();
                }
            }
        }
        else
        {
            isStationary = true;
        }
    }

    protected virtual void CreatePath(Vector3 start, Vector3 end)
    {
        path.CreatePath(start, end);
    }

    public void SetRoomTarget(Room.RoomTarget RT)
    {
        roomTarget = RT;
        int idx = RT.objectInteractionIndex;
        path.CreatePath(transform.position, RT.obj.GetInteractionPointEntryLocation(idx));
    }

    public bool SetPath(Path p)
    {
        if (!p.IsGenerated()) return false;
        path = p;
        return true;
    }

    protected virtual void CreatePathToObject<T>() where T : RoomObject
    {
        pathMan.GetPathToRoomObject<T>(this);
        pathState = PathState.WaitingForPath;
    }

    public void CouldNotSetTarget()
    {
        Debug.Log("Could not find or create path.");
        pathState = PathState.None;
    }
}
