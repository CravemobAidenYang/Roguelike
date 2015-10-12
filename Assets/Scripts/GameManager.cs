using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    public CastleMapGenerator _MapGeneratorPrefab;
    public TileManager _TileManagerPrefab;
    public Player _PlayerPrefab;

    public UIPanel _PauseMenu;
    public Image _GameOverMenu;

    bool _IsStarted;

    bool _IsPlayerTurn = false;
    public bool IsPlayerTurn
    {
        get
        {
            return _IsPlayerTurn;
        }
        set
        {
            _IsPlayerTurn = value;
        }
    }

    bool _IsPause;
    public bool IsPause
    {
        get
        {
            //게임이 시작하지 않았거나 멈춤상태면
            return (!_IsStarted || _IsPause);
        }
    }

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
            _IsPause = false;
            _IsStarted = false;
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
            _IsStarted = true;
        });
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(TogglePauseMenuCoroutine());
        }
    }

    IEnumerator TogglePauseMenuCoroutine()
    {
        if(Player.Instance == null || MonsterManager.Instance == null)
        {
            yield break;
        }

        //행동이 끝날때 까지 대기한 후 일시정지시킨다.
        while(Player.Instance.IsProcessing || MonsterManager.Instance.IsProcessing)
        {
            yield return null;
        }

        if (_IsStarted)
        {
            _IsPause = !_IsPause;

            if (_IsPause)
            {
                _PauseMenu.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                _PauseMenu.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }

    public void Save()
    {
        print("Save Button");
        TileManager.Instance.SaveTileMap();
        Player.Instance.SavePlayerData();
        MonsterManager.Instance.SaveMonsterData();

        _IsPause = false;
        _PauseMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Load()
    {
        print("Load Button");
        TileManager.Instance.LoadTileMap();
        Player.Instance.LoadPlayerData();
        MonsterManager.Instance.LoadMonsterData();
     
        _IsPause = false;
        _PauseMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Exit()
    {
        print("Exit Button");
        Application.Quit();
    }

    public void GameOver()
    {
        print("GameManager.GameOver");
        _GameOverMenu.gameObject.SetActive(true);
    }

    public void Restart()
    {
        _GameOverMenu.gameObject.SetActive(false);
        CastleMapGenerator.Instance.Reset();
    }
}
