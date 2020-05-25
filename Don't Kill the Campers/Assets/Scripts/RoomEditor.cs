using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomEditor : MonoBehaviour
{
    public enum State
    {
        None,
        Build,
        Edit,
        Erase,
        AddDoor,
        AddObject,
    }

    private State _state;
    public State state
    {
        get { return _state; }
        set
        {
            if (_state == value) return;
            switch (_state)
            {
                case State.None: break;
                case State.Build:
                    EndBuildRoom();
                    break;
                case State.Edit: break;
                case State.Erase: break;
                case State.AddDoor:
                    EndAddDoor();
                    break;
                case State.AddObject:
                    EndAddObject();
                    break;
            }
            _state = value;
            switch (_state)
            {
                case State.None: break;
                case State.Build:
                    StartBuildRoom();
                    break;
                case State.Edit: break;
                case State.Erase: break;
                case State.AddDoor:
                    StartAddDoor();
                    break;
                case State.AddObject:
                    StartAddObject();
                    break;
            }
        }
    }

    public RuleTile roomRuleTilePrefab;
    public Tile doorTilePrefab;
    public Tilemap tilemapPrefab;
    public TileBase BlockTile;
    public Bed bedPrefab;
    public Trunk trunkPrefab;
    public Toilet toiletPrefab;

    private Tilemap _tilemapInst, tilemapColl;
    private Vector3Int clickPos, curMousePos, lastMousePos;
    private Grid grid;
    private BoundsInt curBounds;
    private RuleTile[] tileArray;
    private Room room;
    private SimplePathFinding2D PF2D;
    private RoomManager roomMan;
    private RoomObject selectedObj;
    private Type roomType;

    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        tilemapColl = GameObject.Find("TilemapColl").GetComponent<Tilemap>();
        roomMan = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        PF2D = grid.GetComponent<SimplePathFinding2D>();
        roomType = typeof(Cabin);

        state = State.None;

        tileArray = new RuleTile[1000];
        for (int index = 0; index < tileArray.Length; index++)
        {
            tileArray[index] = roomRuleTilePrefab;
        }
    }

    // Update is called once per frame
    void Update()
    { 
        curMousePos = Controls.GetMousePos();

        switch (state)
        {
            case State.None:
                pickRoomType();

                if (Input.GetKeyDown(KeyCode.Space)) state = State.Build;
                else if (Input.GetKeyDown(KeyCode.D)) state = State.AddDoor;
                else if (SetRoomObjectPrefab()) state = State.AddObject;
                break;
            case State.Build:
                ContinueBuildRoom();
                if (Input.GetKeyDown(KeyCode.Space)) state = State.AddDoor;
                break;
            case State.Edit:
                break;
            case State.Erase:
                break;
            case State.AddDoor:
                ContinueAddDoor();
                if (Input.GetKeyDown(KeyCode.D)) state = State.None;
                break;
            case State.AddObject:
                ContinueAddObject();
                break;
        }

        lastMousePos = curMousePos;
    }

    void pickRoomType()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) roomType = typeof(Cabin);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) roomType = typeof(Bathhouse);
    }

    void StartBuildRoom()
    {    
        clickPos = Controls.GetMousePos();

        room = null;
        if (_tilemapInst == null)
        {
            _tilemapInst = Instantiate(tilemapPrefab);
        }
        _tilemapInst.transform.parent = grid.transform;
        _tilemapInst.SetTile(clickPos, roomRuleTilePrefab);
        grid.GetComponent<SimplePathFinding2D>().SelectedNavigationTileMap = _tilemapInst;

        curBounds = GetCurrentBounds();
    }

    void ContinueBuildRoom()
    {
        if (lastMousePos == curMousePos || _tilemapInst == null)
        {
            return;
        }

        EraseTilesWithinBounds(curBounds);

        curBounds = GetCurrentBounds();

        _tilemapInst.SetTilesBlock(curBounds, tileArray);
    }

    void EndBuildRoom()
    {
        SetCollidersOnEdgeOfBound(curBounds);
        room = roomMan.CreateRoom(roomType, curBounds);
        clickPos = Vector3Int.zero;
    }

    BoundsInt GetCurrentBounds()
    {
        int xMin = Mathf.Min(clickPos.x, curMousePos.x);
        int xDiff = Mathf.Abs(clickPos.x - curMousePos.x) + 1;
        int yMin = Mathf.Min(clickPos.y, curMousePos.y);
        int yDiff = Mathf.Abs(clickPos.y - curMousePos.y) + 1;
        return new BoundsInt(xMin, yMin, Controls.GetTileZ(), xDiff, yDiff, Controls.GetTileZ());
    }

    void SetCollidersOnEdgeOfBound(BoundsInt bound)
    {
        for (int x = bound.xMin; x < bound.xMax; x++)
        {
            SetColliderOnPoint(new Vector3Int(x, bound.yMin, Controls.GetTileZpf()));
            SetColliderOnPoint(new Vector3Int(x, bound.yMax - 1, Controls.GetTileZpf()));
        }
        for (int y = bound.yMin; y < bound.yMax; y++)
        {
            SetColliderOnPoint(new Vector3Int(bound.xMin, y, Controls.GetTileZpf()));
            SetColliderOnPoint(new Vector3Int(bound.xMax - 1, y, Controls.GetTileZpf()));
        }
        PF2D.UpdateNavMesh(bound);
    }

    void SetColliderOnPoint(Vector3Int point, bool updateNavMesh = false)
    {
        tilemapColl.SetTile(point, BlockTile);
        if (updateNavMesh)
        {
            PF2D.UpdateNavMesh(new BoundsInt(point.x - 1, point.y - 1, Controls.GetTileZpf(), 2, 2, 0));
        }
    }

    void StartAddDoor()
    {
        _tilemapInst.SetEditorPreviewTile(curMousePos, doorTilePrefab);
    }

    void ContinueAddDoor()
    {
        _tilemapInst.ClearAllEditorPreviewTiles();
        Vector3Int pos = _Bounds.ConstrainPointToBounds(curBounds, curMousePos);
        //pos = ConstrainPointToOuterBounds(curBounds, pos);
        _tilemapInst.SetEditorPreviewTile(pos, doorTilePrefab);
    }

    void EndAddDoor()
    {
        _tilemapInst.ClearAllEditorPreviewTiles();
        _tilemapInst.SetTile(curMousePos, null);
        _tilemapInst.SetTile(curMousePos, doorTilePrefab);
        room.AddDoor(curMousePos);

        tilemapColl.SetTile(Controls.GetMousePosPF(), null);
        PF2D.RemoveOneBlockedPoint(Controls.GetMousePosPF());
    }

    void EraseTilesWithinBounds(BoundsInt bounds)
    {
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                _tilemapInst.SetTile(new Vector3Int(x, y, bounds.zMin), null);
                PF2D.RemoveOneBlockedPoint(new Vector3Int(x, y, Controls.GetTileZpf()));
            }
        }
        PF2D.UpdateNavMesh(bounds);
    }

    void StartAddObject()
    {
        selectedObj = Instantiate(selectedObj);
        selectedObj.transform.position = _Bounds.ConstrainPointToInnerBounds(curBounds, curMousePos);
        
    }

    void ContinueAddObject()
    {
        Vector3 pos = _Bounds.ConstrainPointToInnerBounds(curBounds, curMousePos);
        pos.x += .5f;
        pos.y += .5f;
        selectedObj.transform.position = pos;
        if (Input.GetKeyDown(KeyCode.O)) state = State.None;
    }

    void EndAddObject()
    {
        room.AddRoomObject(selectedObj);
        SetColliderOnPoint(Controls.GetMousePosPF(), true);
        selectedObj = null;
    }

    private bool SetRoomObjectPrefab()
    {
        if (room == null) return false;
        selectedObj = null;

        if (room.GetType() == typeof(Cabin))
        {
            if (Input.GetKeyDown(KeyCode.B)) selectedObj = bedPrefab;
            else if (Input.GetKeyDown(KeyCode.T)) selectedObj = trunkPrefab;
        }
        else if (room.GetType() == typeof(Bathhouse))
        {
            if (Input.GetKeyDown(KeyCode.T)) selectedObj = toiletPrefab;
        }
        return selectedObj != null;
    }
}


