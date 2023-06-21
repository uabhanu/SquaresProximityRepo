using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int[] _coinScoreValues;

    [SerializeField] private TMP_Text[] coinScoreTexts;

    public int[] CoinScoreValues => _coinScoreValues;

    private void Start()
    {
        _coinScoreValues = new int[3];
        UpdateScoreTexts();
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
        for(int i = 0; i < coinScoreTexts.Length; i++)
        {
            coinScoreTexts[i].text = CoinScoreValues[i].ToString();
        }
    }
}
