using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Monster : Unit
{
    static List<Monster> _MonsterList = new List<Monster>();
	
    public static void ProcessAllMonster()
    {
        foreach(var monster in _MonsterList)
        {
            monster.PreProcess();
        }
        foreach (var monster in _MonsterList)
        {
            monster.Process();
        }
        foreach (var monster in _MonsterList)
        {
            monster.PostProcess();
        }

    }

    void PreProcess()
    {

    }

    void Process()
    {

    }

    void PostProcess()
    {

    }

}
