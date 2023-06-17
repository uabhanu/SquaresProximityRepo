using Event = Events.Event;
using Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _blueScoreValue;
    private int _greenScoreValue;
    private int _redScoreValue;
    
    private List<int> _blueCoinValuesList;
    private List<int> _greenCoinValuesList;
    private List<int> _redCoinValuesList;

    [SerializeField] private TMP_Text blueCoinValueTMP;
    [SerializeField] private TMP_Text greenCoinValueTMP;
    [SerializeField] private TMP_Text redCoinValueTMP;

    private void Start()
    {
        _blueCoinValuesList = new List<int>();
        _greenCoinValuesList = new List<int>();
        _redCoinValuesList = new List<int>();
        
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void OnCoinCaptured(int coinValue , int playerID)
    {
        if(playerID == 0)
        {
            _redCoinValuesList.Remove(coinValue);
        }

        if(playerID == 1)
        {
            _greenCoinValuesList.Remove(coinValue);
        }
        
        if(playerID == 2)
        {
            _blueCoinValuesList.Remove(coinValue);
        }
    }

    private void OnCoinPlaced(int coinValue , int playerID)
    {
        if(playerID == 0)
        {
            _redCoinValuesList.Add(coinValue);
        }

        if(playerID == 1)
        {
            _greenCoinValuesList.Add(coinValue);
        }
        
        if(playerID == 2)
        {
            _blueCoinValuesList.Add(coinValue);
        }
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.CoinCaptured , OnCoinCaptured);
        EventsManager.SubscribeToEvent(Event.CoinPlaced , OnCoinPlaced);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.CoinCaptured , OnCoinCaptured);
        EventsManager.UnsubscribeFromEvent(Event.CoinPlaced , OnCoinPlaced);
    }
}
