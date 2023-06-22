using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameStarted;
    private GridManager _gridManager;
    private int _totalCells;
    private PlayerController _playerController;
    private ScoreManager _scoreManager;
    private string[] _playerNamesReceived;
    
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
        _gridManager = FindObjectOfType<GridManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _scoreManager = FindObjectOfType<ScoreManager>();
        
        gameOverPanelsObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        playerInputPanelObj.SetActive(true);

        _playerNamesReceived = new string[3];
        
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

        for(int i = 0; i < winsPanelObjs.Length; i++)
        {
            winsLabelsTMPTexts[i].text = PlayerNameTMPInputFields[i].text + " wins!!!";
        }
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
            _playerNamesReceived[i] = PlayerNameTMPInputFields[i].text;
            playerNameLabelTMPTexts[i].text = PlayerNameTMPInputFields[i].text;

            if(!string.IsNullOrEmpty(_playerNamesReceived[0]) && !string.IsNullOrEmpty(_playerNamesReceived[1]) && !string.IsNullOrEmpty(_playerNamesReceived[2]))
            {
                _gameStarted = true;
                playerInputPanelObj.SetActive(false);
                inGameUIPanelsObj.SetActive(true);
            }
        }
    }
}
