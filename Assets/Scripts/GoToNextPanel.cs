using UnityEngine;
using System.Collections;

public class GoToNextPanel : MonoBehaviour 
{

	public void GoToNextDungeon()
    {
        GameManager.Instance.GoToNextDungeon();
    }
}
