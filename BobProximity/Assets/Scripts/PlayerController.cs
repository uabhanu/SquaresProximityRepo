using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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
        SpriteRenderer trailRenderer = _mouseTrailObj.GetComponentInChildren<SpriteRenderer>();
        Color trailColor = GetPlayerColor(_currentPlayer);
        trailColor.a = 0.5f;
        trailRenderer.color = trailColor;
        
        if(_mouseTrailObj != null)
        {
            _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
        }
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

            if(!_gridManager.IsCellBlocked.GetValue(_cellIndexAtMousePosition.x, _cellIndexAtMousePosition.y))
            {
                Vector2 spawnPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x, _cellIndexAtMousePosition.y);
                GameObject newCoinObj = Instantiate(coinObj, spawnPos, Quaternion.identity, gameObject.transform);
                SpriteRenderer coinRenderer = newCoinObj.GetComponentInChildren<SpriteRenderer>();
                _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
                newCoinObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();

                switch (_currentPlayer)
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

                _gridManager.IsCellBlocked.SetValue(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y , true);
                _mouseTrailObj.SetActive(false);
                EndPlayerTurn();
                StartPlayerTurn();
            }
        }
        else
        {
            if(_mouseTrailObj == null)
            {
                _mouseTrailObj = Instantiate(coinObj , Vector3.zero , Quaternion.identity , gameObject.transform);
                SpriteRenderer trailRenderer = _mouseTrailObj.GetComponentInChildren<SpriteRenderer>();
                Color trailColor = GetPlayerColor(_currentPlayer);
                trailColor.a = 0.5f;
                trailRenderer.color = trailColor;
            }

            if(_cellIndexAtMousePosition != _gridManager.InvalidCellIndex)
            {
                Vector2 snapPos = _gridManager.CellToWorld(_cellIndexAtMousePosition.x , _cellIndexAtMousePosition.y);
                _mouseTrailObj.transform.position = snapPos;
            }
            else
            {
                _mouseTrailObj.transform.position = mouseWorldPos;
            }

            _mouseTrailObj.SetActive(true);
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

    private void EndPlayerTurn()
    {
        //Debug.Log("Player " + (_currentPlayer + 1) + "'s Turn Ended");
        _currentPlayer = (_currentPlayer + 1) % 3;
    }

    private void StartPlayerTurn()
    {
        _coinValue = Random.Range(1 , 21);
        
        //Debug.Log("Player " + (_currentPlayer + 1) + "'s Turn");
        
        if(_mouseTrailObj != null)
        {
            _mouseTrailObj.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
        }
    }
}