using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour
{
    Position _Pos;
    TileState[,] _TileStateArr;

    public Transform CachedTransform {get; private set;}
    public Rigidbody2D CachedRigidbody { get; private set; }
    public BoxCollider2D CachedBoxCollider { get; private set; }

    public int Width { get; set; }
    public int Height { get; set; }

    //Position _Min, _Max;
    public Position Min
    { 
        get
        {
            return new Position(_Pos.x - Width / 2, _Pos.y - Height / 2);
        }
    }

    public Position Max
    {
        get
        {
            return new Position(Min.x + Width - 1, Min.y + Height - 1);
        }
    }

    void Awake()
    {
        CachedBoxCollider = GetComponent<BoxCollider2D>();
        CachedTransform = GetComponent<Transform>();
        CachedRigidbody = GetComponent<Rigidbody2D>();
    }

    public void Init(Position pos, int width, int height)
    {
        SetPos(pos);
        Width = width;
        Height = height;

        //Min = new Position(_Pos.x - Width / 2, _Pos.y - Height / 2);
        //Max = new Position(Min.x + width, Min.y + height);

        _TileStateArr = new TileState[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if(x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    _TileStateArr[x, y] = TileState.WALL;
                }
                else
                {
                    _TileStateArr[x, y] = TileState.GROUND;
                }
            }
        }

        CachedBoxCollider.size = new Vector2(width + 2, height + 2);
    }

    public void SetPos(Position pos)
    {
        _Pos = pos;
        CachedTransform.position = _Pos.vector;
        //Min = new Position(_Pos.x - Width / 2, _Pos.y - Height / 2);
        //Max = new Position(_Pos.x + Width / 2, _Pos.y + Height / 2);
    }

    public void Move(Position moveDistance)
    {
        var pos = new Position(_Pos.x + moveDistance.x, _Pos.y + moveDistance.y);
        SetPos(pos);
    }

    void Update()
    {
        //if(!CachedRigidbody.IsSleeping())
        {
            _Pos.x = (int)CachedTransform.position.x;
            _Pos.y = (int)CachedTransform.position.y;
            //Min = new Position(_Pos.x - Width / 2, _Pos.y - Height / 2);
            //Min.x = _Pos.x - Width / 2;
            //Min.y = _Pos.y - Height / 2;

            //Max = new Position(_Pos.x + Width / 2, _Pos.y + Height / 2);
            //Max.x = Min.x + Width;
            //Max.y = Min.y + Height;
        }
    }

    void OnDrawGizmos()
    {
        //for (int x = Min.x; x <= Max.x; ++x)
        //{
        //    for (int y = Min.y; y <= Max.y; ++y)
        //    {
        //        Vector3 center = new Vector3((float)x, (float)y);
        //        Gizmos.DrawCube(center, Vector3.one);
        //    }
        //}
    }
}
