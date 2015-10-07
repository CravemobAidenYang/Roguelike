using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    //생성된 타일들을 묶어두기 위한 용도
    public Transform _TileGroup;

    public GameObject _PlayerPrefab;

    static TileManager _Instance = null;

    public static TileManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    Tile[,] _Tiles;
    Size _Size;

	// Use this for initialization
	void Awake ()
    {
	    if(_Instance == null)
        {
            _Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
	}
	
    void Update()
    {
        List<List<Position>> regionList;
        if (Input.GetMouseButtonDown(0))
        {
            regionList = GetGroundRegions();
        }
    }

	public void CreateTileMap(Size size)
    {
        _Tiles = new Tile[size.width, size.height];
        _Size = size;
    }

    public Tile[,] GetTileMap()
    {
        return _Tiles;
    }

    public bool IsValidPos(Position pos)
    {
        if (pos.x < 0 || pos.x >= _Size.width || pos.y < 0 || pos.y >= _Size.height)
        {
            return false;
        }
        return true;
    }

    List<List<Position>> GetGroundRegions()
    {
        List<List<Position>> regionList = new List<List<Position>>();
        var checkedFlags = new bool[_Size.width, _Size.height];

        for (int x = 0; x < _Size.width; ++x)
        {
            for (int y = 0; y < _Size.height; ++y)
            {
                if(!checkedFlags[x, y] && _Tiles[x, y].IsWalkable)
                {
                    var region = GetRegionTiles(new Position(x, y));
                    regionList.Add(region);

                    foreach(var pos in region)
                    {
                        checkedFlags[pos.x, pos.y] = true;
                    }
                }
            }
        }

        return regionList;
    }

    List<Position> GetRegionTiles(Position startPos)
    {
        var regionTileList = new List<Position>();
        var checkedFlags = new bool[_Size.width, _Size.height];
        var queue = new Queue<Position>();

        checkedFlags[startPos.x, startPos.y] = true;
        queue.Enqueue(startPos);

        while(queue.Count > 0)
        {
            var pos = queue.Dequeue();
            regionTileList.Add(pos);

            for (int x = pos.x - 1; x <= pos.x + 1; ++x)
            {
                for (int y = pos.y - 1; y <= pos.y + 1; ++y)
                {
                    if(!checkedFlags[x, y] && (x == pos.x || y == pos.y))
                    {
                        var newPos = new Position(x, y);
                        if(IsValidPos(newPos) && _Tiles[x, y].State == _Tiles[startPos.x, startPos.y].State)
                        {
                            checkedFlags[newPos.x, newPos.y] = true;
                            queue.Enqueue(newPos);
                        }
                    }
                }
            }
        }

        return regionTileList;
    }

    public Tile GetTile(Position pos)
    {
        if (!IsValidPos(pos))
        {
            return null;
        }
        return _Tiles[(int)pos.x, (int)pos.y];
    }

    public bool IsWalkableTile(Position pos)
    {
        if (!IsValidPos(pos))
        {
            return false;
        }
        else
        {
            return _Tiles[(int)pos.x, (int)pos.y].IsWalkable;
        }
    }

    public Tile GetRandomWalkableTile()
    {
        while(true)
        {
            int x = Random.Range(0, _Size.width);
            int y = Random.Range(0, _Size.height);

            var tile = _Tiles[x, y];

            if(tile.State == TileState.GROUND)
            {
                return tile;
            }
        }
    }
}
