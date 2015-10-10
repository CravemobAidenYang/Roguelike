using UnityEngine;
using System.Collections;

public abstract class Unit : MonoBehaviour
{
    //Position _Pos;
    public float _Speed;

    Transform _CachedTransform;

    protected Transform CachedTransform
    {
        get
        {
            return _CachedTransform;
        }
    }

    private Position _Pos;
    public Position Pos
    {
        get
        {
            return _Pos;
        }
        protected set
        {
            _Pos = value;
        }
    }

    //현재 Pos에 위치한 타일을 얻어옴
    public Tile CurTile
    {
        get
        {
            return TileManager.Instance.GetTile(Pos);
        }
    }

    void Awake()
    {
        print("Unit.Awake");
        _CachedTransform = this.transform;

        OnAwake();
    }

    protected abstract void OnAwake();

}
