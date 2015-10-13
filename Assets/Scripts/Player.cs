using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerSaveData
{
    public Position pos;

    public int maxHP, hp;
    public int minAttackPower, maxAttackPower;
    public float criticalProbability;
    public int minCriAttackPower, maxCriAttackPower;
    public float speed;

    public float scaleX;

    public bool isDead;

    public PlayerSaveData(Position pos, int maxHP, int hp, int minAttackPower, int maxAttackPower, float critical, int minCri, int maxCri, float speed, float scaleX, bool isDead)
    {
        this.pos = pos;
        this.maxHP = maxHP;
        this.hp = hp;
        this.minAttackPower = minAttackPower;
        this.maxAttackPower = maxAttackPower;
        this.minCriAttackPower = minCri;
        this.maxCriAttackPower = maxCri;
        this.criticalProbability = critical;
        this.speed = speed;
        this.scaleX = scaleX;
        this.isDead = isDead;
    }
}

public class Player : MonoBehaviour//Unit
{
    public DamageLabel _HealLabelPrefab;
    public DamageLabel _DamageLabelPrefab;
    public DamageLabel _CriticalDamageLabelPrefab;

    public UISlider _HPBarPrefab;
    UISlider _HPBar;

    public float _Speed;

    public int _MaxHP;
    int _HP;

    public int _MinAttackPower, _MaxAttackPower;
    public float _CriticalProbability;
    public int _MinCriAttackPower, _MaxCriAttackPower;

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

            _HPBar = Instantiate(_HPBarPrefab);

            Init();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        //_HPBar.transform.SetParent(GameManager.Instance.HUD.transform);
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
                print("Move Done");

                var food = FoodManager.Instance.GetFoodFromPos(_Pos);
                if(food != null)
                {
                    Eat(food);
                }

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

        _CachedAnimator.Play("PlayerAttack", -1, 0f);

        if (Random.Range(0f, 1f) <= _CriticalProbability)
        {
            monster.Hit(Random.Range(_MinCriAttackPower, _MaxCriAttackPower + 1), true);
        }
        else
        {
            monster.Hit(Random.Range(_MinAttackPower, _MaxAttackPower + 1), false);
        }
    }

    void AttackDone()
    {
        print("attack done");
        MonsterManager.Instance.ProcessAllMonster();
        _IsProcessing = false;
        _IsAttack = false;
    }

    public void Hit(Monster attacker, int damage, bool isCritical)
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
            InvalidateHPBar();

            DamageLabel damageLabel;
            if(isCritical)
            {
                damageLabel = Instantiate(_CriticalDamageLabelPrefab);
            }
            else
            {
                damageLabel = Instantiate(_DamageLabelPrefab);
            }
            damageLabel.SetTargetWorldPos(_CachedTransform.position + new Vector3(0, 1, 0));
            damageLabel.text = damage.ToString();


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

    void Eat(Food food)
    {
        print("food Heal Point : " + food.HealPoint);
        _HP += food.HealPoint;

        DamageLabel healLabel = Instantiate(_HealLabelPrefab);
        healLabel.SetTargetWorldPos(_CachedTransform.position + new Vector3(0, 1, 0));
        healLabel.text = "+" + food.HealPoint.ToString();

        if(_HP > _MaxHP)
        {
            _HP = _MaxHP;
        }

        InvalidateHPBar();
        FoodManager.Instance.RemoveFood(food);
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

    Vector3 GetUIScreenPos(Vector3 pos)
    {
        var viewPos = Camera.main.WorldToViewportPoint(pos);

        var uiScreenPos = UICamera.currentCamera.ViewportToWorldPoint(viewPos);
        return uiScreenPos;
    }

    void LateUpdate()
    {
        var orgCamPos = _CachedMainCamTransform.position;
        var newCamPos = Vector3.MoveTowards(orgCamPos, _CachedTransform.position, 50 * Time.deltaTime);
        newCamPos.z = orgCamPos.z;

        _CachedMainCamTransform.position = newCamPos;


        var uiScreenPos = GetUIScreenPos(_CachedTransform.position + new Vector3(0f, 0.6f, 0f));
        _HPBar.transform.position = uiScreenPos;
    }

    public void Init()
    {
        _HP = _MaxHP;

        InvalidateHPBar();

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
        var data = new PlayerSaveData(_Pos, _MaxHP, _HP,
            _MinAttackPower, _MaxAttackPower, _CriticalProbability, _MinCriAttackPower, _MaxCriAttackPower,
            _Speed, _CachedTransform.localScale.x, _IsDead);

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
        _MinAttackPower = data.minAttackPower;
        _MaxAttackPower = data.maxAttackPower;
        _MinCriAttackPower = data.minCriAttackPower;
        _MaxCriAttackPower = data.maxCriAttackPower;
        _CriticalProbability = data.criticalProbability;

        InvalidateHPBar();

        _CachedTransform.position = _Pos.vector;
        _CachedTransform.localScale = new Vector3(data.scaleX, 1, 1);

        _CachedMainCamTransform.position = new Vector3(_CachedTransform.position.x, _CachedTransform.position.y, _CachedMainCamTransform.position.z);

        _IsDead = data.isDead;

        TileManager.Instance.GetTile(_Pos).SetState(TileState.PLAYER, false);
    }

    void InvalidateHPBar()
    {
        _HPBar.value = (float)_HP / (float)_MaxHP;
    }
}
