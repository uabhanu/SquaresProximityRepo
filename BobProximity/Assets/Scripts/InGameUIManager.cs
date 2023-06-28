using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Event = Events.Event;

public class InGameUIManager : MonoBehaviour
{
    private GameManager _gameManager;
    private MainMenuManager _mainMenuManager;
    private PlayerController _playerController;
    
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
    
    public TMP_InputField[] PlayerNameTMPInputFields => playerNameTMPInputFields;

    public TMP_Text[] PlayerTotalWinsLabelsTMPTexts
    {
        get => playerTotalWinsLabelsTMPTexts;
        set => playerTotalWinsLabelsTMPTexts = value;
    }

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _mainMenuManager = FindObjectOfType<MainMenuManager>();
        _playerController = FindObjectOfType<PlayerController>();
        
        continueButtonObj.SetActive(false);
        gameOverPanelsObj.SetActive(false);
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

    private void OnGameOver()
    {
        continueButtonObj.SetActive(true);
        pauseButtonObj.SetActive(false);
        pauseMenuPanelObj.SetActive(false);
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
        
        int highestScorePlayer = _gameManager.GetHighestScorePlayer();

        _gameManager.PlayerTotalWinsArray[highestScorePlayer]++;
        //Debug.Log("Player " + highestScorePlayer + " total wins : " + _playerTotalWinsArray[highestScorePlayer]);
        PlayerTotalWinsLabelsTMPTexts[highestScorePlayer].text = "Total Wins : " + _gameManager.PlayerTotalWinsArray[highestScorePlayer];
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
        
        _gameManager.SaveData();
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
            _gameManager.PlayerNamesReceivedArray[i] = PlayerNameTMPInputFields[i].text;
            playerNameLabelTMPTexts[i].text = PlayerNameTMPInputFields[i].text;

            if(string.IsNullOrEmpty(_gameManager.PlayerNamesReceivedArray[i]))
            {
                allNamesFilled = false;
                break;
            }
        }

        if(allNamesFilled)
        {
            _gameManager.GameStarted = true;
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
        _gameManager.GameStarted = false;
        pauseButtonObj.SetActive(false);
        pauseMenuPanelObj.SetActive(true);
    }

    public void ResetButton()
    {
        PlayerPrefs.DeleteAll();
        _gameManager.LoadData();
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResumeButton()
    {
        _gameManager.GameStarted = true;
        pauseButtonObj.SetActive(true);
        pauseMenuPanelObj.SetActive(false);
    }
}
