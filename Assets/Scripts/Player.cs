using UnityEngine;
using System.Collections;


public class Player : Unit 
{
    public float _Speed;

    Transform _CachedTransform;
    //Position _Pos;

    //이동 등의 작업이 진행 중이면 트루
    bool _IsProcessing = false;

	// Use this for initialization
	void Awake () 
    {
        print("Player Awake");
        _CachedTransform = transform;
	}

    IEnumerator Move_Internal(Vector3 targetPos)
    {
        _IsProcessing = true;
        while(true)
        {
            _CachedTransform.position = Vector2.MoveTowards(_CachedTransform.position, targetPos, _Speed * Time.deltaTime);
            
            if(_CachedTransform.position == targetPos)
            {
                _IsProcessing = false;
                break;
            }
            yield return null;
        }
    }

    void Move(Position targetPos)
    {
        TileManager.Instance.GetTile(_Pos).state = TileState.UNWALKABLE;
        TileManager.Instance.GetTile(targetPos).state = TileState.WALKABLE;
        StartCoroutine(Move_Internal(targetPos.vector));
        _Pos = targetPos;
    }

    bool CheckInput()
    {
        Position targetPos = _Pos;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            targetPos.y++;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            targetPos.y--;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            targetPos.x--;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            targetPos.x++;
        }

        if(TileManager.Instance.IsWalkableTile(targetPos))
        {
            Move(targetPos);
            return true;
        }

        return false;
    }
    void Update()
    {
        if(!_IsProcessing && CheckInput())
        {
            //다른 유닛 프로세싱...
            Monster.ProcessAllMonster();
        }
    }

    public void Init(Position pos)
    {
        _Pos = pos;
        _CachedTransform = transform;

        _CachedTransform.position = pos.vector;
        TileManager.Instance.GetTile(pos).state = TileState.UNWALKABLE;
    }
}
