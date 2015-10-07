using UnityEngine;
using System.Collections;


public class Player : Unit 
{
    public float _Speed;

    Transform _CachedTransform;
    Transform _CachedMainCamTransform;
    //Position _Pos;

    //이동 등의 작업이 진행 중이면 트루
    bool _IsProcessing = false;

    static Player _Instance = null;

    public static Player Instance
    {
        get
        {
            return _Instance;
        }
    }

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;

            _CachedTransform = transform;
            _CachedMainCamTransform = Camera.main.transform;

            var tile = TileManager.Instance.GetRandomWalkableTile();
            _Pos = tile.Pos;
            _CachedTransform.position = _Pos.vector;
            tile.State = TileState.UNIT;

            _CachedMainCamTransform.position = new Vector3(_CachedTransform.position.x, _CachedTransform.position.y, _CachedMainCamTransform.position.z);
        }
        else
        {
            Destroy(this.gameObject);
        }
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
        TileManager.Instance.GetTile(_Pos).State = TileState.GROUND;
        TileManager.Instance.GetTile(targetPos).State = TileState.UNIT;
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

    void LateUpdate()
    {
        var orgCamPos = _CachedMainCamTransform.position;
        var newCamPos = Vector3.MoveTowards(orgCamPos, _CachedTransform.position, 50 * Time.deltaTime);
        newCamPos.z = orgCamPos.z;

        _CachedMainCamTransform.position = newCamPos;
    }

    //public void Init(Position pos)
    //{
    //    _Pos = pos;
    //    _CachedTransform = transform;

    //    _CachedTransform.position = pos.vector;
    //    TileManager.Instance.GetTile(pos).state = TileState.UNWALKABLE;
    //}
}
