using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    private int _numberOfPlayers;
    private int[] _totalReceivedArray;
    private string[] _playerNamesArray;

    [SerializeField] private GameObject continueButtonObj;
    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private GameObject gameTiedPanelObj;
    [SerializeField] private GameObject inGameUIPanelsObj;
    [SerializeField] private GameObject inGameUIPlayerNamesDisplayPanelObj;
    [SerializeField] private GameObject leaderboardPanelObj;
    [SerializeField] private GameObject numberOfPlayersSelectionPanelObj;
    [SerializeField] private GameObject playerInputPanelObj;
    [SerializeField] private GameObject totalReceivedPanelObj;
    [SerializeField] private GameObject totalWinsPanelObj;
    [SerializeField] private GameObject[] inGameUIPlayerNamesDisplayPanelObjs;
    [SerializeField] private GameObject[] leaderboardWinsPanelObjs;
    [SerializeField] private GameObject[] totalReceivedPanelObjs;
    [SerializeField] private GameObject[] winsPanelObjs;
    [SerializeField] private Slider numberOfPlayersSelectionSlider;
    [SerializeField] private TMP_InputField[] playerNameTMPInputFields;
    [SerializeField] private TMP_Text[] totalReceivedTMPTexts;
    [SerializeField] private TMP_Text[] playerTotalWinsLabelsTMPTexts;
    [SerializeField] private TMP_Text[] playerWinsLabelsTMPTexts;

    private void Start()
    {
        continueButtonObj.SetActive(false);
        gameOverPanelsObj.SetActive(false);
        gameTiedPanelObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        leaderboardPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(false);

        _playerNamesArray = new string[_numberOfPlayers];
        _totalReceivedArray = new int[_numberOfPlayers];

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

    private void LoadData()
    {
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            _playerNamesArray[i] = PlayerPrefs.GetString("Player " + i + " Name");
            playerNameTMPInputFields[i].text = _playerNamesArray[i];
        }
    }

    private void SaveData()
    {
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            PlayerPrefs.SetString("Player " + i + " Name" , _playerNamesArray[i]);
        }

        PlayerPrefs.Save();
    }

    private void UpdateInGamePlayerNames(int playerID)
    {
        string playerName = playerNameTMPInputFields[playerID].text;
        EventsManager.Invoke(Event.PlayerNamesUpdated , playerID , playerName);
    }

    public void BackButton()
    {
        leaderboardPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(true);
    }
    
    public void ConfirmButton()
    {
        if(numberOfPlayersSelectionSlider.value == 0 || numberOfPlayersSelectionSlider.value == 1)
        {
            numberOfPlayersSelectionPanelObj.SetActive(false);
            playerInputPanelObj.SetActive(true);
        }
    }

    public void ContinueButton()
    {
        continueButtonObj.SetActive(false);
        gameOverPanelsObj.SetActive(true);
        
        if(_numberOfPlayers == 2)
        {
            totalReceivedPanelObjs[_numberOfPlayers].SetActive(false);
            totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;

            winsPanelObjs[_numberOfPlayers].SetActive(false);
        }

        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerNameTMPInputFields[i].text = _playerNamesArray[i];
            totalReceivedTMPTexts[i].text = playerNameTMPInputFields[i].text + " received : " + _totalReceivedArray[i];
        }
    }

    public void EnterButton()
    {
        bool allNamesFilled = true;
        
        if(_numberOfPlayers == 2)
        {
            inGameUIPlayerNamesDisplayPanelObjs[_numberOfPlayers].SetActive(false);
            inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 545;
        }

        for(int i = 0; i < _numberOfPlayers; i++)
        {
            _playerNamesArray[i] = playerNameTMPInputFields[i].text;

            UpdateInGamePlayerNames(i);

            if(string.IsNullOrEmpty(playerNameTMPInputFields[i].text))
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
        
        if(_numberOfPlayers == 2)
        {
            leaderboardWinsPanelObjs[_numberOfPlayers].SetActive(false);
            totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;
        }

        playerInputPanelObj.SetActive(false);
    }

    public void OkButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetButton()
    {
        EventsManager.Invoke(Event.GameDataReset);
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerNameTMPInputFields[i].text = "";
        }
    }
    
    public void SetPlayersNumber()
    {
        if(numberOfPlayersSelectionSlider.value == 0)
        {
            _numberOfPlayers = 2;
            _playerNamesArray = new string[_numberOfPlayers];
            playerNameTMPInputFields[_numberOfPlayers].gameObject.SetActive(false);
            EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
        }
        
        else if(numberOfPlayersSelectionSlider.value == 1)
        {
            _numberOfPlayers = 3;
            _playerNamesArray = new string[_numberOfPlayers];
            EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
        }
    }

    private void OnGameDataLoaded(int[] totalWinsArray)
    {
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerTotalWinsLabelsTMPTexts[i].text = _playerNamesArray[i] + " Total Wins : " + totalWinsArray[i];
        }
    }

    private void OnGameOver()
    {
        SaveData();
        continueButtonObj.SetActive(true);
    }

    private void OnGameTied()
    {
        gameTiedPanelObj.SetActive(true);
    }

    private void OnPlayerWins(int highestScorePlayer)
    {
        playerTotalWinsLabelsTMPTexts[highestScorePlayer].text = playerNameTMPInputFields[highestScorePlayer].text + " Wins!!!";
        playerWinsLabelsTMPTexts[highestScorePlayer].text = playerTotalWinsLabelsTMPTexts[highestScorePlayer].text;
        winsPanelObjs[highestScorePlayer].SetActive(true);
    }

    private void OnTotalReceived(int[] totalReceivedArray)
    {
        _totalReceivedArray = totalReceivedArray;
    }
    
    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameDataLoaded , OnGameDataLoaded);
        EventsManager.SubscribeToEvent(Event.GameOver , OnGameOver);
        EventsManager.SubscribeToEvent(Event.GameTied , OnGameTied);
        EventsManager.SubscribeToEvent(Event.PlayerWins , OnPlayerWins);
        EventsManager.SubscribeToEvent(Event.PlayerTotalReceived , OnTotalReceived);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameDataLoaded , OnGameDataLoaded);
        EventsManager.UnsubscribeFromEvent(Event.GameOver , OnGameOver);
        EventsManager.UnsubscribeFromEvent(Event.GameTied , OnGameTied);
        EventsManager.UnsubscribeFromEvent(Event.PlayerWins , OnPlayerWins);
        EventsManager.UnsubscribeFromEvent(Event.PlayerTotalReceived , OnTotalReceived);
    }
}