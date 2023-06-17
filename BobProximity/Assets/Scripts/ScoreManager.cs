using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int redCoinScore = 0;
    private int greenCoinScore = 0;
    private int blueCoinScore = 0;
    
    [SerializeField] private TMP_Text blueCoinValueTMP;
    [SerializeField] private TMP_Text greenCoinValueTMP;
    [SerializeField] private TMP_Text redCoinValueTMP;

    private void Start()
    {
        UpdateScoreTexts();
    }
    
    public void CaptureCoin(int capturingPlayerID , int capturedPlayerID , int capturedCoinValue)
    {
        if(capturingPlayerID == 0)
        {
            redCoinScore += capturedCoinValue;
            
            if(capturedPlayerID == 1)
            {
                greenCoinScore -= capturedCoinValue;
            }
            
            else if(capturedPlayerID == 2)
            {
                blueCoinScore -= capturedCoinValue;
            }
        }
        
        else if(capturingPlayerID == 1)
        {
            greenCoinScore += capturedCoinValue;
            
            if(capturedPlayerID == 0)
            {
                redCoinScore -= capturedCoinValue;
            }
            
            else if(capturedPlayerID == 2)
            {
                blueCoinScore -= capturedCoinValue;
            }
        }
        
        else if(capturingPlayerID == 2)
        {
            blueCoinScore += capturedCoinValue;
            
            if(capturedPlayerID == 0)
            {
                redCoinScore -= capturedCoinValue;
            }
            
            else if(capturedPlayerID == 1)
            {
                greenCoinScore -= capturedCoinValue;
            }
        }

        UpdateScoreTexts();
    }

    public void PlaceCoin(int coinValue , int playerID)
    {
        if(playerID == 0)
        {
            redCoinScore += coinValue;
        }
        
        else if(playerID == 1)
        {
            greenCoinScore += coinValue;
        }
        
        else if(playerID == 2)
        {
            blueCoinScore += coinValue;
        }

        UpdateScoreTexts();
    }

    private void UpdateScoreTexts()
    {
        redCoinValueTMP.text = redCoinScore.ToString();
        greenCoinValueTMP.text = greenCoinScore.ToString();
        blueCoinValueTMP.text = blueCoinScore.ToString();
    }
}
