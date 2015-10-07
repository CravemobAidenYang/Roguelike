using UnityEngine;
using System.Collections;

public struct Size
{
    public int width, height;

    public Size(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}

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
}

public delegate void VoidCallback();
public abstract class MapGenerator : MonoBehaviour 
{
    public abstract void Generate(VoidCallback callback);
    //{
    //    return false;
    //}
}
