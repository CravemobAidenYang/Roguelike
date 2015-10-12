using UnityEngine;
using System.Collections;

public enum TileState { GROUND, WALL, PLAYER, MONSTER};

[System.Serializable]
public class TileSaveData
{
    public TileState state;
    public int sprIndex;

    public TileSaveData(TileState state, int sprIndex)
    {
        this.state = state;
        this.sprIndex = sprIndex;
    }
}

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
    int _SprIndex;

    public bool IsUnit
    {
        get
        {
            return (_State == TileState.MONSTER || _State == TileState.PLAYER);
        }
    }

    public bool IsGround
    {
        get
        {
            return (_State == TileState.GROUND);
        }
    }

    public bool IsWall
    {
        get
        {
            return (_State == TileState.WALL);
        }
    }

    public bool IsMonster
    {
        get
        {
            return (_State == TileState.MONSTER);
        }
    }

    public bool IsPlayer
    {
        get
        {
            return (_State == TileState.PLAYER);
        }
    }

    public void SetState(TileState state, bool invalidate = true)
    {
        _State = state;

        if (invalidate)
        {
            if (IsWall)
            {
                _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomWallSprite(out _SprIndex);
                _BoxCollider.enabled = true;
            }
            else
            {
                _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomGroundSprite(out _SprIndex);
                _BoxCollider.enabled = false;
            }
        }
    }

    public TileState GetState()
    {
        return _State;
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

        this.SetState(state);

        _CachedTransform.SetParent(TileManager.Instance._TileGroup);
        //Invalidate();
    }

    public TileSaveData CreateSaveData()
    {
        return new TileSaveData(_State, _SprIndex);
    }

    public void ApplySaveData(TileSaveData data)
    {
        _State = data.state;
        _SprIndex = data.sprIndex;

        if(IsWall)
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetWallSpriteFromIndex(_SprIndex);
        }
        else
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetGroundSpriteFromIndex(_SprIndex);
        }
    }
}
