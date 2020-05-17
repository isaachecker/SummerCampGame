using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Controls
{
    public static Vector3Int GetMousePos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), GetTileZ());
    }

    public static Vector3 GetMousePosF()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = GetTileZ();
        return pos;
    }

    public static Vector3Int GetMousePosPF()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), GetTileZpf());
    }

    public static int GetTileZ()
    {
        return 1;
    }

    public static int GetTileZpf()
    {
        return 0;
    }
}
