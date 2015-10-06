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

    public Position Min { get; private set; }
    public Position Max { get; private set; }

    void Awake()
    {
        CachedBoxCollider = GetComponent<BoxCollider2D>();
        CachedTransform = GetComponent<Transform>();
        CachedRigidbody = GetComponent<Rigidbody2D>();
    }

    public void Init(Position pos, int width, int height)
    {
        _Pos = pos;
        CachedTransform.position = _Pos.vector;

        Width = width;
        Height = height;

        Min = new Position(_Pos.x - Width / 2, _Pos.y - Height / 2);
        Max = new Position(Min.x + width, Max.y + height);

        _TileStateArr = new TileState[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if(x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    _TileStateArr[x, y] = TileState.UNWALKABLE;
                }
                else
                {
                    _TileStateArr[x, y] = TileState.WALKABLE;
                }
            }
        }

        CachedBoxCollider.size = new Vector2(width, height);
    }
}
