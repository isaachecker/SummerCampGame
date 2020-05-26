using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public static string MakeRandomID(int size)
    {
        string id = "";
        //for (int i = 0; i < size; i++)
       // {
         //   id += Convert.ToChar(Mathf.Floor(UnityEngine.Random.Range(65, 90)));
       // }
        return id;
    }

    public static List<int> PickNumbersInRange(int numNumbers, int min, int max)
    {
        List<int> picked = new List<int>();
        int numPossibilities = max - min + 1;
        if (numPossibilities == 0) return picked;
        if (numNumbers > numPossibilities) numNumbers = numPossibilities;
        while (picked.Count < numNumbers)
        {
            int num = UnityEngine.Random.Range(min, max + 1);
            if (!picked.Contains(num)) picked.Add(num);
        }
        return picked;
    }
}
