using UnityEngine;
using System.Collections;

public enum TileState { GROUND, WALL, UNIT};

public class Tile : MonoBehaviour 
{
    Position _Pos;

    public Position Pos
    {
        get
        {
            return _Pos;
        }
    }

    SpriteRenderer _SprRenderer;
    BoxCollider2D _BoxCollider;
    Transform _CachedTransform;

    TileState _State;

    public TileState State
    {
        get
        {
            return _State;
        }
        set
        {
            if(_State != value)
            {
                if (_State != TileState.UNIT && value == TileState.GROUND)
                {
                    _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomGroundSprite();
                    _BoxCollider.enabled = false;
                }
                else if (value == TileState.WALL)
                {
                    _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomWallSprite();
                    _BoxCollider.enabled = true;
                }
                _State = value;
            }
        }
    }

    public bool IsWalkable
    {
        get
        {
            return (_State == TileState.GROUND);
        }
    }

	// Use this for initialization
	void Awake () 
    {
        _SprRenderer = GetComponent<SpriteRenderer>();
        _BoxCollider = GetComponent<BoxCollider2D>();
        _CachedTransform = transform;
	}
	
	public void Init(int x, int y, TileState state)
    {
        _CachedTransform.position = new Vector2(x, y);
        _Pos = new Position(x, y);

        this.State = state;

        _CachedTransform.SetParent(TileManager.Instance._TileGroup);
    }
}
