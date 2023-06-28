using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    private bool _isGameTied;
    
    private MainMenuManager _mainMenuManager;
    private PlayerController _playerController;

    [SerializeField] private GameObject continueButtonObj;
    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private GameObject gameTiedPanelObj;
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
    [SerializeField] private TMP_Text[] totalReceivedTMPTexts;
    [SerializeField] private TMP_Text[] playerTotalWinsLabelsTMPTexts;
    [SerializeField] private TMP_Text[] playerWinsLabelsTMPTexts;

    public TMP_InputField[] PlayerNameTMPInputFields => playerNameTMPInputFields;

    public TMP_Text[] PlayerTotalWinsLabelsTMPTexts => playerTotalWinsLabelsTMPTexts;

    private void Start()
    {
        _mainMenuManager = FindObjectOfType<MainMenuManager>();
        _playerController = FindObjectOfType<PlayerController>();

        continueButtonObj.SetActive(false);
        gameOverPanelsObj.SetActive(false);
        gameTiedPanelObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        leaderboardPanelObj.SetActive(false);
        pauseMenuPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(true);

        if(_mainMenuManager.TotalNumberOfPlayers == 2)
        {
            PlayerNameTMPInputFields[_mainMenuManager.TotalNumberOfPlayers].gameObject.SetActive(false);
        }

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

        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            if(_playerController.TotalReceivedArray[i] > highestScore)
            {
                highestScore = _playerController.TotalReceivedArray[i];
                highestScorePlayer = i;
            }
        }

        return highestScorePlayer;
    }

    private void UpdatePlayerName(int playerID)
    {
        string playerName = PlayerNameTMPInputFields[playerID].text;
        EventsManager.Invoke(Event.PlayerNamesUpdated , playerID , playerName);
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

        EventsManager.Invoke(Event.PlayerWins , highestScorePlayer);

        gameOverPanelsObj.SetActive(true);

        if(!_isGameTied)
        {
            playerTotalWinsLabelsTMPTexts[highestScorePlayer].text = PlayerNameTMPInputFields[highestScorePlayer].text + " Wins!!!";
            playerWinsLabelsTMPTexts[highestScorePlayer].text = playerTotalWinsLabelsTMPTexts[highestScorePlayer].text;
            winsPanelObjs[highestScorePlayer].SetActive(true);
        }
        else
        {
            gameTiedPanelObj.SetActive(true);
        }

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
            UpdatePlayerName(i);

            if(string.IsNullOrEmpty(PlayerNameTMPInputFields[i].text))
            {
                allNamesFilled = false;
                break;
            }
        }

        if(allNamesFilled)
        {
            EventsManager.Invoke(Event.GameStarted);
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
        EventsManager.Invoke(Event.GamePaused);
        pauseButtonObj.SetActive(false);
        pauseMenuPanelObj.SetActive(true);
    }

    public void ResetButton()
    {
        EventsManager.Invoke(Event.GameDataReset);
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResumeButton()
    {
        EventsManager.Invoke(Event.GameResumed);
        pauseButtonObj.SetActive(true);
        pauseMenuPanelObj.SetActive(false);
    }

    private void OnGameOver()
    {
        continueButtonObj.SetActive(true);
        pauseButtonObj.SetActive(false);
        pauseMenuPanelObj.SetActive(false);
    }

    private void OnGameTied()
    {
        _isGameTied = true;
    }
    
    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameOver , OnGameOver);
        EventsManager.SubscribeToEvent(Event.GameTied , OnGameTied);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameOver , OnGameOver);
        EventsManager.UnsubscribeFromEvent(Event.GameTied , OnGameTied);
    }
}