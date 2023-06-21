using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int[] _coinScoreValues;

    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private TMP_Text[] coinScoreTexts;

    private void Start()
    {
        gameOverPanelsObj.SetActive(false);
        _coinScoreValues = new int[3];
        UpdateScoreTexts();
    }
    
    public void CoinBuffedUpScore(int buffedUpCoinPlayerID , int buffedUpCoinIncrement)
    {
        _coinScoreValues[buffedUpCoinPlayerID] += buffedUpCoinIncrement;
        UpdateScoreTexts();
    }

    public void CoinCapturedScore(int capturingPlayerID , int capturedPlayerID , int capturedCoinValue)
    {
        _coinScoreValues[capturingPlayerID] += capturedCoinValue;
        _coinScoreValues[capturedPlayerID] -= capturedCoinValue;
        UpdateScoreTexts();
    }

    public void CoinPlacedScore(int coinValue , int playerID)
    {
        _coinScoreValues[playerID] += coinValue;
        UpdateScoreTexts();
    }

    private void UpdateScoreTexts()
    {
        for(int i = 0; i < coinScoreTexts.Length; i++)
        {
            coinScoreTexts[i].text = _coinScoreValues[i].ToString();
        }
    }
}
