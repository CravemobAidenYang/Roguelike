using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    public CastleMapGenerator _MapGeneratorPrefab;
    public TileManager _TileManagerPrefab;
    public Player _PlayerPrefab;

    public bool IsPlayerTurn { get; set; }

    static GameManager _Instance = null;

    public static GameManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        var tileManager = Instantiate(_TileManagerPrefab);
        var mapGenerator = Instantiate(_MapGeneratorPrefab);
        mapGenerator.Generate(() =>
        {
            var player = Instantiate(_PlayerPrefab);
        });
    }

    void Update()
    {
        //if(Player.Instance != null)
        //{
        //    if (IsPlayerTurn)
        //    {
        //        Player.Instance.TurnProcess();
        //    }
        //    else
        //    {
        //        MonsterManager.Instance.ProcessAllMonster();
        //    }
        //}
    }
}
