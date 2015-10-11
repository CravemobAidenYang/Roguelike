using UnityEngine;
using System.Collections;


public class Player : MonoBehaviour//Unit
{
    public float _Speed;

    public int _MaxHP;
    protected int _HP;

    public int _AttackPower;

    bool _IsDead;
    public bool IsDead
    {
        get
        {
            return _IsDead;
        }
    }

    Position _Pos;
    public Position Pos
    {
        get
        {
            return _Pos;
        }
        protected set
        {
            _Pos = value;
        }
    }

    Transform _CachedTransform;
    Transform _CachedMainCamTransform;
    Animator _CachedAnimator;

    bool _IsAttack;

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

            _CachedTransform = this.transform;
            _CachedMainCamTransform = Camera.main.transform;
            _CachedAnimator = GetComponent<Animator>();

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
            _CachedTransform.position = Vector2.MoveTowards(_CachedTransform.position, targetPos, _Speed * Time.deltaTime);

            if (_CachedTransform.position == targetPos)
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
        TileManager.Instance.GetTile(targetPos).State = TileState.PLAYER;
        StartCoroutine(Move_Internal(targetPos.vector));
        _Pos = targetPos;
    }

    void Attack(Monster monster)
    {
        _IsAttack = true;
        _CachedAnimator.Play("PlayerAttack");

        monster.Hit(_AttackPower);
    }

    void AttackDone()
    {
        _IsAttack = false;
    }

    public void Hit(int damage)
    {
        print("Player.Hit");

        if (!_IsDead)
        {
            if (!_IsAttack)
            {
                _CachedAnimator.Play("PlayerHit");
            }

            _HP -= damage;

            if (_HP <= 0)
            {
                _IsDead = true;
            }
        }
    }

    bool CheckInput()
    {
        Position targetPos = _Pos;
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

        var targetTile = TileManager.Instance.GetTile(targetPos);
        if (targetTile.IsGround)
        {
            Move(targetPos);
            return true;
        }
        else if(targetTile.IsMonster)
        {
            Attack(MonsterManager.Instance.GetMonsterByPos(targetPos));
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
        var newCamPos = Vector3.MoveTowards(orgCamPos, _CachedTransform.position, 50 * Time.deltaTime);
        newCamPos.z = orgCamPos.z;

        _CachedMainCamTransform.position = newCamPos;
    }

    public void Init()
    {
        _HP = _MaxHP;

        _IsDead = false;
        _IsAttack = false;

        //var tile = TileManager.Instance.GetRandomWalkableTile();
        var pos = CastleMapGenerator.Instance.PlayerRoom.GetRandomPosInRoom();
        var tile = TileManager.Instance.GetTile(pos);

        _Pos = pos;
        _CachedTransform.position = _Pos.vector;
        tile.State = TileState.PLAYER;
        _IsProcessing = false;
        _CachedMainCamTransform.position = new Vector3(_CachedTransform.position.x, _CachedTransform.position.y, _CachedMainCamTransform.position.z);
    }


}
