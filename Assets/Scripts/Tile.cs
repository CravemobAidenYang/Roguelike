using UnityEngine;
using System.Collections;

public enum TileState { GROUND, WALL, PLAYER, MONSTER, EXIT};

[System.Serializable]
public class TileSaveData
{
    public TileState state;
    public int sprIndex;
    public bool isVisited;

    public TileSaveData(TileState state, int sprIndex, bool isVisited)
    {
        this.state = state;
        this.sprIndex = sprIndex;
        this.isVisited = isVisited;
    }
}

public class Tile : MonoBehaviour 
{
    Position _Pos;

    //플레이어의 시야에 들어왔던 곳.
    bool _IsVisited;

    //현재 플레이어 시야에 들어있는 곳.
    bool _IsVisible;

    public bool IsVisted
    {
        get
        {
            return _IsVisited;
        }
        set
        {
            _IsVisited = value;
        }
    }

    public bool IsVisble
    {
        get
        {
            return _IsVisible;
        }
        set
        {
            _IsVisible = value;
        }
    }


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

    public bool IsExit
    {
        get
        {
            return (_State == TileState.EXIT);
        }
    }

    public bool HasFood
    {
        get
        {
            if(FoodManager.Instance.GetFoodByPos(_Pos) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
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
                //_BoxCollider.enabled = true;
            }
            else if(IsExit)
            {
                _SprRenderer.sprite = TileSpriteManager.Instance.GetExitSprite();
            }
            else
            {
                _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomGroundSprite(out _SprIndex);
                //_BoxCollider.enabled = false;
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

        _IsVisited = false;

        this.SetState(state);

        _CachedTransform.SetParent(TileManager.Instance._TileGroup);
        //Invalidate();
    }

    public TileSaveData CreateSaveData()
    {
        return new TileSaveData(_State, _SprIndex, _IsVisited);
    }

    public void ApplySaveData(TileSaveData data)
    {
        _State = data.state;
        _SprIndex = data.sprIndex;
        _IsVisited = data.isVisited;

        if(IsWall)
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetWallSpriteFromIndex(_SprIndex);
        }
        else if(IsExit)
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetExitSprite();
        }
        else
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetGroundSpriteFromIndex(_SprIndex);
        }
    }

    //인자로 전달된 값이 r,g,b에 전부 적용됨.
    public void SetColor(float rgb)
    {
        _SprRenderer.color = new Color(rgb, rgb, rgb);
    }
}
