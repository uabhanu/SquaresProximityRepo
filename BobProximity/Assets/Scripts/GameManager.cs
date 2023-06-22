using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameStarted;
    private GridManager _gridManager;
    private const int TotalNumberOfPlayers = 3;
    private int[] _playerTotalWinsArray;
    private int _totalCells;
    private PlayerController _playerController;
    private ScoreManager _scoreManager;
    private string[] _playerNamesReceivedArray;
    
    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private GameObject inGameUIPanelsObj;
    [SerializeField] private GameObject leaderboardPanelObj;
    [SerializeField] private GameObject playerInputPanelObj;
    [SerializeField] private GameObject totalReceivedPanelObj;
    [SerializeField] private GameObject[] winsPanelObjs;
    [SerializeField] private TMP_InputField[] playerNameTMPInputFields;
    [SerializeField] private TMP_Text[] playerNameLabelTMPTexts;
    [SerializeField] private TMP_Text[] totalReceivedTMPTexts;
    [SerializeField] private TMP_Text[] playerTotalWinsLabelsTMPTexts;
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
        _gridManager = FindObjectOfType<GridManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _scoreManager = FindObjectOfType<ScoreManager>();
        
        gameOverPanelsObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        leaderboardPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(true);
        totalReceivedPanelObj.SetActive(false);

        _playerNamesReceivedArray = new string[TotalNumberOfPlayers];
        _playerTotalWinsArray = new int[TotalNumberOfPlayers];
        
        TotalCells = _gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows;

        for(int i = 0; i < winsPanelObjs.Length; i++)
        {
            winsPanelObjs[i].SetActive(false);
        }
        
        LoadData();
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
        Debug.Log("Player Who Won : " + _playerTotalWinsArray[highestScorePlayer]);
        playerTotalWinsLabelsTMPTexts[highestScorePlayer].text = "Total Wins : " + _playerTotalWinsArray[highestScorePlayer];
        winsLabelsTMPTexts[highestScorePlayer].text = PlayerNameTMPInputFields[highestScorePlayer].text + " Wins!!!!";
        SaveData();
    }

    private void LoadData()
    {
        for(int i = 0; i < PlayerNameTMPInputFields.Length; i++)
        {
            PlayerNameTMPInputFields[i].text = PlayerPrefs.GetString("Player " + i + " Name");
            _playerTotalWinsArray[i] = PlayerPrefs.GetInt("Player " + i + " Total Wins");
            playerTotalWinsLabelsTMPTexts[i].text = PlayerNameTMPInputFields[i].text + " Total Wins : " + _playerTotalWinsArray[i];
        }
    }

    private void SaveData()
    {
        for(int i = 0; i < PlayerNameTMPInputFields.Length; i++)
        {
            PlayerPrefs.SetString("Player " + i + " Name" , PlayerNameTMPInputFields[i].text);
        }

        for(int i = 0; i < _playerTotalWinsArray.Length; i++)
        {
            PlayerPrefs.SetInt("Player " + i + " Total Wins" , _playerTotalWinsArray[i]);
        }

        PlayerPrefs.Save();
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameOver , OnGameOver);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameOver , OnGameOver);
    }

    public void BackButton()
    {
        leaderboardPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(true);
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
                inGameUIPanelsObj.SetActive(true);
                playerInputPanelObj.SetActive(false);
            }
        }
    }

    public void OkButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LeaderboardButton()
    {
        LoadData();
        leaderboardPanelObj.SetActive(true);
        playerInputPanelObj.SetActive(false);
    }

    public void ResetButton()
    {
        PlayerPrefs.DeleteAll();

        for(int i = 0; i < PlayerNameTMPInputFields.Length; i++)
        {
            PlayerNameTMPInputFields[i].text = "";
        }
    }
}
