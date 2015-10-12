using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Size
{
    public int width, height;

    public Size(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}

[System.Serializable]
public struct Position
{
    public int x, y;

    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Position(Vector2 pos)
    {
        x = (int)pos.x;
        y = (int)pos.y;
    }

    public Vector2 vector
    {
        get
        {
            return new Vector2(x, y);
        }
    }

    public static bool operator ==(Position a, Position b)
    {
        return (a.x == b.x && a.y == b.y);
    }

    public static bool operator !=(Position a, Position b)
    {
        return (a.x != b.x || a.y != b.y);
    }

    public static Position operator +(Position a, Position b)
    {
        return new Position(a.x + b.x, a.y + b.y);
    }

    public static Position operator -(Position a, Position b)
    {
        return new Position(a.x - b.x, a.y - b.y);
    }

    public static Position left = new Position(-1, 0);
    public static Position right = new Position(1, 0);
    public static Position up = new Position(0, 1);
    public static Position down = new Position(0, -1);
}

public delegate void VoidCallback();
public abstract class MapGenerator : MonoBehaviour 
{
    public abstract void Generate(VoidCallback callback);
    //{
    //    return false;
    //}
}
