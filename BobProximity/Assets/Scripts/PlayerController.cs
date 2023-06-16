using Event = Events.Event;
using Events;
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
    private int _currentPlayer = 0;
    private Vector2Int _cellIndexAtMousePosition;

    [SerializeField] private GameObject coinObj;

    private void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _playerInputActions = new InputActions();
        _playerInputActions.ProximityMap.Enable();

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
            if(_cellIndexAtMousePosition == _gridManager.InvalidCellIndex)
            {
                return;
            }
            
            EventsManager.Invoke(Event.CoinPlaced , _coinValue , _currentPlayer);
            _gridManager.CoinValueData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , _coinValue);
            _gridManager.PlayerIndexData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , _currentPlayer);
            CaptureAdjacentCoin();

            if(!_gridManager.IsCellBlockedData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y))
            {
                Vector2 spawnPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
                GameObject newCoinObj = Instantiate(coinObj , spawnPos , Quaternion.identity , gameObject.transform);
                _gridManager.CoinOnTheCellData.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , newCoinObj);
                SpriteRenderer coinRenderer = newCoinObj.GetComponentInChildren<SpriteRenderer>();
                _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
                newCoinObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();

                switch(_currentPlayer)
                {
                    case 0:
                        coinRenderer.color = Color.red;
                    break;
                
                    case 1:
                        coinRenderer.color = Color.green;
                    break;
                
                    case 2:
                        coinRenderer.color = Color.blue;
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
    
    private void CaptureAdjacentCoin()
    {
        int minX = Mathf.Max(_cellIndexAtMousePosition.x - 1 , 0);
        int maxX = Mathf.Min(_cellIndexAtMousePosition.x + 1 , _gridManager.GridInfo.Cols - 1);
        int minY = Mathf.Max(_cellIndexAtMousePosition.y - 1 , 0);
        int maxY = Mathf.Min(_cellIndexAtMousePosition.y + 1 , _gridManager.GridInfo.Rows - 1);

        int currentPlayerIndex = _gridManager.PlayerIndexData.GetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
        //Debug.Log($"Current Cell ({_cellIndexAtMousePosition.x} , {_cellIndexAtMousePosition.y}) has a coin placed by Player {currentPlayerIndex}");

        for(int x = minX; x <= maxX; x++)
        {
            for(int y = minY; y <= maxY; y++)
            {
                if(x == _cellIndexAtMousePosition.x && y == _cellIndexAtMousePosition.y) continue;

                bool isCellBlocked = _gridManager.IsCellBlockedData.GetValue(x , y);

                if(isCellBlocked)
                {
                    int playerIndexOfAdjacentCoin = _gridManager.PlayerIndexData.GetValue(x , y);
                    int adjacentCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(playerIndexOfAdjacentCoin != currentPlayerIndex && adjacentCoinValue < _coinValue)
                    {
                        //Debug.Log("Adjacent Coin Value : " + adjacentCoinValue.ToString() + " & " + "Current Coin Value : " + _coinValue);
                        _gridManager.PlayerIndexData.SetValue(x , y , currentPlayerIndex);
                        //Debug.Log($"Changed the coin on adjacent Cell ({x} , {y}) to Player {currentPlayerIndex}");

                        UpdateCoinColor(x , y , currentPlayerIndex);
                    }

                    //Debug.Log($"Adjacent Cell ({x} , {y}) has a coin placed by Player {playerIndexOfAdjacentCoin}");
                }
            }
        }
    }

    private void EndPlayerTurn()
    {
        _currentPlayer = (_currentPlayer + 1) % 3;
    }

    private void StartPlayerTurn()
    {
        _coinValue = Random.Range(1 , 21);
        
        UpdateTrailColor();

        if(_mouseTrailObj != null)
        {
            _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
        }
    }
    
    private void UpdateCoinColor(int x , int y , int playerIndex)
    {
        GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);
        //Debug.Log("Name of the Adjacent Coin : " + coin.name);

        if(coin != null)
        {
            SpriteRenderer coinRenderer = coin.GetComponentInChildren<SpriteRenderer>();
            
            switch(playerIndex)
            {
                case 0:
                    coinRenderer.color = Color.red;
                break;
                
                case 1:
                    coinRenderer.color = Color.green;
                break;
                
                case 2:
                    coinRenderer.color = Color.blue;
                break;
                
                default:
                    coinRenderer.color = Color.white;
                break;
            }
        }
    }

    private void UpdateTrailColor()
    {
        if(_mouseTrailObj != null)
        {
            SpriteRenderer trailRenderer = _mouseTrailObj.GetComponentInChildren<SpriteRenderer>();
            Color trailColour = GetPlayerColor(_currentPlayer);
            trailColour.a = 0.5f;
            trailRenderer.color = trailColour;
        }
    }

    private void UpdateTrailVisibility()
    {
        _mouseTrailObj.SetActive(_isMouseMoving);
    }
}