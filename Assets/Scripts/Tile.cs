using UnityEngine;
using System.Collections;

public enum TileState { WALKABLE, UNWALKABLE};

public class Tile : MonoBehaviour 
{
    Position _Pos;

    SpriteRenderer _SprRenderer;
    BoxCollider2D _BoxCollider;
    Transform _CachedTransform;

    TileState _State;

    public TileState state
    {
        get
        {
            return _State;
        }
        set
        {
            _State = value;
        }
    }

    public bool IsWalkable
    {
        get
        {
            return (_State == TileState.WALKABLE);
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

        _State = state;

        if (_State == TileState.WALKABLE)
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomGroundSprite();
        }
        else
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomWallSprite();
            _BoxCollider.enabled = true;
        }
    }
}
