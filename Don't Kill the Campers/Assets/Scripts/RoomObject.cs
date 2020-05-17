using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomObject : MonoBehaviour
{
    private bool isInteractible;
    [SerializeField]
    private List<Vector2Int> interactionPointOffsets, interactionPointEntryOffsets;

    // Start is called before the first frame update
    void Start()
    {
    }

    public bool IsInteractible()
    {
        return isInteractible;
    }

    //require an action once a path follower gets to an entry point
    //require an action to move from the entry point to the interaction point
    //require an action to move from the interaction point to the entry point

}
