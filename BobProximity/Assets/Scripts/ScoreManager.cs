using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private int[] _coinScoreValues;
    private MainMenuManager _mainMenuManager;
    private InGameUIManager _inGameUIManager;

    [SerializeField] private TMP_Text[] coinScoreTMPTexts;

    public int[] CoinScoreValues => _coinScoreValues;

    private void Start()
    {
        _mainMenuManager = FindObjectOfType<MainMenuManager>();
        _coinScoreValues = new int[_mainMenuManager.TotalNumberOfPlayers];
        _inGameUIManager = FindObjectOfType<InGameUIManager>();
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
        for(int i = 0; i < _mainMenuManager.TotalNumberOfPlayers; i++)
        {
            coinScoreTMPTexts[i].text = _inGameUIManager.PlayerNameTMPInputFields[i].text + " : " + CoinScoreValues[i];
        }
    }
}
