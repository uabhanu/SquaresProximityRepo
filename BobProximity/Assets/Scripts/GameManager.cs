using Event = Events.Event;
using Events;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool _isGameTied;
    private int[] _playerTotalWinsArray;
    private MainMenuManager _mainMenuManager;

    private void Start()
    {
        _mainMenuManager = FindObjectOfType<MainMenuManager>();

        _playerTotalWinsArray = new int[_mainMenuManager.TotalNumberOfPlayers];

         LoadData();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void LoadData()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            _playerTotalWinsArray[i] = PlayerPrefs.GetInt("Player " + i + " Total Wins");
            EventsManager.Invoke(Event.GameDataLoaded , _playerTotalWinsArray);
        }
    }

    private void SaveData()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            PlayerPrefs.SetInt("Player " + i + " Total Wins" , _playerTotalWinsArray[i]);
        }

        PlayerPrefs.Save();
    }

    private void OnGameDataReset()
    {
        PlayerPrefs.DeleteAll();
        LoadData();
    }

    private void OnGameTied()
    {
        _isGameTied = true;
    }

    private void OnPlayerWins(int highestScorePlayer)
    {
        if(_isGameTied) return;
        
        _playerTotalWinsArray[highestScorePlayer]++;
        SaveData();
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameDataReset , OnGameDataReset);
        EventsManager.SubscribeToEvent(Event.GameTied , OnGameTied);
        EventsManager.SubscribeToEvent(Event.PlayerWins , OnPlayerWins);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameDataReset , OnGameDataReset);
        EventsManager.UnsubscribeFromEvent(Event.GameTied , OnGameTied);
        EventsManager.UnsubscribeFromEvent(Event.PlayerWins , OnPlayerWins);
    }
}
