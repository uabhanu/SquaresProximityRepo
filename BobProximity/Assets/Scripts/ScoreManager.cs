using Events;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int[] _coinScoreValues;
    private MainMenuManager _mainMenuManager;
    private string[] _playerNames;

    [SerializeField] private TMP_Text[] coinScoreTMPTexts;

    public int[] CoinScoreValues => _coinScoreValues;

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

    public void CoinBuffedUpScore(int buffedUpCoinPlayerID , int buffedUpCoinIncrement)
    {
        CoinScoreValues[buffedUpCoinPlayerID] += buffedUpCoinIncrement;
        UpdateScoreTexts();
    }

    public void CoinCapturedScore(int capturingPlayerID , int capturedPlayerID , int capturedCoinValue)
    {
        CoinScoreValues[capturingPlayerID] += capturedCoinValue;
        CoinScoreValues[capturedPlayerID] -= capturedCoinValue;
        UpdateScoreTexts();
    }

    public void CoinPlacedScore(int coinValue , int playerID)
    {
        CoinScoreValues[playerID] += coinValue;
        UpdateScoreTexts();
    }

    private void UpdateScoreTexts()
    {
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            coinScoreTMPTexts[i].text = _playerNames[i] + " : " + CoinScoreValues[i];
        }
    }
    
    public void OnPlayerNamesUpdated(int playerID , string playerName)
    {
        _playerNames[playerID] = playerName;
        UpdateScoreTexts();
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Events.Event.PlayerNamesUpdated , OnPlayerNamesUpdated);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Events.Event.PlayerNamesUpdated , OnPlayerNamesUpdated);
    }
}