using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int _redCoinScoreValue;
    private int _greenCoinScoreValue;
    private int _blueCoinScoreValue;

    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private TMP_Text blueCoinScoreTMP;
    [SerializeField] private TMP_Text greenCoinScoreTMP;
    [SerializeField] private TMP_Text redCoinScoreTMP;

    private void Start()
    {
        gameOverPanelsObj.SetActive(false);
        UpdateScoreTexts();
    }
    
    public void CoinBuffedUpScore(int buffedUpCoinPlayerID, int buffedUpCoinIncrement)
    {
        if(buffedUpCoinPlayerID == 0)
        {
            _redCoinScoreValue += buffedUpCoinIncrement;
        }
        
        else if(buffedUpCoinPlayerID == 1)
        {
            _greenCoinScoreValue += buffedUpCoinIncrement;
        }
        
        else if(buffedUpCoinPlayerID == 2)
        {
            _blueCoinScoreValue += buffedUpCoinIncrement;
        }

        UpdateScoreTexts();
    }

    public void CoinCapturedScore(int capturingPlayerID , int capturedPlayerID , int capturedCoinValue)
    {
        if(capturingPlayerID == 0)
        {
            _redCoinScoreValue += capturedCoinValue;
            
            if(capturedPlayerID == 1)
            {
                _greenCoinScoreValue -= capturedCoinValue;
            }
            
            else if(capturedPlayerID == 2)
            {
                _blueCoinScoreValue -= capturedCoinValue;
            }
        }
        
        else if(capturingPlayerID == 1)
        {
            _greenCoinScoreValue += capturedCoinValue;
            
            if(capturedPlayerID == 0)
            {
                _redCoinScoreValue -= capturedCoinValue;
            }
            
            else if(capturedPlayerID == 2)
            {
                _blueCoinScoreValue -= capturedCoinValue;
            }
        }
        
        else if(capturingPlayerID == 2)
        {
            _blueCoinScoreValue += capturedCoinValue;
            
            if(capturedPlayerID == 0)
            {
                _redCoinScoreValue -= capturedCoinValue;
            }
            
            else if(capturedPlayerID == 1)
            {
                _greenCoinScoreValue -= capturedCoinValue;
            }
        }

        UpdateScoreTexts();
    }

    public void CoinPlacedScore(int coinValue , int playerID)
    {
        if(playerID == 0)
        {
            _redCoinScoreValue += coinValue;
        }
        
        else if(playerID == 1)
        {
            _greenCoinScoreValue += coinValue;
        }
        
        else if(playerID == 2)
        {
            _blueCoinScoreValue += coinValue;
        }

        UpdateScoreTexts();
    }

    private void UpdateScoreTexts()
    {
        redCoinScoreTMP.text = _redCoinScoreValue.ToString();
        greenCoinScoreTMP.text = _greenCoinScoreValue.ToString();
        blueCoinScoreTMP.text = _blueCoinScoreValue.ToString();
    }
}
