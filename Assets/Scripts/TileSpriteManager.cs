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
    public Sprite[] _FoodSprites;
    public Sprite _ExitSprite;

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

    public Sprite GetRandomGroundSprite(out int index)
    {
        index = Random.Range(0, _GroundSprites.Length);
        return _GroundSprites[index];
    }

    public Sprite GetRandomWallSprite(out int index)
    {
        index = Random.Range(0, _WallSprites.Length);
        return _WallSprites[index];
    }

    public Sprite GetGroundSpriteFromIndex(int index)
    {
        return _GroundSprites[index];
    }

    public Sprite GetWallSpriteFromIndex(int index)
    {
        return _WallSprites[index];
    }

}
