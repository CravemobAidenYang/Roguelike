using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class MonsterListSaveData
{
    public int count;
    public MonsterSaveData[] monsterSaveDatas;

    public MonsterListSaveData(int count, MonsterSaveData[] monsterSaveDatas)
    {
        this.count = count;
        this.monsterSaveDatas = monsterSaveDatas;
    }
}

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance
    {
        get;
        private set;
    }

    public Monster[] _MonsterPrefabArr;

    List<Monster> _MonsterList = new List<Monster>();

    bool _IsProcessing = false;
    public bool IsProcessing
    {
        get
        {
            return _IsProcessing;
        }
    }

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
        print("Process All Monster");

        if(GameManager.Instance.IsPause)
        {
            return;
        }

        _IsProcessing = true;

        foreach (var monster in _MonsterList)
        {
            monster.InitAction();
        }
        foreach(var monster in _MonsterList)
        {
            monster.Action();
        }

        _IsProcessing = false;
    }

    public void Cleanup()
    {
        foreach (var monster in _MonsterList)
        {
            monster.Cleanup();
            Destroy(monster.gameObject);
        }
        _MonsterList.Clear();
    }

    public void SaveMonsterData()
    {
        var monsterDataArr = new MonsterSaveData[_MonsterList.Count];

        for(int i = 0; i < _MonsterList.Count; ++i)
        {
            monsterDataArr[i] = _MonsterList[i].CreateSaveData();
        }

        var monsterListData = new MonsterListSaveData(_MonsterList.Count, monsterDataArr);

        SaveLoad.SaveData("MonsterListSaveData", monsterListData);
    }

    public void LoadMonsterData()
    {
        Cleanup();

        var data = SaveLoad.LoadData<MonsterListSaveData>("MonsterListSaveData", null);

        for (int i = 0; i < data.count; ++i)
        {
            var monster = Instantiate(_MonsterPrefabArr[0]);
            monster.ApplySaveData(data.monsterSaveDatas[i]);
            _MonsterList.Add(monster);
        }
    }
}
