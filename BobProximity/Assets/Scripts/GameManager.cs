using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private bool _gameStarted;
    private GridManager _gridManager;
    private int[] _playerTotalWinsArray;
    private int _totalCells;
    private MainMenuManager _mainMenuManager;
    private PlayerController _playerController;
    private ScoreManager _scoreManager;
    private string[] _playerNamesReceivedArray;
    
    [SerializeField] private GameObject continueButtonObj;
    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private GameObject inGameUIPanelsObj;
    [SerializeField] private GameObject inGameUIPlayerNamesDisplayPanelObj;
    [SerializeField] private GameObject leaderboardPanelObj;
    [SerializeField] private GameObject pauseButtonObj;
    [SerializeField] private GameObject pauseMenuPanelObj;
    [SerializeField] private GameObject playerInputPanelObj;
    [SerializeField] private GameObject totalReceivedPanelObj;
    [SerializeField] private GameObject totalWinsPanelObj;
    [SerializeField] private GameObject[] inGameUIPlayerNamesDisplayPanelObjs;
    [SerializeField] private GameObject[] leaderboardWinsPanelObjs;
    [SerializeField] private GameObject[] totalReceivedPanelObjs;
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
        _mainMenuManager = FindObjectOfType<MainMenuManager>();
        _gridManager = FindObjectOfType<GridManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _scoreManager = FindObjectOfType<ScoreManager>();
        
        continueButtonObj.SetActive(false);
        gameOverPanelsObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        leaderboardPanelObj.SetActive(false);
        pauseMenuPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(true);

        _playerNamesReceivedArray = new string[_mainMenuManager.TotalNumberOfPlayers];
        _playerTotalWinsArray = new int[_mainMenuManager.TotalNumberOfPlayers];

        if(_mainMenuManager.TotalNumberOfPlayers == 2)
        {
            PlayerNameTMPInputFields[_mainMenuManager.TotalNumberOfPlayers].gameObject.SetActive(false);
        }
        
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
        continueButtonObj.SetActive(true);
        pauseMenuPanelObj.SetActive(false);
        GameStarted = false;
    }

    private void LoadData()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            PlayerNameTMPInputFields[i].text = PlayerPrefs.GetString("Player " + i + " Name");
            _playerTotalWinsArray[i] = PlayerPrefs.GetInt("Player " + i + " Total Wins");
            playerTotalWinsLabelsTMPTexts[i].text = PlayerNameTMPInputFields[i].text + " Total Wins : " + _playerTotalWinsArray[i];
        }
    }

    private void SaveData()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            PlayerPrefs.SetString("Player " + i + " Name" , PlayerNameTMPInputFields[i].text);
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

    public void ContinueButton()
    {
        continueButtonObj.SetActive(false);
        
        int highestScorePlayer = GetHighestScorePlayer();

        _playerTotalWinsArray[highestScorePlayer]++;
        //Debug.Log("Player " + highestScorePlayer + " total wins : " + _playerTotalWinsArray[highestScorePlayer]);
        playerTotalWinsLabelsTMPTexts[highestScorePlayer].text = "Total Wins : " + _playerTotalWinsArray[highestScorePlayer];
        winsLabelsTMPTexts[highestScorePlayer].text = PlayerNameTMPInputFields[highestScorePlayer].text + " Wins!!!!";
        
        gameOverPanelsObj.SetActive(true);
        winsPanelObjs[highestScorePlayer].SetActive(true);

        if(_mainMenuManager.TotalNumberOfPlayers == 2)
        {
            totalReceivedPanelObjs[_mainMenuManager.TotalNumberOfPlayers].SetActive(false);
            totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;
            
            winsPanelObjs[_mainMenuManager.TotalNumberOfPlayers].SetActive(false);
        }

        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            totalReceivedTMPTexts[i].text = PlayerNameTMPInputFields[i].text + " received : " + _playerController.TotalReceivedArray[i];
        }
        
        SaveData();
    }
    
    public void EnterButton()
    {
        bool allNamesFilled = true;

        if(_mainMenuManager.TotalNumberOfPlayers == 2)
        {
            inGameUIPlayerNamesDisplayPanelObjs[_mainMenuManager.TotalNumberOfPlayers].SetActive(false);
            inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 545;
        }
        
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            _playerNamesReceivedArray[i] = PlayerNameTMPInputFields[i].text;
            playerNameLabelTMPTexts[i].text = PlayerNameTMPInputFields[i].text;

            if(string.IsNullOrEmpty(_playerNamesReceivedArray[i]))
            {
                allNamesFilled = false;
                break;
            }
        }

        if(allNamesFilled)
        {
            _gameStarted = true;
            inGameUIPanelsObj.SetActive(true);
            playerInputPanelObj.SetActive(false);
        }
    }
    
    public void LeaderboardButton()
    {
        leaderboardPanelObj.SetActive(true);
        
        if(_mainMenuManager.TotalNumberOfPlayers == 2)
        {
            leaderboardWinsPanelObjs[_mainMenuManager.TotalNumberOfPlayers].SetActive(false);
            totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;
        }
        
        playerInputPanelObj.SetActive(false);
    }

    public void OkButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseButton()
    {
        GameStarted = false;
        pauseButtonObj.SetActive(false);
        pauseMenuPanelObj.SetActive(true);
    }

    public void QuitButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void ResetButton()
    {
        PlayerPrefs.DeleteAll();
        LoadData();
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResumeButton()
    {
        GameStarted = true;
        pauseButtonObj.SetActive(true);
        pauseMenuPanelObj.SetActive(false);
    }
}
