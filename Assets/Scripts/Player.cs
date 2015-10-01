using UnityEngine;
using System.Collections;


public class Player : MonoBehaviour 
{
    public float _Speed;

    Transform _CachedTransform;
    Vector2 _Pos;

    //이동 등의 작업이 진행 중이면 트루
    bool _IsProcessing = false;

	// Use this for initialization
	void Awake () 
    {
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

    void Move(Vector2 targetPos)
    {
        MapGenerator.Instance.GetTile(_Pos).IsWalkable = true;
        MapGenerator.Instance.GetTile(targetPos).IsWalkable = false;
        StartCoroutine(Move_Internal(targetPos));
        _Pos = targetPos;
    }

    bool CheckInput()
    {
        Vector2 targetPos = _Pos;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            targetPos.y++;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            targetPos.y--;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetPos.x--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetPos.x++;
        }

        if(MapGenerator.Instance.IsWalkableTile(targetPos))
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
        }
    }

    public void Init(Vector2 pos)
    {
        _Pos = pos;
        _CachedTransform = transform;

        _CachedTransform.position = pos;
        MapGenerator.Instance.GetTile(pos).IsWalkable = false;
    }
}
