using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerSaveData
{
    public Position pos;

    public int maxHP, hp;
    public int attackPower;
    public float speed;

    public float scaleX;

    public bool isDead;

    public PlayerSaveData(Position pos, int maxHP, int hp, int attackPower, float speed, float scaleX, bool isDead)
    {
        this.pos = pos;
        this.maxHP = maxHP;
        this.hp = hp;
        this.attackPower = attackPower;
        this.speed = speed;
        this.scaleX = scaleX;
        this.isDead = isDead;
    }
}

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
    public bool IsProcessing
    {
        get
        {
            return _IsProcessing;
        }
    }

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
                MonsterManager.Instance.ProcessAllMonster();
                break;
            }
            yield return null;
        }
    }

    void Move(Position targetPos)
    {
        TileManager.Instance.GetTile(_Pos).SetState(TileState.GROUND, false);
        //TileManager.Instance.GetTile(_Pos).State = TileState.GROUND;
        TileManager.Instance.GetTile(targetPos).SetState(TileState.PLAYER, false);
        //TileManager.Instance.GetTile(targetPos).State = TileState.PLAYER;
        StartCoroutine(Move_Internal(targetPos.vector));
        _Pos = targetPos;
    }

    void Attack(Monster monster)
    {
        print("attack");
        _IsProcessing = true;
        _IsAttack = true;

        //TODO : 여기 보던중
        //_CachedAnimator.StopPlayback();
        _CachedAnimator.Play("PlayerAttack");

        monster.Hit(_AttackPower);
    }

    void AttackDone()
    {
        print("attack done");
        MonsterManager.Instance.ProcessAllMonster();
        _IsProcessing = false;
        _IsAttack = false;
    }

    public void Hit(Monster attacker, int damage)
    {
        print("Player.Hit");

        //LookDir(attacker.Pos - _Pos);

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
                GameManager.Instance.GameOver();
            }
        }
    }

    void LookDir(Position dir)
    {
        if (dir.x < 0)
        {
            _CachedTransform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dir.x > 0)
        {
            _CachedTransform.localScale = new Vector3(1, 1, 1);
        }
    }

    void CheckInput()
    {
        Position moveDir = new Position(0, 0);
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveDir = Position.up;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            moveDir = Position.down;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveDir = Position.left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            moveDir = Position.right;
        }

        LookDir(moveDir);

        var targetPos = _Pos + moveDir;
        var targetTile = TileManager.Instance.GetTile(targetPos);
        if (targetTile.IsGround)
        {
            Move(targetPos);
        }
        else if(targetTile.IsMonster)
        {
            Attack(MonsterManager.Instance.GetMonsterByPos(targetPos));
        }
    }

    void Update()
    {
        if(GameManager.Instance.IsPause)
        {
            return;
        }

        if(_IsDead)
        {
            return;
        }

        if (!_IsProcessing)
        {
            CheckInput();
            //다른 유닛 프로세싱...
            //Monster.ProcessAllMonster();
            //MonsterManager.Instance.ProcessAllMonster();
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
        Position pos;
        Tile tile;
        do
        {
            pos = CastleMapGenerator.Instance.PlayerRoom.GetRandomPosInRoom();
            tile = TileManager.Instance.GetTile(pos);
        } while (!tile.IsGround);

        _Pos = pos;
        _CachedTransform.position = _Pos.vector;
        tile.SetState(TileState.PLAYER, false);
        //tile.State = TileState.PLAYER;
        _IsProcessing = false;
        _CachedMainCamTransform.position = new Vector3(_CachedTransform.position.x, _CachedTransform.position.y, _CachedMainCamTransform.position.z);
    }

    public void SavePlayerData()
    {
        var data = new PlayerSaveData(_Pos, _MaxHP, _HP, _AttackPower, _Speed, _CachedTransform.localScale.x, _IsDead);

        SaveLoad.SaveData("PlayerSaveData", data);
    }

    public void LoadPlayerData()
    {
        var data = SaveLoad.LoadData<PlayerSaveData>("PlayerSaveData", null);
        ApplySaveData(data);
    }

    void ApplySaveData(PlayerSaveData data)
    {
        _Pos = data.pos;
        _MaxHP = data.maxHP;
        _HP = data.hp;
        _Speed = data.speed;
        _AttackPower = data.attackPower;

        _CachedTransform.position = _Pos.vector;
        _CachedTransform.localScale = new Vector3(data.scaleX, 1, 1);

        _CachedMainCamTransform.position = new Vector3(_CachedTransform.position.x, _CachedTransform.position.y, _CachedMainCamTransform.position.z);

        _IsDead = data.isDead;

        TileManager.Instance.GetTile(_Pos).SetState(TileState.PLAYER, false);
    }
}
