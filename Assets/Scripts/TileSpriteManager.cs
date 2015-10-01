using UnityEngine;
using System.Collections;

public class TileSpriteManager : MonoBehaviour 
{
    static TileSpriteManager _Instance = null;

    public static TileSpriteManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    public Sprite[] _GroundSprites;
    public Sprite[] _WallSprites;

	// Use this for initialization
    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetRandomGroundSprite()
    {
        return _GroundSprites[Random.Range(0, _GroundSprites.Length)];
    }

    public Sprite GetRandomWallSprite()
    {
        return _WallSprites[Random.Range(0, _WallSprites.Length)];
    }
}
