using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance
    {
        get;
        private set;
    }

    public Monster[] _MonsterPrefabArr;

    List<Monster> _MonsterList = new List<Monster>();

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public Monster CreateMonster(int monsterIndex)
    {
        var monster = Instantiate(_MonsterPrefabArr[monsterIndex]);
        _MonsterList.Add(monster);
        return monster;
    }

    public void RemoveMonsterFromList(Monster monster)
    {
        _MonsterList.Remove(monster);
    }

    public Monster GetMonsterByPos(Position pos)
    {
        foreach(var monster in _MonsterList)
        {
            if(monster.Pos == pos)
            {
                return monster;
            }
        }

        return null;
    }

    public void ProcessAllMonster()
    {
        foreach (var monster in _MonsterList)
        {
            monster.InitAction();
        }
        foreach(var monster in _MonsterList)
        {
            monster.Action();
        }
    }

    public void Cleanup()
    {
        foreach (var monster in _MonsterList)
        {
            Destroy(monster.gameObject);
        }
        _MonsterList.Clear();
    }
}
