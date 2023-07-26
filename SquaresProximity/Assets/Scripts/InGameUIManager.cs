using Data;
using Event = Events.Event;
using Events;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    private int _highestScorePlayerID;
    private int _numberOfPlayers;
    private int[] _playersTotalWinsArray;
    private int[] _totalReceivedArray;
    private string[] _playerNamesArray;

    [SerializeField] private GameObject coinUIObj;
    [SerializeField] private GameObject continueButtonObj;
    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private GameObject gameTiedPanelObj;
    [SerializeField] private GameObject inGameUIPanelsObj;
    [SerializeField] private GameObject inGameUIPlayerNamesDisplayPanelObj;
    [SerializeField] private GameObject leaderboardPanelObj;
    [SerializeField] private GameObject pauseButtonObj;
    [SerializeField] private GameObject pauseMenuPanelObj;
    [SerializeField] private GameObject numberOfPlayersSelectionPanelObj;
    [SerializeField] private GameObject playerInputPanelObj;
    [SerializeField] private GameObject totalReceivedPanelObj;
    [SerializeField] private GameObject totalWinsPanelObj;
    [SerializeField] private GameObject[] inGameUIPlayerNamesDisplayPanelObjs;
    [SerializeField] private GameObject[] leaderboardWinsPanelObjs;
    [SerializeField] private GameObject[] totalReceivedPanelObjs;
    [SerializeField] private GameObject[] winsPanelObjs;
    [SerializeField] private TMP_InputField[] playerNameTMPInputFields;
    [SerializeField] private TMP_Text backButtonTMPText;
    [SerializeField] private TMP_Text[] totalReceivedTMPTexts;
    [SerializeField] private TMP_Text[] playerTotalWinsLabelsTMPTexts;
    [SerializeField] private TMP_Text[] playerWinsLabelsTMPTexts;
    [SerializeField] private TMP_Text[] coinScoreTMPTexts;
    [SerializeField] private Toggle[] numberOfPlayersSelectionTogglesArray;
    [SerializeField] private Toggle[] aiHumanTogglesArray;
    
    private void Start()
    {
        continueButtonObj.SetActive(false);
        gameOverPanelsObj.SetActive(false);
        gameTiedPanelObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        leaderboardPanelObj.SetActive(false);
        pauseMenuPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(false);

        _playersTotalWinsArray = new int[_numberOfPlayers];
        _totalReceivedArray = new int[_numberOfPlayers];

        for(int i = 0; i < numberOfPlayersSelectionTogglesArray.Length; i++)
        {
            numberOfPlayersSelectionTogglesArray[i].isOn = false;
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

    private void UpdateInGamePlayerNames(int playerID)
    {
        string playerName = playerNameTMPInputFields[playerID].text;
        EventsManager.Invoke(Event.PlayerNamesUpdated , playerID , playerName);
    }

    public void AIHumanToggle()
    {
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            bool isAI = aiHumanTogglesArray[i].isOn;
            EventsManager.Invoke(Event.AIHumanToggled , i , isAI);
        }
    }

    public void BackButton()
    {
        if(backButtonTMPText.text == "Back")
        {
            leaderboardPanelObj.SetActive(false);
            playerInputPanelObj.SetActive(true);   
        }
        else
        {
            EventsManager.Invoke(Event.GameResumed);
            leaderboardPanelObj.SetActive(false);
            inGameUIPanelsObj.SetActive(true);
            pauseButtonObj.SetActive(true);
            pauseMenuPanelObj.SetActive(false);
        }
    }
    
    public void BackButtonMain()
    {
        numberOfPlayersSelectionPanelObj.SetActive(true);
        playerInputPanelObj.SetActive(false);
    }

    public void ConfirmButton()
    {
        bool isAnyToggleOn = numberOfPlayersSelectionTogglesArray[0].isOn || numberOfPlayersSelectionTogglesArray[1].isOn;

        if(!isAnyToggleOn)
        {
            _numberOfPlayers = 2;
            numberOfPlayersSelectionTogglesArray[0].isOn = true;
        }
        
        else if(numberOfPlayersSelectionTogglesArray[0].isOn && numberOfPlayersSelectionTogglesArray[1].isOn)
        {
            return;
        }
        
        else
        {
            _numberOfPlayers = numberOfPlayersSelectionTogglesArray[0].isOn ? 2 : 3;
        }
        
        numberOfPlayersSelectionPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(true);

        string[] nameKeys = new string[_numberOfPlayers];
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            nameKeys[i] = "Player" + i + "Name";
        }
        
        PlayerPrefsManager.LoadData(ref _playerNamesArray , nameKeys);
        
        string[] winsKeys = new string[_numberOfPlayers];
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            winsKeys[i] = "Player" + i + "TotalWins";
        }
        
        PlayerPrefsManager.LoadData(ref _playersTotalWinsArray , winsKeys);
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerNameTMPInputFields[i].text = _playerNamesArray[i];
            playerTotalWinsLabelsTMPTexts[i].text = _playerNamesArray[i] + " Total Wins: " + _playersTotalWinsArray[i];
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
        
        string[] nameKeys = new string[_numberOfPlayers];
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            nameKeys[i] = "Player" + i + "Name";
        }
        
        PlayerPrefsManager.SaveData(_playerNamesArray , nameKeys);

        string[] winsKeys = new string[_numberOfPlayers];
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            winsKeys[i] = "Player" + i + "TotalWins";
        }
        
        PlayerPrefsManager.SaveData(_playersTotalWinsArray , winsKeys);
    }

    public void EnterButton()
    {
        string[] defaultPlayerNames =
        {
            "Red",
            "Green",
            "Blue"
        };

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
                _playerNamesArray[i] = defaultPlayerNames[i];
                playerNameTMPInputFields[i].text = _playerNamesArray[i];
                aiHumanTogglesArray[0].isOn = true;
            }
        }
        
        inGameUIPanelsObj.SetActive(true);
        playerInputPanelObj.SetActive(false);
        EventsManager.Invoke(Event.GameStarted);
    }

    public void HolesToggle()
    {
        EventsManager.Invoke(Event.HolesToggled);
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

    public void LeaderBoardButtonPauseMenu()
    {
        backButtonTMPText.text = "Back to Game";
        inGameUIPanelsObj.SetActive(false);
        leaderboardPanelObj.SetActive(true);
        
        if(_numberOfPlayers == 2)
        {
            leaderboardWinsPanelObjs[_numberOfPlayers].SetActive(false);
            totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;
        }
        
        pauseButtonObj.SetActive(false);
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

    public void RandomTurnsToggle()
    {
        EventsManager.Invoke(Event.RandomTurnsToggled);
    }

    public void ResetButton()
    {
        PlayerPrefsManager.DeleteAll();
        
        EventsManager.Invoke(Event.GameDataReset);
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerNameTMPInputFields[i].text = "";
            _playersTotalWinsArray[i] = 0;
            playerTotalWinsLabelsTMPTexts[i].text = "";
        }
        
        string[] nameKeys = new string[_numberOfPlayers];

        for(int i = 0; i < _numberOfPlayers; i++)
        {
            nameKeys[i] = "Player" + i + "Name";
        }
        
        PlayerPrefsManager.LoadData(ref _playerNamesArray , nameKeys);
        
        string[] winsKeys = new string[_numberOfPlayers];
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            winsKeys[i] = "Player" + i + "TotalWins";
        }
        
        PlayerPrefsManager.LoadData(ref _playersTotalWinsArray , winsKeys);
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerNameTMPInputFields[i].text = _playerNamesArray[i];
            playerTotalWinsLabelsTMPTexts[i].text = _playerNamesArray[i] + " Total Wins: " + _playersTotalWinsArray[i];
        }
    }

    public void ResumeButton()
    {
        EventsManager.Invoke(Event.GameResumed);
        inGameUIPanelsObj.SetActive(true);
        pauseButtonObj.SetActive(true);
        pauseMenuPanelObj.SetActive(false);
    }

    public void SetPlayersNumber()
    {
        if(numberOfPlayersSelectionTogglesArray[0].isOn)
        {
            _numberOfPlayers = 2;
            _playerNamesArray = new string[_numberOfPlayers];
            _playersTotalWinsArray = new int[_numberOfPlayers];
            playerNameTMPInputFields[_numberOfPlayers].gameObject.SetActive(false);
            EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
        }
        
        else if(numberOfPlayersSelectionTogglesArray[1].isOn)
        {
            _numberOfPlayers = 3;
            _playerNamesArray = new string[_numberOfPlayers];
            _playersTotalWinsArray = new int[_numberOfPlayers];
            playerNameTMPInputFields[_numberOfPlayers - 1].gameObject.SetActive(true);
            EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
        }
    }

    private void OnGameOver()
    {
        coinUIObj.SetActive(false);
        continueButtonObj.SetActive(true);
        pauseButtonObj.SetActive(false);
    }

    private void OnGameTied()
    {
        gameTiedPanelObj.SetActive(true);
    }

    private void OnPlayerWins(int highestScorePlayerID)
    {
        _highestScorePlayerID = highestScorePlayerID;

        for(int i = 0; i < _playersTotalWinsArray.Length; i++)
        {
            if(i == _highestScorePlayerID)
            {
                _playersTotalWinsArray[i]++;   
            }
        }
        
        playerWinsLabelsTMPTexts[_highestScorePlayerID].text = playerNameTMPInputFields[_highestScorePlayerID].text + " Wins!!!";
        winsPanelObjs[_highestScorePlayerID].SetActive(true);
        
        string[] nameKeys = new string[_numberOfPlayers];
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            nameKeys[i] = "Player" + i + "Name";
        }
        
        PlayerPrefsManager.SaveData(_playerNamesArray , nameKeys);

        string[] winsKeys = new string[_numberOfPlayers];
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            winsKeys[i] = "Player" + i + "TotalWins";
        }
        
        PlayerPrefsManager.SaveData(_playersTotalWinsArray , winsKeys);
    }

    private void OnScoreUpdated(int[] coinScoresArray)
    {
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            coinScoreTMPTexts[i].text = _playerNamesArray[i] + " : " + coinScoresArray[i];
        }
    }

    private void OnTotalReceived(int[] totalReceivedArray)
    {
        _totalReceivedArray = totalReceivedArray;
    }
    
    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameOver , new Action(OnGameOver));
        EventsManager.SubscribeToEvent(Event.GameTied , new Action(OnGameTied));
        EventsManager.SubscribeToEvent(Event.PlayerWins , (Action<int>)OnPlayerWins);
        EventsManager.SubscribeToEvent(Event.ScoreUpdated , (Action<int[]>)OnScoreUpdated);
        EventsManager.SubscribeToEvent(Event.PlayerTotalReceived , (Action<int[]>)OnTotalReceived);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
        EventsManager.UnsubscribeFromEvent(Event.GameTied , new Action(OnGameTied));
        EventsManager.UnsubscribeFromEvent(Event.PlayerWins , (Action<int>)OnPlayerWins);
        EventsManager.UnsubscribeFromEvent(Event.ScoreUpdated , (Action<int[]>)OnScoreUpdated);
        EventsManager.UnsubscribeFromEvent(Event.PlayerTotalReceived , (Action<int[]>)OnTotalReceived);
    }
}