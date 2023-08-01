using Event = Events.Event;
using Events;
using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    # region Variables Declarations
    
    private bool _isGameStarted;
    private bool _isMouseMoving;
    private bool _isRandomTurns;
    private bool[] _isAIArray;
    private GameObject _coinUIObj;
    private GameObject _mouseTrailObj;
    private GridManager _gridManager;
    private List<int> _lesserCoinValuesList;
    private List<int> _otherPlayerCoinValuesList;
    private List<int> _selfCoinValuesList;
    private List<Vector2Int> _otherPlayerCoinsCellIndicesList;
    private List<Vector2Int> _lesserCoinsCellIndicesList;
    private List<Vector2Int> _selfCoinsCellIndicesList;
    private List<Vector2Int> _unblockedCellIndicesList;
    private InputActions _playerInputActions;
    private int _coinValue;
    private int _currentPlayerID;
    private int _numberOfPlayers;
    private int _playersListsCapacity;
    private int[] _totalReceivedArray;
    private List<int> _playersRemaining;
    private List<List<int>> _playerNumbersList;
    private Vector2Int _cellIndexAtMousePosition;

    [SerializeField] private float aiCoinPlaceDelay;
    [SerializeField] private GameObject coinObj;
    [SerializeField] private GameObject trailObj;
    
    #endregion

    #region MonoBehaviour Functions
    
    private void Start()
    {
        _coinUIObj = GameObject.Find("CoinUI");
        _playerInputActions = new InputActions();
        _playerInputActions.MobileMap.Enable();
        _playerInputActions.PCMap.Enable();

        _mouseTrailObj = Instantiate(trailObj , Vector3.zero , Quaternion.identity , gameObject.transform);

        SubscribeToEvents();
        UpdateTrailColor();
        UpdateTrailVisibility();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if(!_isGameStarted) return;
        
        if(_gridManager.TotalCells == 0)
        {
            EventsManager.Invoke(Event.GameOver);
            EventsManager.Invoke(Event.PlayerTotalReceived , _totalReceivedArray);
        }
    }
    
    #endregion

    #region AI Functions

    private IEnumerator AIPlaceCoinCoroutine()
    {
        yield return new WaitForSeconds(aiCoinPlaceDelay);
        PlaceCoin();
    }
    
    private IEnumerator AnimateCoinEffect(SpriteRenderer coinRenderer)
    {
        Color originalColor = coinRenderer.color;
        float originalAlpha = originalColor.a;
        float fadeDuration = 0.2f;
        float fadeInterval = 0.05f;
        int fadeSteps = Mathf.RoundToInt(fadeDuration / fadeInterval);
        int fadeCycles = 3;

        for(int cycle = 0; cycle < fadeCycles; cycle++)
        {
            for(int i = 0; i < fadeSteps; i++)
            {
                float t = (float)i / fadeSteps;
                float alpha = Mathf.Lerp(originalAlpha , 0f , t);

                Color newColor = new Color(originalColor.r , originalColor.g , originalColor.b , alpha);
                coinRenderer.color = newColor;

                yield return new WaitForSeconds(fadeInterval);
            }

            yield return new WaitForSeconds(0.1f);

            for(int i = 0; i < fadeSteps; i++)
            {
                float t = (float)i / fadeSteps;
                float alpha = Mathf.Lerp(0f , originalAlpha , t);

                Color newColor = new Color(originalColor.r , originalColor.g , originalColor.b , alpha);
                coinRenderer.color = newColor;

                yield return new WaitForSeconds(fadeInterval);
            }

            yield return new WaitForSeconds(0.1f);
        }

        coinRenderer.color = new Color(originalColor.r , originalColor.g , originalColor.b , originalAlpha);
    }

    private List<Vector2Int> GetAdjacentCellIndices(Vector2Int coinCellIndex)
    {
        List<Vector2Int> adjacentCellIndicesList = new List<Vector2Int>
        {
            new(coinCellIndex.x - 1 , coinCellIndex.y),
            new(coinCellIndex.x + 1 , coinCellIndex.y),
            new(coinCellIndex.x - 1 , coinCellIndex.y + 1),
            new(coinCellIndex.x + 1 , coinCellIndex.y + 1),
            new(coinCellIndex.x , coinCellIndex.y - 1),
            new(coinCellIndex.x , coinCellIndex.y + 1),
            new(coinCellIndex.x - 1 , coinCellIndex.y - 1),
            new(coinCellIndex.x + 1 , coinCellIndex.y - 1)
        };

        return adjacentCellIndicesList;
    }

    private Vector2Int FindBestAdjacentCell(List<int> coinValuesList)
    {
        List<Vector2Int> highestValueCoinCellIndicesList = new List<Vector2Int>();

        if(_lesserCoinValuesList.Count > 0)
        {
            highestValueCoinCellIndicesList = _lesserCoinsCellIndicesList
            .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValuesList[0])
            .ToList();
        }

        if(_selfCoinValuesList.Count > 0)
        {
            List<Vector2Int> selfCoinIndices = _selfCoinsCellIndicesList
            .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValuesList[0])
            .ToList();

            highestValueCoinCellIndicesList.AddRange(selfCoinIndices);
        }

        foreach(Vector2Int coinCellIndex in highestValueCoinCellIndicesList)
        {
            //Todo Even though we are checking if a cell is blocked, it seems that the cell that has the coin already also added to the list. You may have to set Cell Block Data to true when the coin is placed  
            List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndices(coinCellIndex);
            adjacentCellIndicesList = adjacentCellIndicesList
            .Where(adjacentCellIndex => adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
            adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
            !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
            .ToList();

            //Debug.Log("Adjacent Cells available for coin at " + coinCellIndex + " : " + string.Join(" , " , adjacentCellIndicesList));

            if(adjacentCellIndicesList.Count > 0)
            {
                int randomIndex = Random.Range(0, adjacentCellIndicesList.Count);
                //Debug.Log("FindBestAdjacentCell() -> Selected Cell Index : " + adjacentCellIndicesList[randomIndex]);
                return adjacentCellIndicesList[randomIndex];
            }
        }

        return _gridManager.InvalidCellIndex;
    }

    private Vector2Int FindCellToPlaceCoinOn()
    {
        ClearLists();
        PopulateLists();

        Vector2Int targetCellIndex = _gridManager.InvalidCellIndex;
        
        if(_lesserCoinValuesList.Count > 0)
        {
            //Debug.Log("Attack");
            
            _lesserCoinValuesList.Sort((a, b) => b.CompareTo(a));
            targetCellIndex = FindBestAdjacentCell(_lesserCoinValuesList);
            
            //Debug.Log("FindCellToPlaceCoinOn() Attack Block -> Target Cell Index : " + targetCellIndex);
    
            if(targetCellIndex != _gridManager.InvalidCellIndex)
            {
                return targetCellIndex;
            }
        }

        if(_selfCoinValuesList.Count > 0)
        {
            //Debug.Log("Buff Up");
            
            _selfCoinValuesList.Sort((a, b) => b.CompareTo(a));
            targetCellIndex = FindBestAdjacentCell(_selfCoinValuesList);
            
            //Debug.Log("FindCellToPlaceCoinOn() Buff Up Block -> Target Cell Index : " + targetCellIndex);
            
            if(targetCellIndex != _gridManager.InvalidCellIndex)
            {
                return targetCellIndex;
            }
        }

        if(_unblockedCellIndicesList.Count > 0)
        {
            //Debug.Log("Random cell");
            
            int index = Random.Range(0 , _unblockedCellIndicesList.Count);
            targetCellIndex = _unblockedCellIndicesList[index];
            
            //Debug.Log("FindCellToPlaceCoinOn() Random Block -> Target Cell Index : " + targetCellIndex);
            
            if(targetCellIndex != _gridManager.InvalidCellIndex)
            {
                return targetCellIndex;
            }
        }

        return _gridManager.InvalidCellIndex;
    }
    
    private void ClearLists()
    {
        _lesserCoinsCellIndicesList.Clear();
        _lesserCoinValuesList.Clear();
        _selfCoinsCellIndicesList.Clear();
        _selfCoinValuesList.Clear();
        _otherPlayerCoinsCellIndicesList.Clear();
        _otherPlayerCoinValuesList.Clear();
        _unblockedCellIndicesList.Clear();
    }
    
    private void PopulateLists()
    {
        for(int x = 0; x < _gridManager.GridInfo.Cols; x++)
        {
            for(int y = 0; y < _gridManager.GridInfo.Rows; y++)
            {
                if(!_gridManager.IsCellBlockedData.GetValue(x , y))
                {
                    _unblockedCellIndicesList.Add(new Vector2Int(x , y));
                }
                
                else if(_gridManager.IsCellBlockedData.GetValue(x , y))
                {
                    GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);

                    if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) != _currentPlayerID)
                    {
                        _otherPlayerCoinsCellIndicesList.Add(new Vector2Int(x , y));
                        int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                        _otherPlayerCoinValuesList.Add(coinValue);

                        if(coinValue < _coinValue)
                        {
                            _lesserCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            _lesserCoinValuesList.Add(coinValue);
                        }
                    }

                    if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) == _currentPlayerID)
                    {
                        _selfCoinsCellIndicesList.Add(new Vector2Int(x , y));
                        int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                        _selfCoinValuesList.Add(coinValue);
                    }
                }
            }
        }
    }
    
    #endregion

    #region Other Custom Functions
    
    private Color GetPlayerColor(int playerIndex)
    {
        switch(playerIndex)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            default: return Color.white;
        }
    }
    
    private void BuffUpAdjacentCoin()
    {
        int minX = Mathf.Max(_cellIndexAtMousePosition.x - 1 , 0);
        int maxX = Mathf.Min(_cellIndexAtMousePosition.x + 1 , _gridManager.GridInfo.Cols - 1);
        int minY = Mathf.Max(_cellIndexAtMousePosition.y - 1 , 0);
        int maxY = Mathf.Min(_cellIndexAtMousePosition.y + 1 , _gridManager.GridInfo.Rows - 1);

        _currentPlayerID = _gridManager.PlayerIndexData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);

        for(int x = minX; x <= maxX; x++)
        {
            for(int y = minY; y <= maxY; y++)
            {
                if(x == _cellIndexAtMousePosition.x && y == _cellIndexAtMousePosition.y) continue;

                bool isCellBlocked = _gridManager.IsCellBlockedData.GetValue(x , y);

                if(isCellBlocked)
                {
                    int adjacentPlayerID = _gridManager.PlayerIndexData.GetValue(x , y);
                    int adjacentCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(adjacentPlayerID == _currentPlayerID)
                    {
                        int newAdjacentCoinValue = adjacentCoinValue + 1;
                        _gridManager.CoinValueData.SetValue(x , y , newAdjacentCoinValue);
                        
                        GameObject adjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(x , y);
                        TMP_Text adjacentCoinValueText = null;

                        if(adjacentCoinObj != null)
                        {
                            adjacentCoinValueText = adjacentCoinObj.GetComponentInChildren<TMP_Text>();

                            if(adjacentCoinValueText  == null)
                            {
                                int[] offsetX = { -1 , 0 , 1 , -1 , 1 , -1 , 0 , 1 };
                                int[] offsetY = { -1 , -1 , -1 , 0 , 0 , 1 , 1 , 1 };

                                for(int i = 0; i < 8; i++)
                                {
                                    int adjacentCellIndexX = x + offsetX[i];
                                    int adjacentCellIndexY = y + offsetY[i];
                                    
                                    GameObject adjacentAdjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(adjacentCellIndexX , adjacentCellIndexY);
                                    
                                    if(adjacentAdjacentCoinObj != null)
                                    {
                                        TMP_Text adjacentAdjacentCoinValueText = adjacentAdjacentCoinObj.GetComponentInChildren<TMP_Text>();
                                        
                                        if(adjacentAdjacentCoinValueText != null)
                                        {
                                            adjacentCoinValueText = adjacentAdjacentCoinValueText;
                                            break;
                                        }
                                    }
                                }
                            }

                            if(adjacentCoinValueText != null)
                            {
                                adjacentCoinValueText.text = _gridManager.CoinValueData.GetValue(x , y).ToString();
                                EventsManager.Invoke(Event.CoinBuffedUp , adjacentPlayerID , newAdjacentCoinValue - adjacentCoinValue);   
                            }
                            else
                            {
                                Debug.LogWarning("Could not find adjacent cell with TMP_Text component.");
                            }
                        }
                    }
                }
            }
        }
    }

    private void CaptureAdjacentCoin()
    {
        int minX = Mathf.Max(_cellIndexAtMousePosition.x - 1 , 0);
        int maxX = Mathf.Min(_cellIndexAtMousePosition.x + 1 , _gridManager.GridInfo.Cols - 1);
        int minY = Mathf.Max(_cellIndexAtMousePosition.y - 1 , 0);
        int maxY = Mathf.Min(_cellIndexAtMousePosition.y + 1 , _gridManager.GridInfo.Rows - 1);

        _currentPlayerID = _gridManager.PlayerIndexData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
        //Debug.Log($"Current Cell ({_cellIndexAtMousePosition.x} , {_cellIndexAtMousePosition.y}) has a coin placed by Player {currentPlayerIndex}");

        for(int x = minX; x <= maxX; x++)
        {
            for(int y = minY; y <= maxY; y++)
            {
                if(x == _cellIndexAtMousePosition.x && y == _cellIndexAtMousePosition.y) continue;

                bool isCellBlocked = _gridManager.IsCellBlockedData.GetValue(x , y);

                if(isCellBlocked)
                {
                    GameObject adjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(x , y);
                    
                    if(adjacentCoinObj != null)
                    {
                        int adjacentPlayerID = _gridManager.PlayerIndexData.GetValue(x , y);
                        int adjacentPlayerCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                        //Debug.Log($"Adjacent Cell ({x}, {y}) has a coin placed by Player {adjacentPlayerID}. Coin Value: {adjacentPlayerCoinValue}");

                        if(adjacentPlayerID != _currentPlayerID && adjacentPlayerCoinValue < _coinValue)
                        {
                            _gridManager.PlayerIndexData.SetValue(x , y , _currentPlayerID);
                            EventsManager.Invoke(Event.CoinCaptured , _currentPlayerID , adjacentPlayerID , adjacentPlayerCoinValue);
                            //Debug.Log($"Changed the coin on adjacent Cell ({x}, {y}) to Player {_currentPlayerID}");
                            UpdateCoinColor(x , y , _currentPlayerID);
                        }
                    }
                }
            }
        }
    }

    private void EndPlayerTurn()
    {
        _currentPlayerID = (_currentPlayerID + 1) % _numberOfPlayers;
    }

    private void PlaceCoin()
    {
        //Debug.Log("PlaceCoin called for Player ID: " + _currentPlayerID);
        
        _gridManager.CoinValueData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , _coinValue);
        _gridManager.PlayerIndexData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , _currentPlayerID);
        _gridManager.TotalCells--;
        
        //Debug.Log("Total Cells : " + _gridManager.TotalCells--);

        EventsManager.Invoke(Event.CoinPlaced , _coinValue , _currentPlayerID);

        if(!_gridManager.IsCellBlockedData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y))
        {
            BuffUpAdjacentCoin();
            CaptureAdjacentCoin();

            Vector2 spawnPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
            GameObject newCoinObj = Instantiate(coinObj , spawnPos , Quaternion.identity , gameObject.transform);
            _gridManager.CoinOnTheCellData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , newCoinObj);
            SpriteRenderer coinRenderer = newCoinObj.GetComponentInChildren<SpriteRenderer>();
            TMP_Text coinValueTMP = newCoinObj.GetComponentInChildren<TMP_Text>();
            newCoinObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();

            switch(_currentPlayerID)
            {
                case 0:
                    coinRenderer.color = Color.red;
                    coinValueTMP.color = Color.yellow;
                break;

                case 1:
                    coinRenderer.color = Color.green;
                    coinValueTMP.color = Color.blue;
                break;

                case 2:
                    coinRenderer.color = Color.blue;
                    coinValueTMP.color = Color.cyan;
                break;

                default:
                    coinRenderer.color = Color.white;
                    coinValueTMP.color = Color.black;
                break;
            }
            
            for(int i = 0; i < _isAIArray.Length; i++)
            {
                if(_isAIArray[i] && i == _currentPlayerID)
                {
                    StartCoroutine(AnimateCoinEffect(newCoinObj.GetComponentInChildren<SpriteRenderer>()));       
                }
            }
            
            _gridManager.IsCellBlockedData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , true);
            _isMouseMoving = false;
            UpdateTrailVisibility();
            EndPlayerTurn();
            StartPlayerTurn();
        }
    }

    private void ResetPlayersRemaining()
    {
        _playersRemaining.Clear();

        for(int i = 0; i < _numberOfPlayers; i++)
        {
            _playersRemaining.Add(i);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        
        while(n > 1)
        {
            n--;
            int k = Random.Range(0 , n + 1);
            (list[k] , list[n]) = (list[n] , list[k]);
        }
    }

    private void StartPlayerTurn()
    {
        if(_isRandomTurns)
        {
            int remainingPlayersCount = _playersRemaining.Count;
            int randomIndex = Random.Range(0 , remainingPlayersCount);
            _currentPlayerID = _playersRemaining[randomIndex];
            _playersRemaining.RemoveAt(randomIndex);
        }

        bool foundUnblockedCell = false;
        int maxIterations = _gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows;
        int currentIteration = 0;

        if(_playerNumbersList[_currentPlayerID].Count > 0)
        {
            _coinValue = _playerNumbersList[_currentPlayerID][0];

            TMP_Text coinUITMP = _coinUIObj.GetComponentInChildren<TMP_Text>();
            coinUITMP.text = _coinValue.ToString();

            for(int i = 0; i < _totalReceivedArray.Length; i++)
            {
                if(_currentPlayerID == i)
                {
                    _totalReceivedArray[i] += _coinValue;
                }
            }

            _playerNumbersList[_currentPlayerID].RemoveAt(0);
        }

        if(_playersRemaining.Count == 0)
        {
            ResetPlayersRemaining();
        }

        while(!foundUnblockedCell && currentIteration < maxIterations)
        {
            for(int i = 0; i < _isAIArray.Length; i++)
            {
                if(_isAIArray[i] && _currentPlayerID == i)
                {
                    _cellIndexAtMousePosition = FindCellToPlaceCoinOn();

                    if(_cellIndexAtMousePosition != _gridManager.InvalidCellIndex)
                    {
                        StartCoroutine(AIPlaceCoinCoroutine());
                        foundUnblockedCell = true;
                    }
                }
            }

            currentIteration++;
        }

        UpdateCoinUIImageColors();
        UpdateTrailColor();
    }

    private void UpdateCoinColor(int x , int y , int playerIndex)
    {
        GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);
        //Debug.Log("Name of the Adjacent Coin: " + coin.name);

        if(coin != null)
        {
            SpriteRenderer coinRenderer = coin.GetComponentInChildren<SpriteRenderer>();
            TMP_Text coinValueTMP = coin.GetComponentInChildren<TMP_Text>();

            switch(playerIndex)
            {
                case 0:
                    coinRenderer.color = Color.red;
                    coinValueTMP.color = Color.yellow;
                break;

                case 1:
                    coinRenderer.color = Color.green;
                    coinValueTMP.color = Color.blue;
                break;

                case 2:
                    coinRenderer.color = Color.blue;
                    coinValueTMP.color = Color.cyan;
                break;

                default:
                    coinRenderer.color = Color.white;
                break;
            }
        }
    }
    
    private void UpdateCoinUIImageColors()
    {
        if(_coinUIObj != null)
        {
            Color playerColor = GetPlayerColor(_currentPlayerID);
            Image coinUIImage = _coinUIObj.GetComponent<Image>();
            coinUIImage.color = playerColor;
            
            TMP_Text coinUIText = _coinUIObj.GetComponentInChildren<TMP_Text>();
        
            switch(_currentPlayerID)
            {
                case 0:
                    coinUIText.color = Color.yellow;
                break;

                case 1:
                    coinUIText.color = Color.blue;
                break;

                case 2:
                    coinUIText.color = Color.cyan;
                break;

                default:
                    coinUIText.color = Color.black;
                break;
            }
        }
    }

    private void UpdateTrailColor()
    {
        if(_mouseTrailObj != null)
        {
            SpriteRenderer trailRenderer = _mouseTrailObj.GetComponentInChildren<SpriteRenderer>();
            Color playerColor = GetPlayerColor(_currentPlayerID);

            playerColor.a *= 0.5f;
            trailRenderer.color = playerColor;
        }
    }

    private void UpdateTrailVisibility()
    {
        _mouseTrailObj.SetActive(_isMouseMoving);
    }
    
    #endregion
    
    #region Events Related Functions

    private void OnAIHumanToggled(int playerID , bool isAI)
    {
        _isAIArray[playerID] = isAI;
    }

    private void OnGameOver()
    {
        _isGameStarted = false;
    }

    private void OnGamePaused()
    {
        _isGameStarted = false;
    }
    
    private void OnGameResumed()
    {
        _isGameStarted = true;
    }
    
    private void OnGameStarted()
    {
        _isGameStarted = true;

        _gridManager = FindObjectOfType<GridManager>();

        _playersListsCapacity = _gridManager.TotalCells / _numberOfPlayers;

        _selfCoinsCellIndicesList = new List<Vector2Int>();
        _selfCoinValuesList = new List<int>();
        _lesserCoinsCellIndicesList = new List<Vector2Int>();
        _lesserCoinValuesList = new List<int>();
        _otherPlayerCoinsCellIndicesList = new List<Vector2Int>();
        _otherPlayerCoinValuesList = new List<int>();
        _unblockedCellIndicesList = new List<Vector2Int>();
        _playerNumbersList = new List<List<int>>();
        _playersRemaining = new List<int>();

        for(int i = 0; i < _numberOfPlayers; i++)
        {
            _playersRemaining.Add(i);
        }

        for(int i = 0; i < _numberOfPlayers; i++)
        {
            List<int> randomNumbers = new List<int>(_playersListsCapacity);

            for(int j = 0; j < _playersListsCapacity; j++)
            {
                int randomValue = Random.Range(1 , 21);
                randomNumbers.Add(randomValue);
            }

            for(i = 0; i < _numberOfPlayers; i++)
            {
                ShuffleList(randomNumbers);
                _playerNumbersList.Add(new List<int>(randomNumbers));
            }
        }
        
        StartPlayerTurn();
    }

    private void OnMouseLeftClicked()
    {
        if(!_isGameStarted) return;

        for(int i = 0; i < _isAIArray.Length; i++)
        {
            if(_isAIArray[i] && _currentPlayerID == i)
            {
                return;
            }
        }

        if(_cellIndexAtMousePosition == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y))
        {
            return;
        }

        PlaceCoin();
    }

    private void OnMouseMoved()
    {
        if(!_isGameStarted) return;
        
        for(int i = 0; i < _isAIArray.Length; i++)
        {
            if(_isAIArray[i] && _currentPlayerID == i)
            {
                return;
            }
        }

        _isMouseMoving = true;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.nearClipPlane;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        _cellIndexAtMousePosition = _gridManager.WorldToCell(mouseWorldPos);
        
        if(_cellIndexAtMousePosition != _gridManager.InvalidCellIndex)
        {
            Vector2 snapPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
            _mouseTrailObj.transform.position = snapPos;
        }
        else
        {
            _mouseTrailObj.transform.position = mouseWorldPos;
        }

        UpdateTrailVisibility();
    }

    private void OnNumberOfPlayersSelected(int numberOfPlayers)
    {
        _numberOfPlayers = numberOfPlayers;
        _isAIArray = new bool[_numberOfPlayers];
        _totalReceivedArray = new int[_numberOfPlayers];
    }

    private void OnRandomTurnsToggled()
    {
        _isRandomTurns = !_isRandomTurns;
    }
    
    private void OnTouchscreenTapped()
    {
        if(!_isGameStarted) return;

        if(_cellIndexAtMousePosition == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y))
        {
            return;
        }

        PlaceCoin();
    }
    
    private void SubscribeToEvents()
    {
        EventsManager.SubscribeToEvent(Event.AIHumanToggled , (Action<int , bool>)OnAIHumanToggled);
        EventsManager.SubscribeToEvent(Event.GameOver , new Action(OnGameOver));
        EventsManager.SubscribeToEvent(Event.GamePaused , new Action(OnGamePaused));
        EventsManager.SubscribeToEvent(Event.GameResumed , new Action(OnGameResumed));
        EventsManager.SubscribeToEvent(Event.GameStarted , new Action(OnGameStarted));
        EventsManager.SubscribeToEvent(Event.MouseLeftClicked , new Action(OnMouseLeftClicked));
        EventsManager.SubscribeToEvent(Event.MouseMoved , new Action(OnMouseMoved));
        EventsManager.SubscribeToEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
        EventsManager.SubscribeToEvent(Event.RandomTurnsToggled , new Action(OnRandomTurnsToggled));
        EventsManager.SubscribeToEvent(Event.TouchscreenTapped , new Action(OnTouchscreenTapped));
    }
    
    private void UnsubscribeFromEvents()
    {
        EventsManager.UnsubscribeFromEvent(Event.AIHumanToggled , (Action<int , bool>)OnAIHumanToggled);
        EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
        EventsManager.UnsubscribeFromEvent(Event.GamePaused , new Action(OnGamePaused));
        EventsManager.UnsubscribeFromEvent(Event.GameResumed , new Action(OnGameResumed));
        EventsManager.UnsubscribeFromEvent(Event.GameStarted , new Action(OnGameStarted));
        EventsManager.UnsubscribeFromEvent(Event.MouseLeftClicked , new Action(OnMouseLeftClicked));
        EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
        EventsManager.UnsubscribeFromEvent(Event.RandomTurnsToggled , new Action(OnRandomTurnsToggled));
        EventsManager.UnsubscribeFromEvent(Event.TouchscreenTapped , new Action(OnTouchscreenTapped));
    }
    
    #endregion
}
