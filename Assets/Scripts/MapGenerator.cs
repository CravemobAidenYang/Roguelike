using UnityEngine;
using System.Collections;

public struct Size
{
    public int width, height;

    public Size(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}

public class MapGenerator : MonoBehaviour 
{
    static MapGenerator _Instance = null;

    public static MapGenerator Instance
    {
        get
        {
            return _Instance;
        }
    }

    //생성된 타일들을 한데 모아두기 위한 용도의 트랜스폼
    public Transform _TileGroup;

    public GameObject _TilePrefab;
    public GameObject _Player;

    public Size _MapSize = new Size(100, 50);
    public float _StartAliveChance;
    public int _DeathLimit;
    public int _BirthLimit;
    public int _NumOfSteps;

    Cell[,] _Map;
    //Tile[,] _Tiles;

	// Use this for initialization
    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }
	
    void Init()
    {
        _Map = new Cell[_MapSize.width, _MapSize.height];
        TileManager.Instance.CreateTileMap(_MapSize);
        //_Tiles = new Tile[_MapSize.width, _MapSize.height];
    }

    void Start()
    {
        Generate();
    }

    void Update()
    {
    }

    void MapInit(Cell[,] map)
    {
        for (int x = 0; x < _MapSize.width; ++x)
        {
            for (int y = 0; y < _MapSize.height; ++y)
            {
                if (x == 0 || x == _MapSize.width - 1 || y == 0 || y == _MapSize.height - 1)
                {
                    map[x, y] = new Cell(false);
                }
                else
                {
                    bool alive = (Random.Range(0f, 1f) <= _StartAliveChance);
                    map[x, y] = new Cell(alive);
                }
            }
        }
    }

    int GetNumberOfAliveNeighbor(Cell[,] map, int xPos, int yPos)
    {
        int minX = Mathf.Max(0, xPos - 1);
        int maxX = Mathf.Min(_MapSize.width - 1, xPos + 1);
        int minY = Mathf.Max(0, yPos - 1);
        int maxY = Mathf.Min(_MapSize.height - 1, yPos + 1);

        int count = 0;
        for (int x = minX; x <= maxX; ++x)
        {
            for (int y = minY; y <= maxY; ++y)
            {
                if(x == xPos && y == yPos)
                {
                    continue;
                }

                if (map[x, y].Alive)
                {
                    ++count;
                }
            }
        }

        return count;
    }

    void DoSimulationStep(Cell[,] map)
    {
        var oldMap = map.Clone() as Cell[,];

        for (int x = 0; x < _MapSize.width; ++x)
        {
            for (int y = 0; y < _MapSize.height; ++y)
            {
                int numOfAliveNeighbor = GetNumberOfAliveNeighbor(oldMap, x, y);

                if (map[x, y].Alive)
                {
                    //살아있는 이웃이 death limit보다 적으면 죽인다.
                    map[x, y].Alive = (numOfAliveNeighbor >= _DeathLimit);
                }
                else
                {
                    //살아있는 이웃이 birth limit보다 많으면 살려낸다.
                    map[x, y].Alive = (numOfAliveNeighbor >= _BirthLimit);
                }
            }
        }
    }

    void SetTilesOnMap(Cell[,] map)
    {
        var tileMap = TileManager.Instance.GetTileMap();
        for (int x = 0; x < _MapSize.width; ++x)
        {
            for (int y = 0; y < _MapSize.height; ++y)
            {
                var tileGO = Instantiate(_TilePrefab) as GameObject;
                var tile = tileGO.GetComponent<Tile>();
                tileGO.transform.SetParent(_TileGroup);

                tile.Init(x, y, map[x, y].Alive);
                tileMap[x, y] = tile;
            }
        }
    }

	public void Generate()
    {
        MapInit(_Map);

        for (int i = 0; i < _NumOfSteps; ++i)
        {
            DoSimulationStep(_Map);
        }

        SetTilesOnMap(_Map);
    }
}
