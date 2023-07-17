using Event = Events.Event;
using Events;
using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _numberOfPlayers;
    private int[] _coinScoreValuesArray;
    private string[] _playerNamesArray;

    private void Start()
    {
        SubscribeToEvents();
        EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void CoinBuffedUpScore(int buffedUpCoinPlayerID , int buffedUpCoinIncrement)
    {
        _coinScoreValuesArray[buffedUpCoinPlayerID] += buffedUpCoinIncrement;
        EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
    }

    private void CoinCapturedScore(int capturingPlayerID , int capturedPlayerID , int capturedCoinValue)
    {
        _coinScoreValuesArray[capturingPlayerID] += capturedCoinValue;

        if(_coinScoreValuesArray[capturedPlayerID] > 0)
        {
            _coinScoreValuesArray[capturedPlayerID] -= capturedCoinValue;
            EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);   
        }
    }
    
    private int GetHighestScorePlayer()
    {
        int highestScore = int.MinValue;
        int highestScorePlayerID = -1;

        for(int i = 0; i < _numberOfPlayers; i++)
        {
            if(_coinScoreValuesArray[i] > highestScore)
            {
                highestScore = _coinScoreValuesArray[i];
                highestScorePlayerID = i;
            }
        }

        return highestScorePlayerID;
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
        _coinScoreValuesArray[playerID] += coinValue;
        EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
    }

    private void OnGameOver()
    {
        int highestScorePlayerID = GetHighestScorePlayer();

        for(int i = 0; i < _numberOfPlayers - 1; i++)
        {
            for(int j = i + 1; j < _numberOfPlayers; j++)
            {
                if(_coinScoreValuesArray[i] == _coinScoreValuesArray[j])
                {
                    for(int k = 0; k < _numberOfPlayers; k++)
                    {
                        if(_coinScoreValuesArray[k] > _coinScoreValuesArray[i] && _coinScoreValuesArray[k] > _coinScoreValuesArray[j])
                        {
                            EventsManager.Invoke(Event.PlayerWins , k);
                            return;
                        }
                    }

                    EventsManager.Invoke(Event.GameTied);
                    return;
                }
            }
        }

        EventsManager.Invoke(Event.PlayerWins , highestScorePlayerID);
    }

    private void OnNumberOfPlayersSelected(int numberOfPlayers)
    {
        _numberOfPlayers = numberOfPlayers;
        _coinScoreValuesArray = new int[_numberOfPlayers];
        _playerNamesArray = new string[_numberOfPlayers];
    }
    
    private void OnPlayerNamesUpdated(int playerID , string playerName)
    {
        _playerNamesArray[playerID] = playerName;
        EventsManager.Invoke(Event.ScoreUpdated , _coinScoreValuesArray);
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.CoinBuffedUp , (Action<int , int>)OnCoinBuffedUp);
        EventsManager.SubscribeToEvent(Event.CoinCaptured , (Action<int , int , int>)OnCoinCaptured);
        EventsManager.SubscribeToEvent(Event.CoinPlaced , (Action<int , int>)OnCoinPlaced);
        EventsManager.SubscribeToEvent(Event.GameOver , new Action(OnGameOver));
        EventsManager.SubscribeToEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
        EventsManager.SubscribeToEvent(Event.PlayerNamesUpdated , (Action<int , string>)OnPlayerNamesUpdated);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.CoinBuffedUp , (Action<int , int>)OnCoinBuffedUp);
        EventsManager.UnsubscribeFromEvent(Event.CoinCaptured , (Action<int , int , int>)OnCoinCaptured);
        EventsManager.UnsubscribeFromEvent(Event.CoinPlaced , (Action<int , int>)OnCoinPlaced);
        EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
        EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
        EventsManager.UnsubscribeFromEvent(Event.PlayerNamesUpdated , (Action<int , string>)OnPlayerNamesUpdated);
    }
}