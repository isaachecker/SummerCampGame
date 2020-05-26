using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DesireType
{
    none = 0,
    //Camper Core Desires
    bathroom = 1, eat, drink, health, bathe,
    //Camper Anc Desires
    chill = 10, music, community, nature, exercise, creative,
    science, education, spirituality
}
[System.Serializable]
public class Desire
{
    public string name;
    private DesireType type;
    [Range(0, 100)]
    [SerializeField]
    private float value = 0;
    private float incSpeed;
    private float incSpeedMult;
    private float decSpeed;
    const int wantValue = 50;
    const int needValue = 75;

    public Desire()
    {
        type = DesireType.none;
        name = type.ToString();
        incSpeed = 1;
        incSpeedMult = 0.1f;
    }

    public Desire(DesireType _type, float _incSpeed)
    {
        type = _type;
        name = type.ToString();
        incSpeed = _incSpeed;
        incSpeedMult = 0.1f;
    }

    public float Increment()
    {
        value = Mathf.Min(value + (incSpeed * incSpeedMult * Time.deltaTime), 100); 
        return value;
    }

    public float Decrement()
    {
        value = Mathf.Max(value - (decSpeed * Time.deltaTime), 0);
        if (value == 0) decSpeed = 0;
        return value;
    }

    public float IncrementAndDecrement()
    {
        Increment();
        Decrement();
        return value;
    }

    public bool IsWanted() { return value >= wantValue; }
    public bool IsNeeded() { return value >= needValue; }
    public void SetDecrementSpeed(float speed)
    {
        decSpeed = speed;
    }
    public DesireType GetDesireType()
    {
        return type;
    }
    public void SetIncSpeedMult(float val)
    {
        incSpeedMult = val;
    }
    public void ResetIncSpeedMult()
    {
        incSpeedMult = 1;
    }
    public void Reset()
    {
        ResetIncSpeedMult();
        SetDecrementSpeed(0);
        value = 0;
    }

    public static List<DesireType> GetRandomAncDesireTypes(int numDesires)
    {
        List<DesireType> types = new List<DesireType>();
        if (numDesires <= 0) return types;
        List<int> enumVals = Controls.PickNumbersInRange(numDesires, 10, 18);
        for (int i = 0; i < enumVals.Count; i++)
        {
            types.Add((DesireType)enumVals[i]);
        }
        return types;
    }
}
