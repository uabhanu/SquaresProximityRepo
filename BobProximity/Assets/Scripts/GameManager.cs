using Data;
using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameStarted;
    private GridManager _gridManager;
    private int[] _playerTotalWinsArray;
    private int _totalCells;
    private const int TotalNumberOfPlayers = 3;
    private PlayerController _playerController;
    private ScoreManager _scoreManager;
    private readonly string _filePath = "D:\\player_wins.json";
    private string[] _playerNamesReceivedArray;
    
    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private GameObject inGameUIPanelsObj;
    [SerializeField] private GameObject playerInputPanelObj;
    [SerializeField] private GameObject[] winsPanelObjs;
    [SerializeField] private TMP_InputField[] playerNameTMPInputFields;
    [SerializeField] private TMP_Text[] playerNameLabelTMPTexts;
    [SerializeField] private TMP_Text[] totalReceivedTMPTexts;
    [SerializeField] private TMP_Text[] winsLabelsTMPTexts;

    public bool GameStarted
    {
        get => _gameStarted;
        set => _gameStarted = value;
    }
    
    public int TotalCells
    {
        get => _totalCells;
        set => _totalCells = value;
    }

    public TMP_InputField[] PlayerNameTMPInputFields => playerNameTMPInputFields;

    private void Start()
    {
        LoadData();
        _gridManager = FindObjectOfType<GridManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _scoreManager = FindObjectOfType<ScoreManager>();
        
        gameOverPanelsObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        playerInputPanelObj.SetActive(true);

        _playerNamesReceivedArray = new string[TotalNumberOfPlayers];
        _playerTotalWinsArray = new int[TotalNumberOfPlayers];
        
        TotalCells = _gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows;

        for(int i = 0; i < winsPanelObjs.Length; i++)
        {
            winsPanelObjs[i].SetActive(false);
        }
        
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private int GetHighestScorePlayer()
    {
        int highestScore = int.MinValue;
        int highestScorePlayer = -1;

        for(int i = 0; i < _scoreManager.CoinScoreValues.Length; i++)
        {
            if(_scoreManager.CoinScoreValues[i] > highestScore)
            {
                highestScore = _scoreManager.CoinScoreValues[i];
                highestScorePlayer = i;
            }
        }

        return highestScorePlayer;
    }

    private void OnGameOver()
    {
        GameStarted = false;
        int highestScorePlayer = GetHighestScorePlayer();
        string playerName = "Player " + highestScorePlayer;
        Debug.Log(playerName + " wins with a score of " + _scoreManager.CoinScoreValues[highestScorePlayer]);
        gameOverPanelsObj.SetActive(true);
        
        for(int i = 0; i < totalReceivedTMPTexts.Length; i++)
        {
            totalReceivedTMPTexts[i].text = PlayerNameTMPInputFields[i].text + " received : " + _playerController.TotalReceivedArray[i];
        }
        
        winsPanelObjs[highestScorePlayer].SetActive(true);
        _playerTotalWinsArray[highestScorePlayer]++;
        SaveData();


        for(int i = 0; i < winsPanelObjs.Length; i++)
        {
            winsLabelsTMPTexts[i].text = PlayerNameTMPInputFields[i].text + " wins!!!";
        }
    }
    
    public void LoadData()
    {
        GameDataWrapper gameData = JsonDataManager.LoadData<GameDataWrapper>(_filePath);
        
        for(int i = 0; i < gameData.PlayerNames.Length; i++)
        {
            playerNameTMPInputFields[i].text = gameData.PlayerNames[i];
        }
    }

    private void SaveData()
    {
        GameDataWrapper gameData = new GameDataWrapper
        {
            PlayerNames = _playerNamesReceivedArray,
            PlayerWins = _playerTotalWinsArray
        };

        JsonDataManager.SaveData(gameData , _filePath);
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameOver , OnGameOver);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameOver , OnGameOver);
    }

    public void OkButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void EnterButton()
    {
        for(int i = 0; i < PlayerNameTMPInputFields.Length; i++)
        {
            _playerNamesReceivedArray[i] = PlayerNameTMPInputFields[i].text;
            playerNameLabelTMPTexts[i].text = PlayerNameTMPInputFields[i].text;

            if(!string.IsNullOrEmpty(_playerNamesReceivedArray[0]) && !string.IsNullOrEmpty(_playerNamesReceivedArray[1]) && !string.IsNullOrEmpty(_playerNamesReceivedArray[2]))
            {
                _gameStarted = true;
                playerInputPanelObj.SetActive(false);
                inGameUIPanelsObj.SetActive(true);
            }
        }
    }

    public class GameDataWrapper
    {
        public int[] PlayerWins { get; set; }
        public string[] PlayerNames { get; set; }
    }
}
