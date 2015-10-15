using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerSaveData
{
    public Position pos;

    public int maxHP, hp;
    public int minAttackPower, maxAttackPower;
    public float criticalProbability;
    public int minCriAttackPower, maxCriAttackPower;
    public float speed;
    public int killCount;
    public int traveledDungeonCount;

    public float scaleX;

    public bool isDead;

    public PlayerSaveData(Position pos, int maxHP, int hp,
        int minAttackPower, int maxAttackPower, float critical, int minCri, int maxCri,
        float speed, int killCount, int traveledDungeonCount, float scaleX, bool isDead)
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
        this.killCount = killCount;
        this.traveledDungeonCount = traveledDungeonCount;
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

    public int _Sight;

    public float _Speed;

    public int _MaxHP;
    int _HP;

    int _KillCount;
    int _TraveledDungeonCount;

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

    //void Start()
    //{
    //    //_HPBar.transform.SetParent(GameManager.Instance.HUD.transform);
    //}

    IEnumerator Move_Internal(Vector3 targetPos)
    {
        _IsProcessing = true;
        while (true)
        {
            _CachedTransform.position = Vector2.MoveTowards(_CachedTransform.position, targetPos, _Speed * Time.deltaTime);

            if (_CachedTransform.position == targetPos)
            {
                print("Move Done");
                _IsProcessing = false;
                MoveDone();
                break;
            }
            yield return null;
        }
    }

    void MoveDone()
    {
        var food = FoodManager.Instance.GetFoodByPos(_Pos);
        if (food != null)
        {
            Eat(food);
        }

        MonsterManager.Instance.ProcessAllMonster();

        CalcSight();
    }

    void Move(Position targetPos)
    {
        TileManager.Instance.GetTile(_Pos).SetState(TileState.GROUND, false);
        TileManager.Instance.GetTile(targetPos).SetState(TileState.PLAYER, false);

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

    public void AddKillCount()
    {
        _KillCount++;
    }

    public void Hit(int damage, bool isCritical)
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
                GameManager.Instance.GameOver(_KillCount, _TraveledDungeonCount);
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
        else if(targetTile.IsExit)
        {
            GameManager.Instance.ShowGoToNextPanel();
            _TraveledDungeonCount++;
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

        CalcSight();
    }

    public void Init()
    {
        _KillCount = 0;
        _TraveledDungeonCount = 1;

        _HP = _MaxHP;

        InvalidateHPBar();

        _IsDead = false;
        _IsAttack = false;

        SetPosInStartRoom();

        _IsProcessing = false;
        _CachedMainCamTransform.position = new Vector3(_CachedTransform.position.x, _CachedTransform.position.y, _CachedMainCamTransform.position.z);

        CalcSight();
    }

    public void SetPosInStartRoom()
    {
        Position? pos;
        Tile tile;

        int tryCount = 0;
        do
        {
            pos = CastleMapGenerator.Instance.PlayerRoom.GetRandomPosInRoom();
            tile = TileManager.Instance.GetTile(pos.Value);

            if (tryCount > 1000)
            {
                break;
            }

            tryCount++;
        } while (!tile.IsGround || tile.HasFood);

        _Pos = pos.Value;
        _CachedTransform.position = _Pos.vector;
        tile.SetState(TileState.PLAYER, false);
    }

    public void SavePlayerData()
    {
        var data = new PlayerSaveData(_Pos, _MaxHP, _HP,
            _MinAttackPower, _MaxAttackPower, _CriticalProbability, _MinCriAttackPower, _MaxCriAttackPower,
            _Speed, _KillCount, _TraveledDungeonCount, _CachedTransform.localScale.x, _IsDead);

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
        _KillCount = data.killCount;
        _TraveledDungeonCount = data.traveledDungeonCount;
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

    public void CalcSight()
    {
        var sightMin = new Position(_Pos.x - _Sight, _Pos.y - _Sight);
        var sightMax = new Position(_Pos.x + _Sight, _Pos.y + _Sight);

        //타일을 모두 보이지 않음으로 설정.
        TileManager.Instance.ResetTilesVisibleInfo();

        //이미 봤던 영역은 먼저 색을 정해준다.
        TileManager.Instance.SetVisitedTilesColor();

        for (int x = sightMin.x; x <= sightMax.x; ++x)
        {
            for (int y = sightMin.y; y <= sightMax.y; ++y)
            {
                //사각형의 시야의 끝에 해당하는 타일들에 레이를 쏘고 타일의 색을 지정한다.. 
                if (x == sightMin.x || x == sightMax.x ||
                    y == sightMin.y || y == sightMax.y)
                {
                    ShootRayAndCheckVisible(x, y);
                }
            }
        }

        CalcSightPostProcess();
    }

    void CalcSightPostProcess()
    {
        var sightMin = new Position(_Pos.x - _Sight, _Pos.y - _Sight);
        var sightMax = new Position(_Pos.x + _Sight, _Pos.y + _Sight);

        for (int x = sightMin.x; x <= sightMax.x; ++x)
        {
            for (int y = sightMin.y; y <= sightMax.y; ++y)
            {
                //대각선 상에 위치한 타일들에 대해
                if (x != _Pos.x && y != _Pos.y)
                {
                    var tile = TileManager.Instance.GetTile(new Position(x, y));

                    if (tile == null)
                    {
                        continue;
                    }

                    float dist = (_CachedTransform.position - tile.Pos.vector3).sqrMagnitude;

                    //원 영역안에 들어오면
                    if (dist <= _Sight * _Sight)
                    {
                        if (tile.IsWall)
                        {
                            //대각선 방향을 구한다.
                            int dirX = (_Pos.x < x) ? 1 : -1;
                            int dirY = (_Pos.y < y) ? 1 : -1;

                            //체크하고자 하는 타일로부터 구한 방향의 반대에 위치한 3개의 타일위치를 구한다.
                            var posArr = new Position[3];
                            posArr[0] = new Position(tile.Pos.x - dirX, tile.Pos.y - dirY);
                            posArr[1] = new Position(tile.Pos.x, tile.Pos.y - dirY);
                            posArr[2] = new Position(tile.Pos.x - dirX, tile.Pos.y);

                            for (int i = 0; i < posArr.Length; ++i)
                            {
                                var checkTile = TileManager.Instance.GetTile(posArr[i]);

                                if (!checkTile.IsWall && checkTile.IsVisble)
                                {
                                    float color = 1f - Mathf.Min(1f, dist / (_Sight * _Sight));
                                    color = Mathf.Max(TileManager.Instance.VisitedTileColor, color);

                                    SetVisibleTile(tile, color);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void ShootRayAndCheckVisible(int x, int y)
    {
        var curPosVec = _CachedTransform.position;
        curPosVec.z = 0;

        var hitInfoArr = Physics2D.LinecastAll(curPosVec, new Vector2(x, y), 1 << LayerMask.NameToLayer("Tile"));

        //레이에 충돌한 타일들을 순회하며
        foreach (var hitInfo in hitInfoArr)
        {
            if (hitInfo.collider != null)
            {
                var tile = hitInfo.collider.gameObject.GetComponent<Tile>();

                float dist = (curPosVec - tile.Pos.vector3).sqrMagnitude;

                //원모양 시야에 들어왔을때
                if (dist <= _Sight * _Sight)
                {
                    float color = 1f - Mathf.Min(1f, dist / (_Sight * _Sight));
                    color = Mathf.Max(TileManager.Instance.VisitedTileColor, color);

                    SetVisibleTile(tile, color);

                    //벽인 경우 더이상 검사하지 않고 다음 레이로 넘어간다.
                    if (tile.IsWall)
                    {
                        break;
                    }
                }
            }
        }
    }

    //타일의 색을 지정하고 그 위의 몬스터와 푸드도 보일지 말지 결정
    void SetVisibleTile(Tile tile, float rgb)
    {
        tile.SetColor(rgb);
        tile.IsVisble = true;
        tile.IsVisted = true;
        //타일위에 있는 몬스터와 음식에도 같은 명암을 적용한다.
        if (tile.IsMonster)
        {
            var monster = MonsterManager.Instance.GetMonsterByPos(tile.Pos);
            monster.SetColor(rgb);
        }
        if (tile.HasFood)
        {
            var food = FoodManager.Instance.GetFoodByPos(tile.Pos);
            food.SetColor(rgb);
        }
    }
}
