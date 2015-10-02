using UnityEngine;
using System.Collections;

public struct Cell 
{
    bool _Alive;

    Position _Pos;

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

    public Cell(Position pos, bool alive)
    {
        _Pos = pos;
        _Alive = alive;
    }

}
