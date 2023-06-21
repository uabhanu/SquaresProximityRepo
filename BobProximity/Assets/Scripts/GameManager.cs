using Event = Events.Event;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private GridManager _gridManager;
    private int _totalCells;
    private PlayerController _playerController;
    private ScoreManager _scoreManager;
    
    [SerializeField] private GameObject gameOverPanelsObj;
    [SerializeField] private TMP_Text[] totalReceivedTexts;
    [SerializeField] private GameObject[] winsPanelObjs;
    
    public int TotalCells
    {
        get => _totalCells;
        set => _totalCells = value;
    }

    private void Start()
    {
        gameOverPanelsObj.SetActive(false);
        _gridManager = FindObjectOfType<GridManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _scoreManager = FindObjectOfType<ScoreManager>();
        TotalCells = _gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows;

        for(int i = 0; i < winsPanelObjs.Length; i++)
        {
            winsPanelObjs[i].SetActive(false);
        }
        
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private int GetHighestScorePlayer()
    {
        int highestScore = int.MinValue;
        int highestScorePlayer = -1;

        for(int i = 0; i < _scoreManager.CoinScoreValues.Length; i++)
        {
            if(_scoreManager.CoinScoreValues[i] > highestScore)
            {
                highestScore = _scoreManager.CoinScoreValues[i];
                highestScorePlayer = i;
            }
        }

        return highestScorePlayer;
    }

    private void OnGameOver()
    {
        int highestScorePlayer = GetHighestScorePlayer();
        string playerName = "Player " + highestScorePlayer;
        Debug.Log(playerName + " wins with a score of " + _scoreManager.CoinScoreValues[highestScorePlayer]);
        gameOverPanelsObj.SetActive(true);
        
        for(int i = 0; i < totalReceivedTexts.Length; i++)
        {
            totalReceivedTexts[i].text = _playerController.TotalReceivedArray[i].ToString();
        }
        
        winsPanelObjs[highestScorePlayer].SetActive(true);
    }

    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.GameOver , OnGameOver);
    }
    
    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.GameOver , OnGameOver);
    }

    public void OkButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
