using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int[] _scoreValuesArray;
    
    [SerializeField] private TMP_Text[] scoreLabelsArray;

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void OnCoinPlaced(int coinValue , int playerID)
    {
        //Debug.Log("Player " + playerID + " placed the coin and " + " coin value : " + coinValue);
        _scoreValuesArray[playerID] += coinValue;
        scoreLabelsArray[playerID].text = _scoreValuesArray[playerID].ToString();
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.CoinPlaced , OnCoinPlaced);
    }

    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.CoinPlaced , OnCoinPlaced);
    }
}
