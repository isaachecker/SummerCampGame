using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum State
{
    None,
    Build,
    Edit,
    Erase,
    AddDoor
}

public class RoomEditor : MonoBehaviour
{
    public RuleTile roomRuleTilePrefab;
    public Tile doorTilePrefab;
    public Tilemap tilemapPrefab;
    public Room roomPrefab;
    public TileBase BlockTile;
    private Tilemap _tilemapInst, tilemapColl;
    private Vector3Int clickPos, curMousePos, lastMousePos;
    private Grid grid;
    private State state;
    private BoundsInt curBounds, lastBounds;
    private List<BoundsInt> bounds;
    private RuleTile[] tileArray;
    private Room room;
    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        tilemapColl = GameObject.Find("TilemapColl").GetComponent<Tilemap>();
        state = State.None;
        bounds = new List<BoundsInt>();

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
                if (Input.GetKeyDown(KeyCode.Space))StartBuildRoom();
                else if (Input.GetKeyDown(KeyCode.D)) StartAddDoor();
                break;
            case State.Build:
                ContinueBuildRoom();
                if (Input.GetKeyDown(KeyCode.Space)) EndBuildRoom();
                break;
            case State.Edit:
                break;
            case State.Erase:
                break;
            case State.AddDoor:
                ContinueAddDoor();
                if (Input.GetKeyDown(KeyCode.D)) EndAddDoor();
                break;
        }

        if (Input.GetKeyDown(KeyCode.C)) Debug.Log(TestCollider());

        lastMousePos = curMousePos;
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

        state = State.Build;
    }

    void ContinueBuildRoom()
    {
        if (lastMousePos == curMousePos || _tilemapInst == null)
        {
            return;
        }

        EraseTilesWithinBounds(curBounds);

        curBounds = GetCurrentBounds();

        Debug.Log(curBounds.ToString());
        _tilemapInst.SetTilesBlock(curBounds, tileArray);
    }

    void EndBuildRoom()
    {
        if (curBounds.size != Vector3Int.zero)
        {
            bounds.Add(curBounds);
            SetCollidersOnEdgeOfBound(curBounds);
            StartAddDoor();
            room = Instantiate(roomPrefab);
            room.Initialize(_tilemapInst, curBounds);
        }
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

    bool BoundsInListEncapsulatesBound(BoundsInt containee)
    {
        if (bounds.Count == 0) return false;
        foreach (BoundsInt container in bounds)
        {
            if (BoundsEncapsulatesBounds(container, containee)) return true;
        }
        return false;
    }

    bool BoundsEncapsulatesBounds(BoundsInt container, BoundsInt containee)
    {
        if (!container.Contains(containee.max)) return false;
        if (!container.Contains(containee.min)) return false;
        return true;
    }

    void SetCollidersOnEdgeOfBound(BoundsInt bound)
    {
        for (int x = bound.xMin; x < bound.xMax; x++)
        {
            tilemapColl.SetTile(new Vector3Int(x, bound.yMin, Controls.GetTileZpf()), BlockTile);
            tilemapColl.SetTile(new Vector3Int(x, bound.yMax - 1, Controls.GetTileZpf()), BlockTile);
        }
        for (int y = bound.yMin; y < bound.yMax; y++)
        {
            tilemapColl.SetTile(new Vector3Int(bound.xMin, y, Controls.GetTileZpf()), BlockTile);
            tilemapColl.SetTile(new Vector3Int(bound.xMax - 1, y, Controls.GetTileZpf()), BlockTile);
        }
        grid.GetComponent<SimplePathFinding2D>().UpdateNavMesh(bound);
    }

    bool TestCollider()
    {
        RaycastHit2D test;
        Vector3 pos = Controls.GetMousePosF();
        test = Physics2D.Raycast(new Vector2(pos.x, pos.y), new Vector2(0, 0));
        return test.transform != null;
    }

    void StartAddDoor()
    {
        _tilemapInst.SetEditorPreviewTile(curMousePos, doorTilePrefab);

        state = State.AddDoor;
    }

    void ContinueAddDoor()
    {
        _tilemapInst.ClearAllEditorPreviewTiles();
        _tilemapInst.SetEditorPreviewTile(curMousePos, doorTilePrefab);
    }

    void EndAddDoor()
    {
        _tilemapInst.ClearAllEditorPreviewTiles();
        _tilemapInst.SetTile(curMousePos, null);
        _tilemapInst.SetTile(curMousePos, doorTilePrefab);
        room.AddDoor(curMousePos);

        tilemapColl.SetTile(Controls.GetMousePosPF(), null);
        grid.GetComponent<SimplePathFinding2D>().RemoveOneBlockedPoint(Controls.GetMousePosPF());

        state = State.None;
    }

    bool BoundsIntersect(BoundsInt bound1, BoundsInt bound2)
    {
        bool xInt = (bound1.xMin <= bound2.xMin && bound1.xMax >= bound2.xMin) ||
            (bound1.xMin <= bound2.xMax && bound1.xMax >= bound2.xMax);
        bool yInt = (bound1.yMin <= bound2.yMin && bound1.yMax >= bound2.yMin) ||
            (bound1.yMin <= bound2.yMax && bound1.yMax >= bound2.yMax);
        return xInt && yInt;
    }

    void EraseTilesWithinBounds(BoundsInt bounds)
    {
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                _tilemapInst.SetTile(new Vector3Int(x, y, bounds.zMin), null);
            }
        }
    }
}


