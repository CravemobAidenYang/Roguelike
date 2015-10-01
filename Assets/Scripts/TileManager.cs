using UnityEngine;
using System.Collections;

public class TileManager : MonoBehaviour
{
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
	
	public void CreateTileMap(Size size)
    {
        _Tiles = new Tile[size.width, size.height];
        _Size = size;
    }

    public Tile[,] GetTileMap()
    {
        return _Tiles;
    }

    public bool IsValidPos(Vector2 pos)
    {
        if (pos.x < 0 || pos.x >= _Size.width || pos.y < 0 || pos.y >= _Size.height)
        {
            return false;
        }
        return true;
    }

    public Tile GetTile(Vector2 pos)
    {
        if (!IsValidPos(pos))
        {
            return null;
        }
        return _Tiles[(int)pos.x, (int)pos.y];
    }

    public bool IsWalkableTile(Vector2 pos)
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
}
