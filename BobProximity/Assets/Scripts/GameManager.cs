using Event = Events.Event;
using Events;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool _isGameTied;
    private int _numberOfPlayers;
    private int[] _playerTotalWinsArray;

    private void Start()
    {
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
            _playerTotalWinsArray[i] = PlayerPrefs.GetInt("Player " + i + " Total Wins");
            EventsManager.Invoke(Event.GameDataLoaded , _playerTotalWinsArray);
        }
    }

    private void SaveData()
    {
        for(int i = 0; i < _numberOfPlayers; i++)
        {
            PlayerPrefs.SetInt("Player " + i + " Total Wins" , _playerTotalWinsArray[i]);
        }

        PlayerPrefs.Save();
    }

    private void OnGameDataReset()
    {
        PlayerPrefs.DeleteAll();
    }

    private void OnGameTied()
    {
        _isGameTied = true;
    }

    private void OnNumberOfPlayersSelected(int numberOfPlayers)
    {
        _numberOfPlayers = numberOfPlayers;
        _playerTotalWinsArray = new int[_numberOfPlayers];
        LoadData();
    }

    private void OnPlayerWins(int highestScorePlayerID)
    {
        if(_isGameTied) return;

        for(int i = 0; i < _playerTotalWinsArray.Length; i++)
        {
            if(i == highestScorePlayerID)
            {
                Debug.Log("i : " + i);
                Debug.Log("Highest Score Player ID :  : " + highestScorePlayerID);
                _playerTotalWinsArray[i]++; //ToDo This is increasing by 3 rather than 1 so fix this
            }
        }
        
        SaveData();
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameDataReset , OnGameDataReset);
        EventsManager.SubscribeToEvent(Event.GameTied , OnGameTied);
        EventsManager.SubscribeToEvent(Event.NumberOfPlayersSelected , OnNumberOfPlayersSelected);
        EventsManager.SubscribeToEvent(Event.PlayerWins , OnPlayerWins);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameDataReset , OnGameDataReset);
        EventsManager.UnsubscribeFromEvent(Event.GameTied , OnGameTied);
        EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersSelected , OnNumberOfPlayersSelected);
        EventsManager.UnsubscribeFromEvent(Event.PlayerWins , OnPlayerWins);
    }
}
