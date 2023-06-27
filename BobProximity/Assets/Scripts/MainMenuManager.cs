using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager _instance;
    
    private int _totalNumberOfPlayers;

    [SerializeField] private Slider numberOfPlayersSelectionSlider;

    public int TotalNumberOfPlayers
    {
        get => _totalNumberOfPlayers;
        set => _totalNumberOfPlayers = value;
    }

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ConfirmButton()
    {
        if(numberOfPlayersSelectionSlider.value == 0 || numberOfPlayersSelectionSlider.value == 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
        }
    }

    public void SetPlayersNumber()
    {
        if(numberOfPlayersSelectionSlider.value == 0)
        {
            TotalNumberOfPlayers = 2;
        }
        
        else if(numberOfPlayersSelectionSlider.value == 1)
        {
            TotalNumberOfPlayers = 3;
        }   
    }
}
