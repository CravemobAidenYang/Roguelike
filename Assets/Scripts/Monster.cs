using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MonsterSaveData
{
    public int monsterIndex;

    public Position pos;
    public int maxHP, hp;
    public int minAttackPower, maxAttackPower;
    public float criticalProbability;
    public int minCriAttackPower, maxCriAttackPower;
    public int halfAreaX, halfAreaY;
    public float scaleX;
    public float speed;

    public MonsterSaveData(int monsterIndex, Position pos, int maxHP, int hp,
        int minAttackPower, int maxAttackPower, float critical, int minCri, int maxCri,
        int halfAreaX, int halfAreaY, float scaleX, float speed)
    {
        this.monsterIndex = monsterIndex;
        this.pos = pos;
        this.maxHP = maxHP;
        this.hp = hp;
        this.minAttackPower = minAttackPower;
        this.maxAttackPower = maxAttackPower;
        this.minCriAttackPower = minCri;
        this.maxCriAttackPower = maxCri;
        this.criticalProbability = critical;
        this.halfAreaX = halfAreaX;
        this.halfAreaY = halfAreaY;
        this.scaleX = scaleX;
        this.speed = speed;
    }
}

public class Monster : MonoBehaviour//Unit
{
    public DamageLabel _CriticalDamageLabelPrefab;
    public DamageLabel _DamageLabelPrefab;

    public UISlider _HPBarPrefab;
    UISlider _HPBar;

    Transform _CachedTransform;
    Animator _CachedAnimator;
    SpriteRenderer _SprRenderer;

    int _MonsterIndex;

    public int _MaxHP;
    protected int _HP;

    public int HP
    {
        get
        {
            return _HP;
        }
    }

    public float _Speed;

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

    bool _IsDead;
    public bool IsDead
    {
        get
        {
            return _IsDead;
        }
    }

    //현재 Pos에 위치한 타일을 얻어옴
    public Tile CurTile
    {
        get
        {
            return TileManager.Instance.GetTile(_Pos);
        }
    }

    public int _HalfAreaX, _HalfAreaY;

    public int _MinAttackPower, _MaxAttackPower;

    public float _CriticalProbability;
    public int _MinCriAttackPower, _MaxCriAttackPower;

    bool _IsAlreadyActioned = false;

    public void InitAction()
    {
        _IsAlreadyActioned = false;
    }

    void Awake()
    {
        _CachedTransform = this.transform;
        _CachedAnimator = GetComponent<Animator>();
        _SprRenderer = GetComponent<SpriteRenderer>();

        _HPBar = Instantiate(_HPBarPrefab);
        SetColor(0f);

    }

    void InvalidateHPBar()
    {
        _HPBar.value = (float)_HP / (float)_MaxHP;
    }

    void LateUpdate()
    {
        var screenPos = Camera.main.WorldToScreenPoint(_CachedTransform.position + new Vector3(0f, 0.6f, 0f));

        var uiScreenPos = UICamera.currentCamera.ScreenToWorldPoint(screenPos);
        _HPBar.transform.position = uiScreenPos;
    }

    public void Cleanup()
    {
        Destroy(_HPBar.gameObject);
    }

    public void Action()
    {
        if(GameManager.Instance.IsPause)
        {
            return;
        }
        if(_IsAlreadyActioned)
        {
            return;
        }

        _IsAlreadyActioned = true;


        if(IsPlayerInMyArea())
        {
            var playerPos = Player.Instance.Pos;
            var moveDirQueue = new Queue<Position>();

            LookDir(playerPos - _Pos);

            if(playerPos == _Pos + Position.left || playerPos == _Pos + Position.right ||
                playerPos == _Pos + Position.up || playerPos == _Pos + Position.down)
            {
                Attack();
                return;
            }

            //x방향으로의 길이가 더 멀다면 x방향으로의 이동명령을 큐에 먼저넣음.
            //TODO: (메소드 분할 필요)
            if (Mathf.Abs(playerPos.x - _Pos.x) > Mathf.Abs(playerPos.y - _Pos.y))
            {
                if (playerPos.x > _Pos.x)
                {
                    moveDirQueue.Enqueue(Position.right);
                }
                else if(playerPos.x < _Pos.x)
                {
                    moveDirQueue.Enqueue(Position.left);
                }

                if (playerPos.y > _Pos.y)
                {
                    moveDirQueue.Enqueue(Position.up);
                }
                else if (playerPos.y < _Pos.y)
                {
                    moveDirQueue.Enqueue(Position.down);
                }
            }
            else
            {
                if (playerPos.y > _Pos.y)
                {
                    moveDirQueue.Enqueue(Position.up);
                }
                else if (playerPos.y < _Pos.y)
                {
                    moveDirQueue.Enqueue(Position.down);
                }

                if (playerPos.x > _Pos.x)
                {
                    moveDirQueue.Enqueue(Position.right);
                }
                else if (playerPos.x < _Pos.x)
                {
                    moveDirQueue.Enqueue(Position.left);
                }
            }

            Move(moveDirQueue);
        }
    }

    void Attack()
    {
        print("Monster.Attack");

        if(!Player.Instance.IsDead)
        {
            _CachedAnimator.Play("MonsterAttack");
            if(Random.Range(0f, 1f) <= _CriticalProbability)
            {
                Player.Instance.Hit(Random.Range(_MinCriAttackPower, _MaxCriAttackPower + 1), true);
            }
            else
            {
                Player.Instance.Hit(Random.Range(_MinAttackPower, _MaxAttackPower + 1), false);
            }
        }
    }

    public void Hit(int damage, bool isCritical)
    {
        print("Monster.Hit");

        LookDir(Player.Instance.Pos - _Pos);

        if (!_IsDead)
        {
            _HP -= damage;

            DamageLabel damageLabel;
            if (isCritical)
            {
                damageLabel = Instantiate(_CriticalDamageLabelPrefab);
            }
            else
            {
                damageLabel = Instantiate(_DamageLabelPrefab);
            }
            damageLabel.SetTargetWorldPos(_CachedTransform.position + new Vector3(0, 1, 0));
            damageLabel.text = damage.ToString();

            InvalidateHPBar();
            
            if (_HP <= 0)
            {
                _IsDead = true;
                Die();
            }
        }
    }

    void Die()
    {
        CurTile.SetState(TileState.GROUND, false);
        //CurTile.State = TileState.GROUND;
        MonsterManager.Instance.RemoveMonsterFromList(this);
        Cleanup();
        Destroy(gameObject);
        Player.Instance.AddKillCount();
        //this.gameObject.SetActive(false);
    }

    IEnumerator Move_Internal(Vector3 targetPos)
    {
        //print("monster move internal");
        while (true)
        {
            _CachedTransform.position = Vector2.MoveTowards(_CachedTransform.position, targetPos, _Speed * Time.deltaTime);

            if (_CachedTransform.position == targetPos)
            {
                break;
            }
            yield return null;
        }
    }

    void LookDir(Position dir)
    {
        if (dir.x < 0)
        {
            _CachedTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (dir.x > 0)
        {
            _CachedTransform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void Move(Queue<Position> moveDirQueue)
    {
        while(moveDirQueue.Count > 0)
        {
            //이동하고자 하는 위치를 꺼내온다.
            var moveDir = moveDirQueue.Dequeue();
            var targetPos = new Position(_Pos.x + moveDir.x, _Pos.y + moveDir.y);
            var targetTile = TileManager.Instance.GetTile(targetPos);

            //이동하고자 하는 타일이 땅이라면 바로 이동한다.
            if (TileManager.Instance.IsGroundTile(targetPos))
            {
                CurTile.SetState(TileState.GROUND, false);
                //CurTile.State = TileState.GROUND;
                targetTile.SetState(TileState.MONSTER, false);
                //targetTile.State = TileState.MONSTER;
                _Pos = targetPos;
                StopCoroutine("Move_Internal");
                StartCoroutine("Move_Internal", targetPos.vector3);
                //StartCoroutine(Move_Internal(targetPos.vector));
                return;
            }
            //땅이 아니라면 타일의 상태를 파악한다.
            else
            {
                //벽이라면 이동할 수 없으므로 다음 타일을 체크한다.
                if(targetTile.IsWall)
                {
                    continue;
                }
                //만약 몬스터라면
                else if(targetTile.IsMonster)
                {
                    var monster = MonsterManager.Instance.GetMonsterByPos(targetPos);

                    //그 자리에 있는 몬스터가 이미 행동을 했는지 파악한다.
                    if(monster._IsAlreadyActioned)
                    {
                        continue;
                    }
                    else
                    {
                        //행동하지 않았다면 그 자리에 있는 몬스터가 먼저 행동하게 해준다.
                        monster.Action();

                        //그 자리에 있는 몬스터의 행동이 끝난 뒤에도 그 자리에 몬스터가 있으면
                        if(targetTile.IsMonster)
                        {
                            //그 타일로의 이동을 포기하고 다음 이동하고자 하는 타일로 다시 체크한다.
                            continue;
                        }
                        //그 자리에 몬스터가 사라졌으면 그 자리로 이동한다.
                        else
                        {
                            CurTile.SetState(TileState.GROUND, false);
                            //CurTile.State = TileState.GROUND;
                            targetTile.SetState(TileState.MONSTER, false);
                            //targetTile.State = TileState.MONSTER;
                            _Pos = targetPos;
                            StopCoroutine("Move_Internal");
                            StartCoroutine("Move_Internal", targetPos.vector3);
                            return;
                        }
                    }
                }
            }
        }
    }                       

    bool IsPlayerInMyArea()
    {
        var playerPos = Player.Instance.Pos;
        var areaMin = new Position(_Pos.x - _HalfAreaX, _Pos.y - _HalfAreaY);
        var areaMax = new Position(_Pos.x + _HalfAreaX, _Pos.y + _HalfAreaY);

        if(areaMin.x <= playerPos.x && playerPos.x <= areaMax.x)
        {
            if (areaMin.y <= playerPos.y && playerPos.y <= areaMax.y)
            {
                return true;
            }
        }

        return false;
    }

    public void Init(int index, Position pos)
    {
        _MonsterIndex = index;
        _HP = _MaxHP;

        InvalidateHPBar();

        _IsDead = false;

        _Pos = pos;
        CurTile.SetState(TileState.MONSTER, false);
        //CurTile.State = TileState.MONSTER;
        _CachedTransform.position = pos.vector;
    }

    public MonsterSaveData CreateSaveData()
    {
        return new MonsterSaveData(_MonsterIndex, _Pos, _MaxHP, _HP,
            _MinAttackPower, _MaxAttackPower, _CriticalProbability, _MinCriAttackPower, _MaxCriAttackPower,
            _HalfAreaX, _HalfAreaY, _CachedTransform.localScale.x, _Speed);
    }

    public void ApplySaveData(MonsterSaveData data)
    {
        _MonsterIndex = data.monsterIndex;
        _Pos = data.pos;
        _MaxHP = data.maxHP;
        _HP = data.hp;
        _MinAttackPower = data.minAttackPower;
        _MaxAttackPower = data.maxAttackPower;
        _MinCriAttackPower = data.minCriAttackPower;
        _MaxCriAttackPower = data.maxCriAttackPower;
        _CriticalProbability = data.criticalProbability;

        _HalfAreaX = data.halfAreaX;
        _HalfAreaY = data.halfAreaY;
        _CachedTransform.localScale = new Vector3(data.scaleX, 1, 1);
        _Speed = data.speed;
        _IsDead = false;

        InvalidateHPBar();

        _CachedTransform.position = _Pos.vector;

        CurTile.SetState(TileState.MONSTER, false);
    }

    public void SetColor(float rgb)
    {
        if(rgb > 0f)
        {
            var color = new Color(rgb, rgb, rgb, 1f);
            _SprRenderer.color = color;
            _HPBar.gameObject.SetActive(true);
        }
        else
        {
            var color = new Color(rgb, rgb, rgb, 0f);
            _SprRenderer.color = color;
            _HPBar.gameObject.SetActive(false);
        }
    }
}
