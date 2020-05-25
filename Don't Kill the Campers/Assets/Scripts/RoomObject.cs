using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomObject : MonoBehaviour
{
    [Serializable]
    protected class InteractionPoint
    {
        [SerializeField]
        Vector3 center;
        [SerializeField]
        Vector2 interactionOffsetFromCenter;
        [SerializeField]
        Vector2 entryOffsetFromCenter;
        bool isLocked = false;
        bool? canBeAccessed;

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
                //generate a path to door. If path exists, set canBeAccessed to true
            }
            return canBeAccessed == true ? true : false; //for now, so no compiler errors
        }

        public Vector3 GetEntryPointLocation(Vector3 objPos)
        {
            objPos.x += entryOffsetFromCenter.x;
            objPos.y += entryOffsetFromCenter.y;
            return objPos;
        }

        public Vector3 GetInteractionPointLocation(Vector3 objPos)
        {
            objPos.x += interactionOffsetFromCenter.x;
            objPos.y += interactionOffsetFromCenter.y;
            return objPos;
        }
    }
    
    [SerializeField]
    private List<InteractionPoint> interactionPoints;
    private string ID;
    [SerializeField]
    protected CamperState toCamperState;

    // Start is called before the first frame update
    void Start()
    {
        ID = Controls.MakeRandomID(6);
    }

    public string GetID()
    {
        return ID;
    }

    public bool HasOpenInteractionPoint()
    {
        foreach (InteractionPoint IP in interactionPoints)
        {
            if (!IP.IsLocked()) return true;
        }
        return false;
    }

    public int LockOpenInteractionPoint()
    {
        for (int i = 0; i < interactionPoints.Count; i++)
        {
            InteractionPoint IP = interactionPoints[i];
            if (!IP.IsLocked() && IP.Lock())
            {
                return i;
            }
        }
        return -1;
    }

    public int LockOpenInteractionPoint(ref Vector3 entryPosition)
    {
        int i;
        for (i = 0; i < interactionPoints.Count; i++)
        {
            InteractionPoint IP = interactionPoints[i];
            if (!IP.IsLocked() && IP.Lock())
            {
                break;
            }
        }
        entryPosition = GetInteractionPointEntryLocation(i);
        return i >= 0 ? i : -1;
    }

    public bool UnlockInteractionPoint(int index)
    {
        if (index >= interactionPoints.Count) return false;
        return interactionPoints[index].Unlock();
    }

    public Vector3 GetInteractionPointEntryLocation(int index)
    {
        if (index >= interactionPoints.Count || index < 0)
        {
            Debug.LogError("Bad Interaction Point input");
            return Vector3.zero;
        }
        return interactionPoints[index].GetEntryPointLocation(transform.position);
    }

    public Vector3 GetInteractionPointLocation(int index)
    {
        if (index >= interactionPoints.Count || index < 0)
        {
            Debug.LogError("Bad Interaction Point input");
            return Vector3.zero;
        }
        return interactionPoints[index].GetInteractionPointLocation(transform.position);
    }

    public bool AreInteractionPointsAvailable()
    {
        foreach (InteractionPoint IP in interactionPoints)
        {
            if (!IP.IsLocked()) return true;
        }
        return false;
    }

    public static List<Room.Type> GetAllowedRoomTypes<T>() where T : RoomObject
    {
        Type typeT = typeof(T);
        if (typeT == typeof(Bed)) return Bed.GetAllowedRoomTypesSub();
        else if (typeT == typeof(Trunk)) return Trunk.GetAllowedRoomTypesSub();
        else if (typeT == typeof(Toilet)) return Toilet.GetAllowedRoomTypesSub();
        return new List<Room.Type>();
    }

    public CamperState GetCamperStateOnUse()
    {
        return toCamperState;
    }
}
