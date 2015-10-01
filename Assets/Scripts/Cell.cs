using UnityEngine;
using System.Collections;

public struct Cell 
{
    bool _Alive;

    public bool Alive
    {
        get
        {
            return _Alive;
        }
        set
        {
            _Alive = value;
        }
    }

    public Cell(bool alive)
    {
        _Alive = alive;
    }

}
