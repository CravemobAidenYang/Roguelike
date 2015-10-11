//using UnityEngine;
//using System.Collections;

//public abstract class Unit : MonoBehaviour
//{
//    public int _MaxHP;
//    protected int _HP;

//    public float _Speed;

//    bool _IsDead;
//    protected bool IsDead
//    {
//        get
//        {
//            return _IsDead;
//        }
//        set
//        {
//            _IsDead = value;
//        }
//    }

//    Transform _CachedTransform;
//    protected Transform CachedTransform
//    {
//        get
//        {
//            return _CachedTransform;
//        }
//    }

//    private Position _Pos;
//    public Position Pos
//    {
//        get
//        {
//            return _Pos;
//        }
//        protected set
//        {
//            _Pos = value;
//        }
//    }

//    //현재 Pos에 위치한 타일을 얻어옴
//    public Tile CurTile
//    {
//        get
//        {
//            return TileManager.Instance.GetTile(Pos);
//        }
//    }

//    void Awake()
//    {
//        print("Unit.Awake");
//        _CachedTransform = this.transform;

//        OnAwake();
//    }

//    protected abstract void OnAwake();


//    public void Init()
//    {
//        _HP = _MaxHP;
//        _IsDead = false;

//        OnInit();
//    }

//    protected abstract void OnInit();
//}
