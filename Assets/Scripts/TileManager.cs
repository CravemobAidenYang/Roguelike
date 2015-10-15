using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class TileMapSaveData
{
    public Size size;
    public TileSaveData[,] tileSaveDatas;

    public TileMapSaveData(Size size, TileSaveData[,] tileSaveDatas)
    {
        this.size = size;
        this.tileSaveDatas = tileSaveDatas;
    }
}

public class TileManager : MonoBehaviour
{
    public Tile _TilePrefab;

    //생성된 타일들을 묶어두기 위한 용도
    public Transform _TileGroup;

    static TileManager _Instance = null;

    public static TileManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    public float _VisitedTileColor;
    public float VisitedTileColor
    {
        get
        {
            return _VisitedTileColor;
        }
    }

    Tile[,] _Tiles;
    Size _Size;

    public void ResetTilesVisibleInfo()
    {
        foreach(var tile in _Tiles)
        {
            tile.IsVisble = false;
        }
    }

    public void SetVisitedTilesColor()
    {
        foreach(var tile in _Tiles)
        {
            if(tile.IsVisted)
            {
                tile.SetColor(_VisitedTileColor);

                if (tile.IsMonster)
                {
                    //몬스터는 안보여야함.
                    var monster = MonsterManager.Instance.GetMonsterByPos(tile.Pos);
                    monster.SetColor(0f);
                }
                if(tile.HasFood)
                {
                    var food = FoodManager.Instance.GetFoodByPos(tile.Pos);
                    food.SetColor(_VisitedTileColor);
                }
            }
        }
    }

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
    }

	public void CreateTileMap(Size size)
    {
        DestroyTileMap();
        _Tiles = new Tile[size.width, size.height];

        for (int x = 0; x < size.width; ++x)
        {
            for (int y = 0; y < size.height; ++y)
            {
                var tile = Instantiate(_TilePrefab) as Tile;

                tile.Init(x, y, TileState.WALL);
                _Tiles[x, y] = tile;
            }
        }
        _Size = size;
    }

    public void DestroyTileMap()
    {
        if (_Tiles != null)
        {
            foreach(var tile in _Tiles)
            {
                if(tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }
        }
        _Tiles = null;
        _Size = new Size(0, 0);
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
                if(!checkedFlags[x, y] && _Tiles[x, y].IsGround)
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
                        if(IsValidPos(newPos) && _Tiles[x, y].GetState() == _Tiles[startPos.x, startPos.y].GetState())
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

    public bool IsGroundTile(Position pos)
    {
        if (!IsValidPos(pos))
        {
            return false;
        }
        else
        {
            return _Tiles[(int)pos.x, (int)pos.y].IsGround;
        }
    }

    public Tile GetRandomWalkableTile()
    {
        while(true)
        {
            int x = Random.Range(0, _Size.width);
            int y = Random.Range(0, _Size.height);

            var tile = _Tiles[x, y];

            if(tile.IsGround)
            {
                return tile;
            }
        }
    }

    public void SaveTileMap()
    {
        TileSaveData[,] tileDatas = new TileSaveData[_Size.width, _Size.height];
        for (int x = 0; x < _Size.width; ++x)
        {
            for (int y = 0; y < _Size.height; ++y)
            {
                tileDatas[x, y] = _Tiles[x, y].CreateSaveData();
            }
        }

        SaveLoad.SaveData("TileMapSaveData", new TileMapSaveData(_Size, tileDatas));
    }

    public void LoadTileMap()
    {
        DestroyTileMap();

        var tileMapSaveData = SaveLoad.LoadData<TileMapSaveData>("TileMapSaveData", null);

        CreateTileMap(tileMapSaveData.size);

        for (int x = 0; x < _Size.width; ++x)
        {
            for (int y = 0; y < _Size.height; ++y)
            {
                _Tiles[x, y].ApplySaveData(tileMapSaveData.tileSaveDatas[x, y]);
            }
        }
    }
}
