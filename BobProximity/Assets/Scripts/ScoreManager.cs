using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _numberOfPlayers;
    private int[] _coinScoreValues;
    private string[] _playerNamesArray;

    [SerializeField] private TMP_Text[] coinScoreTMPTexts;

    private void Start()
    {
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

        for(int i = 0; i < _numberOfPlayers; i++)
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
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            coinScoreTMPTexts[i].text = _playerNamesArray[i] + " : " + _coinScoreValues[i];
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
        for(int i = 0; i < _numberOfPlayers - 1; i++)
        {
            for(int j = i + 1; j < _numberOfPlayers; j++)
            {
                if(_coinScoreValues[i] == _coinScoreValues[j])
                {
                    EventsManager.Invoke(Event.GameTied);
                }
                else
                {
                    int highestScorePlayerID = GetHighestScorePlayer();
                    EventsManager.Invoke(Event.PlayerWins , highestScorePlayerID);
                }
            }
        }
    }

    private void OnNumberOfPlayersSelected(int numberOfPlayers)
    {
        _numberOfPlayers = numberOfPlayers;
        _coinScoreValues = new int[_numberOfPlayers];
        _playerNamesArray = new string[_numberOfPlayers];
    }
    
    private void OnPlayerNamesUpdated(int playerID , string playerName)
    {
        _playerNamesArray[playerID] = playerName;
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