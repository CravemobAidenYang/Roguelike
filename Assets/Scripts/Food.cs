using UnityEngine;
using System.Collections;

[System.Serializable]
public class FoodSaveData
{
    public int foodIndex;
    public int healPoint;
    public Position pos;

    public FoodSaveData(int foodIndex, int healPoint, Position pos)
    {
        this.foodIndex = foodIndex;
        this.healPoint = healPoint;
        this.pos = pos;
    }
}

public class Food : MonoBehaviour 
{
    public int _FoodIndex;
    public int _MinHealPoint, _MaxHealPoint;

    int _HealPoint;

    public int HealPoint
    {
        get
        {
            return _HealPoint;
        }
    }

    Position _Pos;

    public Position Pos
    {
        get
        {
            return _Pos;
        }
    }

    Transform _CachedTransform;
    SpriteRenderer _SprRenderer;

    void Awake()
    {
        _CachedTransform = GetComponent<Transform>();
        _SprRenderer = GetComponent<SpriteRenderer>();
    }

	public void Init(Position pos)
    {
        _Pos = pos;
        _CachedTransform.position = pos.vector;

        _HealPoint = Random.Range(_MinHealPoint, _MaxHealPoint + 1);
    }

    public FoodSaveData CreateSaveData()
    {
        return new FoodSaveData(_FoodIndex, _HealPoint, _Pos);
    }

    public void ApplySaveData(FoodSaveData data)
    {
        _FoodIndex = data.foodIndex;
        _HealPoint = data.healPoint;
        _Pos = data.pos;

        _CachedTransform.position = _Pos.vector;
    }
}
