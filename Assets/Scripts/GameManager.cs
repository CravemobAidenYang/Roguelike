using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    public CastleMapGenerator _MapGeneratorPrefab;
    public TileManager _TileManagerPrefab;
    public Player _PlayerPrefab;

    static GameManager _Instance = null;

    public bool PlayerTurn { get; set; }

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
        //if (PlayerTurn)
        //{
        //    Player.Instance.TurnProcess();
        //}
        //else
        //{
        //    Monster.ProcessAllMonster();
        //}
    }
}
