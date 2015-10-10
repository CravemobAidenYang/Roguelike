using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Monster : Unit
{
    public int _HalfAreaX, _HalfAreaY;

    bool _IsAlreadyActioned = false;

    public void InitAction()
    {
        _IsAlreadyActioned = false;
    }

    protected override void OnAwake()
    {
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

            //x방향으로의 길이가 더 멀다면 x방향으로의 이동명령을 큐에 먼저넣음.
            //TODO: (메소드 분할 필요)
            if (Mathf.Abs(playerPos.x - Pos.x) > Mathf.Abs(playerPos.y - Pos.y))
            {
                if (playerPos.x > Pos.x)
                {
                    moveDirQueue.Enqueue(right);
                }
                else if(playerPos.x < Pos.x)
                {
                    moveDirQueue.Enqueue(left);
                }

                if (playerPos.y > Pos.y)
                {
                    moveDirQueue.Enqueue(up);
                }
                else if (playerPos.y < Pos.y)
                {
                    moveDirQueue.Enqueue(down);
                }
            }
            else
            {
                if (playerPos.y > Pos.y)
                {
                    moveDirQueue.Enqueue(up);
                }
                else if (playerPos.y < Pos.y)
                {
                    moveDirQueue.Enqueue(down);
                }

                if (playerPos.x > Pos.x)
                {
                    moveDirQueue.Enqueue(right);
                }
                else if (playerPos.x < Pos.x)
                {
                    moveDirQueue.Enqueue(left);
                }
            }

            Move(moveDirQueue);
        }
    }

    IEnumerator Move_Internal(Vector3 targetPos)
    {
        print("monster move internal");
        while (true)
        {
            CachedTransform.position = Vector2.MoveTowards(CachedTransform.position, targetPos, _Speed * Time.deltaTime);

            if (CachedTransform.position == targetPos)
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
            var targetPos = new Position(Pos.x + moveDir.x, Pos.y + moveDir.y);
            var targetTile = TileManager.Instance.GetTile(targetPos);

            //이동하고자 하는 타일이 땅이라면 바로 이동한다.
            if (TileManager.Instance.IsGroundTile(targetPos))
            {
                CurTile.State = TileState.GROUND;
                targetTile.State = TileState.MONSTER;
                Pos = targetPos;
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
                            Pos = targetPos;
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
        var areaMin = new Position(Pos.x - _HalfAreaX, Pos.y - _HalfAreaY);
        var areaMax = new Position(Pos.x + _HalfAreaX, Pos.y + _HalfAreaY);

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
        Pos = pos;
        CurTile.State = TileState.MONSTER;
        CachedTransform.position = pos.vector;
    }

}
