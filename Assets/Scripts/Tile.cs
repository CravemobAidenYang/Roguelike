using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour 
{
    public Vector2 pos;

    SpriteRenderer _SprRenderer;
    BoxCollider2D _BoxCollider;
    Transform _CachedTransform;

    bool _IsWalkable;
    public bool IsWalkable
    {
        get
        {
            return _IsWalkable;
        }
        set
        {
            _IsWalkable = value;
        }
    }

	// Use this for initialization
	void Awake () 
    {
        _SprRenderer = GetComponent<SpriteRenderer>();
        _BoxCollider = GetComponent<BoxCollider2D>();
        _CachedTransform = transform;
	}
	
	public void Init(int x, int y, bool isWalkable)
    {
        _CachedTransform.position = new Vector2(x, y);
        pos = new Vector2(x, y);

        _IsWalkable = isWalkable;

        if(_IsWalkable)
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomGroundSprite();
        }
        else
        {
            _SprRenderer.sprite = TileSpriteManager.Instance.GetRandomWallSprite();
            _BoxCollider.enabled = true;
        }
    }
}
