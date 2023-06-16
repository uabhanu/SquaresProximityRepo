using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private int[] scoreValuesArray;
    [SerializeField] private TMP_Text[] scoreValueLabelsArray;

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
        scoreValuesArray[playerID] += coinValue;
        scoreValueLabelsArray[playerID].text = scoreValuesArray[playerID].ToString();
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
