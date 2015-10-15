using UnityEngine;
using System.Collections;

public class GameOverMenu : MonoBehaviour 
{
    public UILabel _ScoreLabel;

    public void SetScoreText(int killCount, int traveledDungeonCount)
    {
        _ScoreLabel.text = "당신은 " + traveledDungeonCount + "개의 던전을 탐험했고,\n" + killCount + "마리 의 몬스터를 사냥하였습니다.";
    }
}
