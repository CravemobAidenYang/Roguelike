using UnityEngine;
using System.Collections;


public class Player : Unit
{
    //public float _Speed;

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

    protected override void OnAwake()
    {
        if (_Instance == null)
        {
            _Instance = this;

            _CachedMainCamTransform = Camera.main.transform;

            Init();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    IEnumerator Move_Internal(Vector3 targetPos)
    {
        _IsProcessing = true;
        while (true)
        {
            CachedTransform.position = Vector2.MoveTowards(CachedTransform.position, targetPos, _Speed * Time.deltaTime);

            if (CachedTransform.position == targetPos)
            {
                _IsProcessing = false;
                break;
            }
            yield return null;
        }
    }

    void Move(Position targetPos)
    {
        TileManager.Instance.GetTile(Pos).State = TileState.GROUND;
        TileManager.Instance.GetTile(targetPos).State = TileState.PLAYER;
        StartCoroutine(Move_Internal(targetPos.vector));
        Pos = targetPos;
    }

    bool CheckInput()
    {
        Position targetPos = Pos;
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

        if (TileManager.Instance.IsGroundTile(targetPos))
        {
            Move(targetPos);
            return true;
        }

        return false;
    }
    void Update()
    {
        if (!_IsProcessing && CheckInput())
        {
            //다른 유닛 프로세싱...
            //Monster.ProcessAllMonster();
            MonsterManager.Instance.ProcessAllMonster();
        }
    }

    void LateUpdate()
    {
        var orgCamPos = _CachedMainCamTransform.position;
        var newCamPos = Vector3.MoveTowards(orgCamPos, CachedTransform.position, 50 * Time.deltaTime);
        newCamPos.z = orgCamPos.z;

        _CachedMainCamTransform.position = newCamPos;
    }

    public void Init()
    {
        //var tile = TileManager.Instance.GetRandomWalkableTile();
        var pos = CastleMapGenerator.Instance.PlayerRoom.GetRandomPosInRoom();
        var tile = TileManager.Instance.GetTile(pos);

        Pos = pos;
        CachedTransform.position = Pos.vector;
        tile.State = TileState.PLAYER;
        _IsProcessing = false;
        _CachedMainCamTransform.position = new Vector3(CachedTransform.position.x, CachedTransform.position.y, _CachedMainCamTransform.position.z);
    }
}
