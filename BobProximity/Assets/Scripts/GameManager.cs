using Event = Events.Event;
using Events;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool _gameStarted;
    private bool _isGameTied;
     private GridManager _gridManager;
    private InGameUIManager _inGameUIManager;
    private int[] _playerTotalWinsArray;
    private int _totalCells;
    private MainMenuManager _mainMenuManager;

    public bool GameStarted
    {
        get => _gameStarted;
        set => _gameStarted = value;
    }
    
    public int TotalCells
    {
        get => _totalCells;
        set => _totalCells = value;
    }

    public int[] PlayerTotalWinsArray
    {
        get => _playerTotalWinsArray;
        set => _playerTotalWinsArray = value;
    }

    private void Start()
    {
        _inGameUIManager = FindObjectOfType<InGameUIManager>();
        _mainMenuManager = FindObjectOfType<MainMenuManager>();
         _gridManager = FindObjectOfType<GridManager>();
         
        PlayerTotalWinsArray = new int[_mainMenuManager.TotalNumberOfPlayers];

        TotalCells = _gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows;

        LoadData();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void LoadData()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            _inGameUIManager.PlayerNameTMPInputFields[i].text = PlayerPrefs.GetString("Player " + i + " Name");
            PlayerTotalWinsArray[i] = PlayerPrefs.GetInt("Player " + i + " Total Wins");
            _inGameUIManager.PlayerTotalWinsLabelsTMPTexts[i].text = _inGameUIManager.PlayerNameTMPInputFields[i].text + " Total Wins : " + PlayerTotalWinsArray[i];
        }
    }

    public void SaveData()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            PlayerPrefs.SetString("Player " + i + " Name" , _inGameUIManager.PlayerNameTMPInputFields[i].text);
            PlayerPrefs.SetInt("Player " + i + " Total Wins" , PlayerTotalWinsArray[i]);
        }

        PlayerPrefs.Save();
    }

    private void OnGameDataReset()
    {
        PlayerPrefs.DeleteAll();
        LoadData();
    }
    
    private void OnGameOver()
    {
        GameStarted = false;
    }

    private void OnGamePaused()
    {
        GameStarted = false;
    }
    
    private void OnGameResumed()
    {
        GameStarted = true;
    }

    private void OnGameStarted()
    {
        GameStarted = true;
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
        EventsManager.SubscribeToEvent(Event.GameOver , OnGameOver);
        EventsManager.SubscribeToEvent(Event.GamePaused , OnGamePaused);
        EventsManager.SubscribeToEvent(Event.GameResumed , OnGameResumed);
        EventsManager.SubscribeToEvent(Event.GameStarted , OnGameStarted);
        EventsManager.SubscribeToEvent(Event.GameTied , OnGameTied);
        EventsManager.SubscribeToEvent(Event.PlayerWins , OnPlayerWins);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameDataReset , OnGameDataReset);
        EventsManager.UnsubscribeFromEvent(Event.GameOver , OnGameOver);
        EventsManager.UnsubscribeFromEvent(Event.GamePaused , OnGamePaused);
        EventsManager.UnsubscribeFromEvent(Event.GameResumed , OnGameResumed);
        EventsManager.UnsubscribeFromEvent(Event.GameStarted , OnGameStarted);
        EventsManager.UnsubscribeFromEvent(Event.GameTied , OnGameTied);
        EventsManager.UnsubscribeFromEvent(Event.PlayerWins , OnPlayerWins);
    }
}
