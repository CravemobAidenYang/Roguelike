//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class MonsterManager : MonoBehaviour 
//{
//    public static MonsterManager Instance
//    {
//        get;
//        private set;
//    }

//    public Monster[] _MonsterPrefabArr;

//    List<Monster> _MonsterList = new List<Monster>();

//    // Use this for initialization
//    void Awake () 
//    {
//        if(Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(this.gameObject);
//        }
//    }

//    bool AllMonsterProcessIsDone()
//    {
//        foreach(var monster in _MonsterList)
//        {
//            if(monster.IsProcessing)
//            {
//                return false;
//            }
//        }
//        return true;
//    }

//    IEnumerator WaitForAllMonsterProcessDone()
//    {
//        while(true)
//        {
//            if(AllMonsterProcessIsDone())
//            {
//                GameManager.Instance.IsPlayerTurn = true;
//                break;
//            }
//            yield return null;
//        }
//    }

//    public void ProcessAllMonster()
//    {
//        if(!AllMonsterProcessIsDone())
//        {
//            return;
//        }

//        foreach (var monster in _MonsterList)
//        {
//            monster.PreProcess();
//        }
//        foreach (var monster in _MonsterList)
//        {
//            monster.Process();
//        }
//        foreach (var monster in _MonsterList)
//        {
//            monster.PostProcess();
//        }

//        StartCoroutine(WaitForAllMonsterProcessDone());
//    }

//    public Monster CreateMonster(int index)
//    {
//        var monster = Instantiate(_MonsterPrefabArr[index]);
//        _MonsterList.Add(monster);
//        return monster;
//    }

//    public void Cleanup()
//    {
//        for (int i = 0; i < _MonsterList.Count; ++i)
//        {
//            Destroy(_MonsterList[i].gameObject);
//        }
//        _MonsterList.Clear();
//    }
//}
