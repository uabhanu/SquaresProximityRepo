using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Event = Events.Event;
using Random = UnityEngine.Random;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Constructor
    
        public GameManager(IPlayerTurnsManager iPlayerTurnsManager)
        {
            _iPlayerTurnsManager = iPlayerTurnsManager;
        }
    
        #endregion
    
        #region Interfaces Declarations

        private IPlayerTurnsManager _iPlayerTurnsManager;

        #endregion
    
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
        private List<int> _playersRemainingList;
        private List<List<int>> _playerNumbersList;
        private Vector2Int _cellIndexAtMousePosition;

        [SerializeField] private bool isTestingMode;
        [SerializeField] private float aiCoinPlaceDelay;
        [SerializeField] private GameObject coinObj;
        [SerializeField] private GameObject trailObj;

        public bool[] IsAIArray => _isAIArray;
        public bool IsRandomTurns => _isRandomTurns;
        public GameObject CoinUIObj => _coinUIObj;
        public GameObject MouseTrailObj => _mouseTrailObj;
        public int NumberOfPlayers => _numberOfPlayers;
        public List<List<int>> PlayerNumbersList => _playerNumbersList;
        public List<int> PlayersRemainingList => _playersRemainingList;

        public int CoinValue
        {
            get => _coinValue;
            set => _coinValue = value;
        }
    
        public int CurrentPlayerID
        {
            get => _currentPlayerID;
            set => _currentPlayerID = value;
        }
    
        public int[] TotalReceivedArray
        {
            get => _totalReceivedArray;
            set => _totalReceivedArray = value;
        }

        public Vector2Int CellIndexAtMousePosition
        {
            get => _cellIndexAtMousePosition;
            set => _cellIndexAtMousePosition = value;
        }

        #endregion

        #region MonoBehaviour Functions
    
        private void Start()
        {
            _coinUIObj = GameObject.Find("CoinUI");
            _playerInputActions = new InputActions();
            _playerInputActions.MobileMap.Enable();
            _playerInputActions.PCMap.Enable();
        
            _mouseTrailObj = Instantiate(trailObj , Vector3.zero , Quaternion.identity , gameObject.transform);

            ToggleEventSubscription(true);
            UpdateTrailVisibility();
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }

        private void Update()
        {
            if(!_isGameStarted) return;
        
            if(_gridManager.TotalCells == 0)
            {
                EventsManager.Invoke(Event.GameOver);
                EventsManager.Invoke(Event.PlayerTotalReceived , TotalReceivedArray);
            }
        }
    
        #endregion

        #region AI Functions

        public IEnumerator AIPlaceCoinCoroutine()
        {
            yield return new WaitForSeconds(aiCoinPlaceDelay);
            PlaceCoin();
        }
    
        public IEnumerator AnimateCoinEffect(SpriteRenderer coinRenderer , Color? capturingColor = null)
        {
            Color originalColor = coinRenderer.color;
            float originalAlpha = originalColor.a;
            float capturingAlpha = capturingColor.HasValue ? capturingColor.Value.a : 0f;

            float fadeDuration = 0.2f;
            float fadeInterval = 0.05f;
            int fadeSteps = Mathf.RoundToInt(fadeDuration / fadeInterval);
            int fadeCycles = 3;

            for(int cycle = 0; cycle < fadeCycles; cycle++)
            {
                for(int i = 0; i < fadeSteps; i++)
                {
                    float t = (float)i / fadeSteps;
                    float alpha = Mathf.Lerp(originalAlpha , capturingAlpha, t);

                    Color newColor = new Color(originalColor.r , originalColor.g , originalColor.b , alpha);
                    coinRenderer.color = newColor;

                    yield return new WaitForSeconds(fadeInterval);
                }

                yield return new WaitForSeconds(0.1f);

                for(int i = 0; i < fadeSteps; i++)
                {
                    float t = (float)i / fadeSteps;
                    float alpha = Mathf.Lerp(capturingAlpha , originalAlpha , t);

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
    
        private void ListAdjacentCellsForLesserCoins()
        {
            if(_lesserCoinValuesList.Count > 0)
            {
                bool foundCoinWithUnblockedCells = false;

                foreach(int coinValue in _lesserCoinValuesList.OrderByDescending(x => x))
                {
                    if(foundCoinWithUnblockedCells) break;

                    List<Vector2Int> highestValueCoinCellIndicesList = _lesserCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue).ToList();

                    foreach(Vector2Int coinCellIndex in highestValueCoinCellIndicesList)
                    {
                        List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndices(coinCellIndex);

                        adjacentCellIndicesList = adjacentCellIndicesList
                            .Where(adjacentCellIndex => adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
                                                        adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
                                                        !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                            .ToList();

                        if(adjacentCellIndicesList.Count > 0)
                        {
                            Debug.Log("Lesser Coins Highest Value: " + coinValue + " , Coin Cell Index: " + coinCellIndex + " , Unblocked Adjacent Cells: " + string.Join(" , " , adjacentCellIndicesList));
                            foundCoinWithUnblockedCells = true;
                            break;
                        }
                    }
                }
            }
        }
    
        private void ListAdjacentCellsForSelfCoins()
        {
            if(_selfCoinValuesList.Count > 0)
            {
                bool foundCoinWithUnblockedCells = false;

                foreach(int coinValue in _selfCoinValuesList.OrderByDescending(x => x))
                {
                    if(foundCoinWithUnblockedCells) break;

                    List<Vector2Int> highestValueCoinCellIndicesList = _selfCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue).ToList();

                    foreach(Vector2Int coinCellIndex in highestValueCoinCellIndicesList)
                    {
                        List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndices(coinCellIndex);

                        adjacentCellIndicesList = adjacentCellIndicesList
                            .Where(adjacentCellIndex => adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
                                                        adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
                                                        !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                            .ToList();

                        if(adjacentCellIndicesList.Count > 0)
                        {
                            Debug.Log("Self Coins Highest Value: " + coinValue + " , Coin Cell Index: " + coinCellIndex + " , Unblocked Adjacent Cells: " + string.Join(" , " , adjacentCellIndicesList));
                            foundCoinWithUnblockedCells = true;
                            break;
                        }
                    }
                }
            }
        }

        private Vector2Int FindBestAdjacentCell(List<int> coinValuesList)
        {
            if(isTestingMode)
            {
                ListAdjacentCellsForLesserCoins();
                ListAdjacentCellsForSelfCoins();   
            }

            List<Vector2Int> validAdjacentCellIndicesList = new List<Vector2Int>();
        
            foreach(int coinValue in coinValuesList.OrderByDescending(x => x))
            {
                List<Vector2Int> highestValueCoinCellIndicesList = new List<Vector2Int>();
        
                if(_lesserCoinValuesList.Count > 0 && coinValue == _lesserCoinValuesList.Max())
                {
                    highestValueCoinCellIndicesList.AddRange(_lesserCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue));
                }
        
                else if(_selfCoinValuesList.Count > 0 && coinValue == _selfCoinValuesList.Max())
                {
                    highestValueCoinCellIndicesList.AddRange(_selfCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue));
                }
        
                foreach(Vector2Int coinCellIndex in highestValueCoinCellIndicesList)
                {
                    List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndices(coinCellIndex);
        
                    adjacentCellIndicesList = adjacentCellIndicesList
                        .Where(adjacentCellIndex => adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
                                                    adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
                                                    !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                        .ToList();
        
                    validAdjacentCellIndicesList.AddRange(adjacentCellIndicesList);
                }
        
                if(validAdjacentCellIndicesList.Count > 0)
                {
                    int randomIndex = Random.Range(0 , validAdjacentCellIndicesList.Count);
                    //Debug.Log("FindBestAdjacentCell() -> Selected Cell Index : " + validAdjacentCellIndicesList[randomIndex]);
                    //Debug.Log("Adjacent Cells available for coin at " + validAdjacentCellIndicesList[randomIndex] + " : " + string.Join(" , " , validAdjacentCellIndicesList));
                    return validAdjacentCellIndicesList[randomIndex];
                }
            }

            return _gridManager.InvalidCellIndex;
        }


        public Vector2Int FindCellToPlaceCoinOn()
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

                        if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) != CurrentPlayerID)
                        {
                            _otherPlayerCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                            _otherPlayerCoinValuesList.Add(coinValue);

                            if(coinValue < CoinValue)
                            {
                                _lesserCoinsCellIndicesList.Add(new Vector2Int(x , y));
                                _lesserCoinValuesList.Add(coinValue);
                            }
                        }

                        if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) == CurrentPlayerID)
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
    
        public Color GetPlayerColor(int playerIndex)
        {
            switch(playerIndex)
            {
                case 0: return Color.red;
                case 1: return Color.green;
                case 2: return Color.blue;
                default: return Color.white;
            }
        }
    
        public int GetCurrentCoinValue()
        {
            int currentPlayerID = CurrentPlayerID;
        
            if(PlayerNumbersList[currentPlayerID].Count > 0)
            {
                return PlayerNumbersList[currentPlayerID][0];
            }
        
            return 0;
        }
    
        private void BuffUpAdjacentCoin()
        {
            CurrentPlayerID = _gridManager.PlayerIndexData.GetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y);

            ProcessAdjacentCells((x, y, adjacentCoinObj) =>
            {
                if(adjacentCoinObj != null)
                {
                    int adjacentPlayerID = _gridManager.PlayerIndexData.GetValue(x , y);
                    int adjacentCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(adjacentPlayerID == CurrentPlayerID)
                    {
                        int newAdjacentCoinValue = adjacentCoinValue + 1;
                        _gridManager.CoinValueData.SetValue(x , y , newAdjacentCoinValue);

                        _iPlayerTurnsManager.UpdateAdjacentCoinText(x , y , newAdjacentCoinValue);
                        EventsManager.Invoke(Event.CoinBuffedUp , adjacentPlayerID , newAdjacentCoinValue - adjacentCoinValue);
                    }
                }
            });
        }

        private void CaptureAdjacentCoin()
        {
            CurrentPlayerID = _gridManager.PlayerIndexData.GetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y);

            ProcessAdjacentCells((x, y, adjacentCoinObj) =>
            {
                if(adjacentCoinObj != null)
                {
                    int adjacentPlayerID = _gridManager.PlayerIndexData.GetValue(x , y);
                    int adjacentPlayerCoinValue = _gridManager.CoinValueData.GetValue(x , y);

                    if(adjacentPlayerID != CurrentPlayerID && adjacentPlayerCoinValue < CoinValue)
                    {
                        _gridManager.PlayerIndexData.SetValue(x , y , CurrentPlayerID);
                        EventsManager.Invoke(Event.CoinCaptured , CurrentPlayerID , adjacentPlayerID , adjacentPlayerCoinValue);
                        _iPlayerTurnsManager.UpdateCoinColor(x , y , CurrentPlayerID);
                    }
                }
            });
        }

        private void PlaceCoin()
        {
            _gridManager.CoinValueData.SetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y , CoinValue);
            _gridManager.PlayerIndexData.SetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y , CurrentPlayerID);
            _gridManager.TotalCells--;

            EventsManager.Invoke(Event.CoinPlaced , CoinValue , CurrentPlayerID);

            if(!_gridManager.IsCellBlockedData.GetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y))
            {
                BuffUpAdjacentCoin();
                CaptureAdjacentCoin();

                Vector2 spawnPos = _gridManager.CellToWorld(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y);
                GameObject newCoinObj = Instantiate(coinObj , spawnPos , Quaternion.identity , gameObject.transform);
                _gridManager.CoinOnTheCellData.SetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y , newCoinObj);
                newCoinObj.GetComponentInChildren<TextMeshPro>().text = CoinValue.ToString();

                _iPlayerTurnsManager.UpdateCoinColor(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y , CurrentPlayerID);
            
                for(int i = 0; i < IsAIArray.Length; i++)
                {
                    if(IsAIArray[i] && i == CurrentPlayerID)
                    {
                        StartCoroutine(AnimateCoinEffect(newCoinObj.GetComponentInChildren<SpriteRenderer>()));       
                    }
                }
            
                _gridManager.IsCellBlockedData.SetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y , true);
                _isMouseMoving = false;
            
                _iPlayerTurnsManager.UpdateCoinUIImageColors();
                UpdateTrailVisibility();
            
                _iPlayerTurnsManager.EndPlayerTurn();
                _iPlayerTurnsManager.StartPlayerTurn();
            }
        }
    
        private void ProcessAdjacentCells(Action<int , int , GameObject> processAction)
        {
            int minX = Mathf.Max(CellIndexAtMousePosition.x - 1 , 0);
            int maxX = Mathf.Min(CellIndexAtMousePosition.x + 1 , _gridManager.GridInfo.Cols - 1);
            int minY = Mathf.Max(CellIndexAtMousePosition.y - 1 , 0);
            int maxY = Mathf.Min(CellIndexAtMousePosition.y + 1 , _gridManager.GridInfo.Rows - 1);

            for(int x = minX; x <= maxX; x++)
            {
                for(int y = minY; y <= maxY; y++)
                {
                    if(x == CellIndexAtMousePosition.x && y == CellIndexAtMousePosition.y) continue;

                    bool isCellBlocked = _gridManager.IsCellBlockedData.GetValue(x , y);
                
                    if(isCellBlocked)
                    {
                        GameObject adjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(x , y);
                        processAction(x , y , adjacentCoinObj);
                    }
                }
            }
        }

        public void ResetPlayersRemaining()
        {
            PlayersRemainingList.Clear();

            for(int i = 0; i < NumberOfPlayers; i++)
            {
                PlayersRemainingList.Add(i);
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

        private void UpdateTrailVisibility()
        {
            MouseTrailObj.SetActive(_isMouseMoving);
        }
    
        #endregion
    
        #region Events Related Functions

        private void OnAIHumanToggled(int playerID , bool isAI)
        {
            IsAIArray[playerID] = isAI;
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
        
            _iPlayerTurnsManager = new PlayerTurnsManager(this , _gridManager);
        
            _iPlayerTurnsManager.UpdateTrailColor();

            _playersListsCapacity = _gridManager.TotalCells / NumberOfPlayers;

            _selfCoinsCellIndicesList = new List<Vector2Int>();
            _selfCoinValuesList = new List<int>();
            _lesserCoinsCellIndicesList = new List<Vector2Int>();
            _lesserCoinValuesList = new List<int>();
            _otherPlayerCoinsCellIndicesList = new List<Vector2Int>();
            _otherPlayerCoinValuesList = new List<int>();
            _unblockedCellIndicesList = new List<Vector2Int>();
            _playerNumbersList = new List<List<int>>();
            _playersRemainingList = new List<int>();

            for(int i = 0; i < NumberOfPlayers; i++)
            {
                PlayersRemainingList.Add(i);
            }

            for(int i = 0; i < NumberOfPlayers; i++)
            {
                List<int> randomNumbers = new List<int>(_playersListsCapacity);

                for(int j = 0; j < _playersListsCapacity; j++)
                {
                    int randomValue = Random.Range(1 , 21);
                    randomNumbers.Add(randomValue);
                }

                for(i = 0; i < NumberOfPlayers; i++)
                {
                    ShuffleList(randomNumbers);
                    PlayerNumbersList.Add(new List<int>(randomNumbers));
                }
            }
        
            _iPlayerTurnsManager.StartPlayerTurn();
        }

        private void OnMouseLeftClicked()
        {
            if(!_isGameStarted) return;

            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            if(CellIndexAtMousePosition == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y))
            {
                return;
            }

            PlaceCoin();
        }

        private void OnMouseMoved()
        {
            if(!_isGameStarted) return;
        
            for(int i = 0; i < IsAIArray.Length; i++)
            {
                if(IsAIArray[i] && CurrentPlayerID == i)
                {
                    return;
                }
            }

            _isMouseMoving = true;

            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseScreenPos.z = Camera.main.nearClipPlane;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            CellIndexAtMousePosition = _gridManager.WorldToCell(mouseWorldPos);
        
            if(CellIndexAtMousePosition != _gridManager.InvalidCellIndex)
            {
                Vector2 snapPos = _gridManager.CellToWorld(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y);
                MouseTrailObj.transform.position = snapPos;
            }
            else
            {
                MouseTrailObj.transform.position = mouseWorldPos;
            }

            UpdateTrailVisibility();
        }

        private void OnNumberOfPlayersSelected(int numberOfPlayers)
        {
            _numberOfPlayers = numberOfPlayers;
            _isAIArray = new bool[NumberOfPlayers];
            TotalReceivedArray = new int[NumberOfPlayers];
        }

        private void OnRandomTurnsToggled()
        {
            _isRandomTurns = !IsRandomTurns;
        }
    
        private void OnTouchscreenTapped()
        {
            if(!_isGameStarted) return;

            if(CellIndexAtMousePosition == _gridManager.InvalidCellIndex || _gridManager.IsCellBlockedData.GetValue(CellIndexAtMousePosition.x , CellIndexAtMousePosition.y))
            {
                return;
            }

            PlaceCoin();
        }
    
        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
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
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.AIHumanToggled , (Action<int , bool>)OnAIHumanToggled);
                EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.UnsubscribeFromEvent(Event.GamePaused , new Action(OnGamePaused));
                EventsManager.UnsubscribeFromEvent(Event.GameResumed , new Action(OnGameResumed));
                EventsManager.UnsubscribeFromEvent(Event.GameStarted , new Action(OnGameStarted));
                EventsManager.UnsubscribeFromEvent(Event.MouseLeftClicked , new Action(OnMouseLeftClicked));
                EventsManager.UnsubscribeFromEvent(Event.MouseMoved , new Action(OnMouseMoved));
                EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersSelected , (Action<int>)OnNumberOfPlayersSelected);
                EventsManager.UnsubscribeFromEvent(Event.RandomTurnsToggled , new Action(OnRandomTurnsToggled));
                EventsManager.UnsubscribeFromEvent(Event.TouchscreenTapped , new Action(OnTouchscreenTapped));
            }
        }

        #endregion
    }
}
