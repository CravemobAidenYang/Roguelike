using UnityEngine;
using System.Collections;

public class DamageLabel : MonoBehaviour 
{
    Transform _CachedTransform;
    UILabel _Label;
    Vector3? targetWorldPos = null;

    public void DestroyDamageLabel()
    {
        Destroy(this.gameObject);
    }

    public string text
    {
        get
        {
            return _Label.text;
        }
        set
        {
            _Label.text = value;
        }
    }

    public void SetTargetWorldPos(Vector3 worldPos)
    {
        targetWorldPos = worldPos;
        var viewportPos = Camera.main.WorldToViewportPoint(targetWorldPos.Value);
        _CachedTransform.position = UICamera.currentCamera.ViewportToWorldPoint(viewportPos);

    }

    void Awake()
    {
        _Label = GetComponent<UILabel>();
        _CachedTransform = GetComponent<Transform>();
    }

    void Update()
    {
        if (targetWorldPos != null)
        {
            var viewportPos = Camera.main.WorldToViewportPoint(targetWorldPos.Value);
            _CachedTransform.position = UICamera.currentCamera.ViewportToWorldPoint(viewportPos);
        }
    }
}
