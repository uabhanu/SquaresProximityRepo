using Event = Events.Event;
using Events;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int _numberOfPlayers;
    private int[] _coinScoreValues;
    private MainMenuManager _mainMenuManager;
    private string[] _playerNames;

    [SerializeField] private TMP_Text[] coinScoreTMPTexts;

    private void Start()
    {
        _mainMenuManager = FindObjectOfType<MainMenuManager>();
        _coinScoreValues = new int[_mainMenuManager.TotalNumberOfPlayers];
        _playerNames = new string[_mainMenuManager.TotalNumberOfPlayers];
        SubscribeToEvents();
        UpdateScoreTexts();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void CoinBuffedUpScore(int buffedUpCoinPlayerID , int buffedUpCoinIncrement)
    {
        _coinScoreValues[buffedUpCoinPlayerID] += buffedUpCoinIncrement;
        UpdateScoreTexts();
    }

    private void CoinCapturedScore(int capturingPlayerID , int capturedPlayerID , int capturedCoinValue)
    {
        _coinScoreValues[capturingPlayerID] += capturedCoinValue;
        _coinScoreValues[capturedPlayerID] -= capturedCoinValue;
        UpdateScoreTexts();
    }
    
    private int GetHighestScorePlayer()
    {
        int highestScore = int.MinValue;
        int highestScorePlayer = -1;

        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            if(_coinScoreValues[i] > highestScore)
            {
                highestScore = _coinScoreValues[i];
                highestScorePlayer = i;
            }
        }

        return highestScorePlayer;
    }

    private void UpdateScoreTexts()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            coinScoreTMPTexts[i].text = _playerNames[i] + " : " + _coinScoreValues[i];
        }
    }

    private void OnCoinBuffedUp(int playerID , int coinValue)
    {
        CoinBuffedUpScore(playerID , coinValue);
    }

    private void OnCoinCaptured(int currentPlayerID , int adjacentPlayerID , int adjacentPlayerCoinValue)
    {
        CoinCapturedScore(currentPlayerID , adjacentPlayerID , adjacentPlayerCoinValue);
    }

    private void OnCoinPlaced(int coinValue , int playerID)
    {
        _coinScoreValues[playerID] += coinValue;
        UpdateScoreTexts();
    }

    private void OnGameOver()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers - 1; i++)
        {
            for(int j = i + 1; j < _mainMenuManager.TotalNumberOfPlayers; j++)
            {
                if(_coinScoreValues[i] == _coinScoreValues[j])
                {
                    EventsManager.Invoke(Event.GameTied);
                }
                else
                {
                    int highestScorePlayer = GetHighestScorePlayer();
                    EventsManager.Invoke(Event.PlayerWins , highestScorePlayer);
                }
            }
        }
    }

    private void OnNumberOfPlayersSelected(int numberOfPlayers)
    {
        _numberOfPlayers = numberOfPlayers;
        Debug.Log("Number Of Players : " + _numberOfPlayers);
    }
    
    private void OnPlayerNamesUpdated(int playerID , string playerName)
    {
        _playerNames[playerID] = playerName;
        UpdateScoreTexts();
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.CoinBuffedUp , OnCoinBuffedUp);
        EventsManager.SubscribeToEvent(Event.CoinCaptured , OnCoinCaptured);
        EventsManager.SubscribeToEvent(Event.CoinPlaced , OnCoinPlaced);
        EventsManager.SubscribeToEvent(Event.GameOver , OnGameOver);
        EventsManager.SubscribeToEvent(Event.NumberOfPlayersSelected , OnNumberOfPlayersSelected);
        EventsManager.SubscribeToEvent(Event.PlayerNamesUpdated , OnPlayerNamesUpdated);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.CoinBuffedUp , OnCoinBuffedUp);
        EventsManager.UnsubscribeFromEvent(Event.CoinCaptured , OnCoinCaptured);
        EventsManager.UnsubscribeFromEvent(Event.CoinPlaced , OnCoinPlaced);
        EventsManager.UnsubscribeFromEvent(Event.GameOver , OnGameOver);
        EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersSelected , OnNumberOfPlayersSelected);
        EventsManager.UnsubscribeFromEvent(Event.PlayerNamesUpdated , OnPlayerNamesUpdated);
    }
}