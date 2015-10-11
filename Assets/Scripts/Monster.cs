using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Monster : MonoBehaviour//Unit
{
    Transform _CachedTransform;
    Animator _CachedAnimator;

    public int _MaxHP;
    protected int _HP;

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

    public int _AttackPower;

    bool _IsAlreadyActioned = false;

    public void InitAction()
    {
        _IsAlreadyActioned = false;
    }

    void Awake()
    {
        _CachedTransform = this.transform;
        _CachedAnimator = GetComponent<Animator>();
    }

    public void Action()
    {
        _IsAlreadyActioned = true;


        if(IsPlayerInMyArea())
        {
            var playerPos = Player.Instance.Pos;
            var moveDirQueue = new Queue<Position>();

            var right = new Position(1, 0);
            var left = new Position(-1, 0);
            var up = new Position(0, 1);
            var down = new Position(0, -1);

            if(playerPos == _Pos + left || playerPos == _Pos + right ||
                playerPos == _Pos + up || playerPos == _Pos + down)
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
                    moveDirQueue.Enqueue(right);
                }
                else if(playerPos.x < _Pos.x)
                {
                    moveDirQueue.Enqueue(left);
                }

                if (playerPos.y > _Pos.y)
                {
                    moveDirQueue.Enqueue(up);
                }
                else if (playerPos.y < _Pos.y)
                {
                    moveDirQueue.Enqueue(down);
                }
            }
            else
            {
                if (playerPos.y > _Pos.y)
                {
                    moveDirQueue.Enqueue(up);
                }
                else if (playerPos.y < _Pos.y)
                {
                    moveDirQueue.Enqueue(down);
                }

                if (playerPos.x > _Pos.x)
                {
                    moveDirQueue.Enqueue(right);
                }
                else if (playerPos.x < _Pos.x)
                {
                    moveDirQueue.Enqueue(left);
                }
            }

            Move(moveDirQueue);
        }
    }

    void Attack()
    {
        if(!Player.Instance.IsDead)
        {
            _CachedAnimator.Play("MonsterAttack");
            Player.Instance.Hit(_AttackPower);
        }
    }

    public void Hit(int damage)
    {
        print("Monster.Hit");
        if (!_IsDead)
        {
            _HP -= damage;
            if (_HP <= 0)
            {
                _IsDead = true;
                Die();
            }
        }
    }

    void Die()
    {
        CurTile.State = TileState.GROUND;
        MonsterManager.Instance.RemoveMonsterFromList(this);
        this.gameObject.SetActive(false);
    }

    IEnumerator Move_Internal(Vector3 targetPos)
    {
        print("monster move internal");
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
                CurTile.State = TileState.GROUND;
                targetTile.State = TileState.MONSTER;
                _Pos = targetPos;
                StartCoroutine(Move_Internal(targetPos.vector));
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
                            CurTile.State = TileState.GROUND;
                            targetTile.State = TileState.MONSTER;
                            _Pos = targetPos;
                            StartCoroutine(Move_Internal(targetPos.vector));
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

    public void Init(Position pos)
    {
        _HP = _MaxHP;
        _IsDead = false;

        _Pos = pos;
        CurTile.State = TileState.MONSTER;
        _CachedTransform.position = pos.vector;
    }

}
