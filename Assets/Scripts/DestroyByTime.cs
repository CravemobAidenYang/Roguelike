using UnityEngine;
using System.Collections;

public class DestroyByTime : MonoBehaviour 
{
    public float _LifeTime;
    float _StartTime;

	void Start () 
    {
        _StartTime = Time.time;
	}
	
	void Update () 
    {
        if (Time.time - _StartTime >= _LifeTime)
        {
            Destroy(gameObject);
        }
	}
}
