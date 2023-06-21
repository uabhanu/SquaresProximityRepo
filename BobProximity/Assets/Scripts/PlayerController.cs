using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private bool _isMouseMoving;
    private GameObject _mouseTrailObj;
    private GridManager _gridManager;
    private InputActions _playerInputActions;
    private int _coinValue;
    private int _currentPlayerID;
    private List<List<int>> _playerNumbersList;
    private int[] _totalReceivedArray;
    private ScoreManager _scoreManager;
    private Vector2Int _cellIndexAtMousePosition;

    [SerializeField] private GameObject coinObj;

    private void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _playerInputActions = new InputActions();
        _playerInputActions.ProximityMap.Enable();
        _scoreManager = FindObjectOfType<ScoreManager>();
        _totalReceivedArray = new int[3];
        
        int capacity = ((_gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows) / _totalReceivedArray.Length) + 1;
        
        //Debug.Log("Lists Capacity : " + capacity);

        _playerNumbersList = new List<List<int>>();

        for(int i = 0; i < _totalReceivedArray.Length; i++)
        {
            List<int> playerNumbers = new List<int>(capacity);

            for(int j = 1; j <= capacity; j++)
            {
                playerNumbers.Add(j % 20 + 1);
            }

            ShuffleList(playerNumbers);
            _playerNumbersList.Add(playerNumbers);
        }

        StartPlayerTurn();

        _mouseTrailObj = Instantiate(coinObj , Vector3.zero , Quaternion.identity , gameObject.transform);
        UpdateTrailColor();
        
        if(_mouseTrailObj != null)
        {
            _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
        }

        UpdateTrailVisibility();
    }

    private void Update()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.nearClipPlane;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        _cellIndexAtMousePosition = _gridManager.WorldToCell(mouseWorldPos);

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(_cellIndexAtMousePosition == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y))
            {
                //Debug.Log("This is either an invalid cell or the cell is blocked so can't place any coin here :(");
                return;
            }

            _gridManager.CoinValueData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , _coinValue);
            _gridManager.PlayerIndexData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , _currentPlayerID);
            _scoreManager.PlaceCoin(_coinValue , _currentPlayerID);
            

            if(!_gridManager.IsCellBlockedData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y))
            {
                BuffUpAdjacentCoin();
                CaptureAdjacentCoin();
                
                Vector2 spawnPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
                GameObject newCoinObj = Instantiate(coinObj , spawnPos , Quaternion.identity , gameObject.transform);
                _gridManager.CoinOnTheCellData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , newCoinObj);
                SpriteRenderer coinRenderer = newCoinObj.GetComponentInChildren<SpriteRenderer>();
                TMP_Text coinValueTMP = newCoinObj.GetComponentInChildren<TMP_Text>();
                _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
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

                _gridManager.IsCellBlockedData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , true);
                _isMouseMoving = false;
                UpdateTrailVisibility();
                EndPlayerTurn();
                StartPlayerTurn();
            }
        }
        else
        {
            if(_isMouseMoving)
            {
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
            else
            {
                if(_cellIndexAtMousePosition != _gridManager.InvalidCellIndex)
                {
                    Vector2 snapPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
                    _mouseTrailObj.transform.position = snapPos;
                }
                else
                {
                    _mouseTrailObj.transform.position = mouseWorldPos;
                }
            }

            if(!_isMouseMoving && Mouse.current.delta.ReadValue() != Vector2.zero)
            {
                _isMouseMoving = true;
                UpdateTrailVisibility();
            }
        }
    }

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
                    int adjacentCoinPlayerID = _gridManager.PlayerIndexData.GetValue(x , y);
                    int adjacentCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(adjacentCoinPlayerID == _currentPlayerID)
                    {
                        int newAdjacentCoinValue = adjacentCoinValue + 1;
                        _gridManager.CoinValueData.SetValue(x , y , newAdjacentCoinValue);
                        GameObject adjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(x , y);
                        TMP_Text adjacentCoinValueText = adjacentCoinObj.GetComponentInChildren<TMP_Text>();
                        adjacentCoinValueText.text = _gridManager.CoinValueData.GetValue(x , y).ToString();
                        _scoreManager.BuffUpCoin(adjacentCoinPlayerID , newAdjacentCoinValue - adjacentCoinValue);
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
                    int adjacentCoinPlayerID = _gridManager.PlayerIndexData.GetValue(x , y);
                    int adjacentCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(adjacentCoinPlayerID != _currentPlayerID && adjacentCoinValue < _coinValue)
                    {
                        //Debug.Log("Adjacent Coin Value : " + adjacentCoinValue.ToString() + " & " + "Current Coin Value : " + _coinValue);
                        _gridManager.PlayerIndexData.SetValue(x , y , _currentPlayerID);
                        _scoreManager.CaptureCoin(_currentPlayerID , adjacentCoinPlayerID , adjacentCoinValue);
                        //Debug.Log($"Changed the coin on adjacent Cell ({x} , {y}) to Player {currentPlayerIndex}");

                        UpdateCoinColor(x , y , _currentPlayerID);
                    }

                    //Debug.Log($"Adjacent Cell ({x} , {y}) has a coin placed by Player {playerIndexOfAdjacentCoin}");
                }
            }
        }
    }

    private void EndPlayerTurn()
    {
        _currentPlayerID = (_currentPlayerID + 1) % 3;
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
        if(_currentPlayerID >= _playerNumbersList.Count || _currentPlayerID < 0 || _playerNumbersList[_currentPlayerID].Count == 0)
        {
            _coinValue = 1;
        }
        else
        {
            _coinValue = _playerNumbersList[_currentPlayerID][0];
            _playerNumbersList[_currentPlayerID].RemoveAt(0);
        }
        
        for(int i = 0; i < _totalReceivedArray.Length; i++)
        {
            if(_currentPlayerID == i)
            {
                 _totalReceivedArray[i] += _coinValue;
            }
        }

        if (_mouseTrailObj != null)
        {
            _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
        }

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
                    coinValueTMP.color = Color.black;
                break;
            }
        }
    }

    private void UpdateTrailColor()
    {
        if(_mouseTrailObj != null)
        {
            SpriteRenderer trailRenderer = _mouseTrailObj.GetComponentInChildren<SpriteRenderer>();
            TMP_Text trailText = _mouseTrailObj.GetComponentInChildren<TMP_Text>();

            Color playerColor = GetPlayerColor(_currentPlayerID);
            playerColor.a = 0.5f;

            trailRenderer.color = playerColor;

            switch(_currentPlayerID)
            {
                case 0:
                    trailText.color = Color.yellow;
                break;

                case 1:
                    trailText.color = Color.blue;
                break;

                case 2:
                    trailText.color = Color.cyan;
                break;

                default:
                    trailText.color = Color.black;
                break;
            }
        }
    }

    private void UpdateTrailVisibility()
    {
        _mouseTrailObj.SetActive(_isMouseMoving);
    }
}