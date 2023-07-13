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
    [SerializeField] private TMP_Text[] coinScoreTMPTexts;
    [SerializeField] private Toggle[] aiHumanTogglesArray;

    private void Start()
    {
        continueButtonObj.SetActive(false);
        gameOverPanelsObj.SetActive(false);
        gameTiedPanelObj.SetActive(false);
        inGameUIPanelsObj.SetActive(false);
        leaderboardPanelObj.SetActive(false);
        playerInputPanelObj.SetActive(false);
        
        _playersTotalWinsArray = new int[_numberOfPlayers];
        _totalReceivedArray = new int[_numberOfPlayers];

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
        for(int i = 0; i < aiHumanTogglesArray.Length; i++)
        {
            bool isAI = aiHumanTogglesArray[i].isOn;
            EventsManager.Invoke(Event.AIHumanToggled , i , isAI);
        }
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
        
        PlayerPrefsManager.LoadData(_playerNamesArray, _playersTotalWinsArray);

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
        
        PlayerPrefsManager.SaveData(_playerNamesArray , _playersTotalWinsArray);
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

    public void RandomTurnsToggle()
    {
        EventsManager.Invoke(Event.RandomTurnsToggled);
    }

    public void ResetButton()
    {
        PlayerPrefsManager.DeleteAll(_playerNamesArray , _playersTotalWinsArray);
        EventsManager.Invoke(Event.GameDataReset);
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerNameTMPInputFields[i].text = "";
            _playersTotalWinsArray[i] = 0;
            playerTotalWinsLabelsTMPTexts[i].text = "";
        }
        
        PlayerPrefsManager.LoadData(_playerNamesArray , _playersTotalWinsArray);
        
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            playerNameTMPInputFields[i].text = _playerNamesArray[i];
            playerTotalWinsLabelsTMPTexts[i].text = _playerNamesArray[i] + " Total Wins: " + _playersTotalWinsArray[i];
        }
    }
    
    public void SetPlayersNumber()
    {
        if(numberOfPlayersSelectionSlider.value == 0)
        {
            _numberOfPlayers = 2;
            _playerNamesArray = new string[_numberOfPlayers];
            _playersTotalWinsArray = new int[_numberOfPlayers];
            playerNameTMPInputFields[_numberOfPlayers].gameObject.SetActive(false);
            EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
        }
        
        else if(numberOfPlayersSelectionSlider.value == 1)
        {
            _numberOfPlayers = 3;
            _playerNamesArray = new string[_numberOfPlayers];
            _playersTotalWinsArray = new int[_numberOfPlayers];
            EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
        }
    }

    private void OnGameOver()
    {
        coinUIObj.SetActive(false);
        continueButtonObj.SetActive(true);
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
        
        PlayerPrefsManager.SaveData(_playerNamesArray , _playersTotalWinsArray);
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