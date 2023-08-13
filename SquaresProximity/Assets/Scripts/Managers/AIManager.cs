using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    public class AIManager : IAIManager
    {
        #region Constructor
        
        public AIManager(GameManager gameManager , GridManager gridManager)
        {
            _gameManager = gameManager;
            _gridManager = gridManager;
        }
        
        #endregion

        #region Variables Declarations
        
        private GameManager _gameManager;
        private GridManager _gridManager;
        
        #endregion

        #region User Defined Functions
        
        private List<Vector2Int> GetAdjacentCellIndicesList(Vector2Int coinCellIndex)
        {
            if(coinCellIndex != _gridManager.InvalidCellIndex)
            {
                List<Vector2Int> adjacentCellIndicesList = new List<Vector2Int>
                {
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x - 1 , coinCellIndex.y)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x + 1 , coinCellIndex.y)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x - 1 , coinCellIndex.y + 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x + 1 , coinCellIndex.y + 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x , coinCellIndex.y - 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x , coinCellIndex.y + 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x - 1 , coinCellIndex.y - 1)),
                    _gridManager.WorldToCell(_gridManager.CellToWorld(coinCellIndex.x + 1 , coinCellIndex.y - 1))
                };

                return adjacentCellIndicesList;
            }

            return new List<Vector2Int>();
        }

        private List<Vector2Int> FindAdjacentCellIndicesList(List<int> coinValuesList)
        {
            List<Vector2Int> validAdjacentCellIndicesList = new List<Vector2Int>();

            foreach(int coinValue in coinValuesList)
            {
                List<Vector2Int> coinCellIndicesList = new List<Vector2Int>();
        
                if(_gameManager.LesserCoinValuesList.Contains(coinValue))
                {
                    coinCellIndicesList = _gameManager.LesserCoinsCellIndicesList
                    .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue)
                    .ToList();
                }
                
                else if(_gameManager.SelfCoinValuesList.Contains(coinValue))
                {
                    coinCellIndicesList = _gameManager.SelfCoinsCellIndicesList
                    .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue)
                    .ToList();
                }

                foreach(Vector2Int coinCellIndex in coinCellIndicesList)
                {
                    if(coinCellIndex != _gridManager.InvalidCellIndex)
                    {
                        //Debug.Log("FindAdjacentCellIndicesList() -> Cell Index To Pass to GetAdjacentCellIndicesList() : " + coinCellIndex);
                        List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(coinCellIndex);
                        
                        adjacentCellIndicesList = adjacentCellIndicesList
                        .Where(adjacentCellIndex => adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
                        adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
                        !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                        .ToList();

                        validAdjacentCellIndicesList.AddRange(adjacentCellIndicesList);
                    }
                }
            }

            return validAdjacentCellIndicesList;
        }

        private void ClearLists()
        {
            _gameManager.LesserCoinsCellIndicesList.Clear();
            _gameManager.LesserCoinValuesList.Clear();
            _gameManager.SelfCoinsCellIndicesList.Clear();
            _gameManager.SelfCoinValuesList.Clear();
            _gameManager.OtherPlayerCoinsCellIndicesList.Clear();
            _gameManager.OtherPlayerCoinValuesList.Clear();
            _gameManager.UnblockedCellIndicesList.Clear();
        }

        private void PopulateLists()
        {
            for(int x = 0; x < _gridManager.GridInfo.Cols; x++)
            {
                for(int y = 0; y < _gridManager.GridInfo.Rows; y++)
                {
                    bool hasAdjacentUnblockedCell = GetAdjacentCellIndicesList(new Vector2Int(x , y)).Any(adjacentIndex =>
                    adjacentIndex.x >= 0 && adjacentIndex.x < _gridManager.GridInfo.Cols &&
                    adjacentIndex.y >= 0 && adjacentIndex.y < _gridManager.GridInfo.Rows &&
                    !_gridManager.IsCellBlockedData.GetValue(adjacentIndex.x , adjacentIndex.y));
                    
                    if(!_gridManager.IsCellBlockedData.GetValue(x , y))
                    {
                        _gameManager.UnblockedCellIndicesList.Add(new Vector2Int(x , y));
                    }
                
                    else if(_gridManager.IsCellBlockedData.GetValue(x , y))
                    {
                        GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);
        
                        if(coin != null && _gridManager.PlayerIDData.GetValue(x , y) != _gameManager.CurrentPlayerID)
                        {
                            _gameManager.OtherPlayerCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                            _gameManager.OtherPlayerCoinValuesList.Add(coinValue);

                            if(coinValue < _gameManager.CoinValue && hasAdjacentUnblockedCell)
                            {
                                _gameManager.LesserCoinsCellIndicesList.Add(new Vector2Int(x , y));
                                
                                _gameManager.LesserCoinValuesList.Add(coinValue);
                                List<int> sortedLesserCoinValues = _gameManager.LesserCoinValuesList.OrderByDescending(value => value).ToList();
                                _gameManager.LesserCoinValuesList = sortedLesserCoinValues;
                            }
                        }
        
                        if(coin != null && _gridManager.PlayerIDData.GetValue(x , y) == _gameManager.CurrentPlayerID && hasAdjacentUnblockedCell)
                        {
                            _gameManager.SelfCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                            
                            _gameManager.SelfCoinValuesList.Add(coinValue);
                            List<int> sortedLesserCoinValues = _gameManager.SelfCoinValuesList.OrderByDescending(value => value).ToList();
                            _gameManager.SelfCoinValuesList = sortedLesserCoinValues;
                        }
                    }
                }
            }
        }
        
        public IEnumerator AIPlaceCoinCoroutine()
        {
            yield return new WaitForSeconds(_gameManager.AICoinPlaceDelay);
            _gameManager.ICoinPlacer.PlaceCoin(_gameManager.CellIndexToUse);
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

        public Vector2Int FindCellToPlaceCoinOn()
        {
            ClearLists();
            PopulateLists();

            Vector2Int targetCellIndex = _gridManager.InvalidCellIndex;
            List<Vector2Int> attackCellIndicesList = new List<Vector2Int>();
            List<Vector2Int> buffUpCellIndicesList = new List<Vector2Int>();

            if(_gameManager.LesserCoinValuesList.Count > 0)
            {
                attackCellIndicesList = FindAdjacentCellIndicesList(_gameManager.LesserCoinValuesList);
            }

            if(_gameManager.SelfCoinValuesList.Count > 0)
            {
                buffUpCellIndicesList = FindAdjacentCellIndicesList(_gameManager.SelfCoinValuesList);
            }

            if(attackCellIndicesList.Count > 0)
            {
                Debug.Log("Attack");

                int highestValue = int.MinValue;
                int highestValueIndex = -1;

                for(int i = 0; i < _gameManager.LesserCoinValuesList.Count; i++)
                {
                    int coinValue = _gameManager.LesserCoinValuesList[i];

                    if(coinValue > highestValue)
                    {
                        highestValue = coinValue;
                        highestValueIndex = i;
                    }
                }

                if(highestValueIndex != -1)
                {
                    int highestValueCoinValue = _gameManager.LesserCoinValuesList[highestValueIndex];

                    List<Vector2Int> highestValueCellIndicesList = new List<Vector2Int>();

                    for(int i = 0; i < _gameManager.LesserCoinsCellIndicesList.Count; i++)
                    {
                        Vector2Int highestValueCoinCellIndex = _gameManager.LesserCoinsCellIndicesList[i];

                        int coinValue = _gridManager.CoinValueData.GetValue(highestValueCoinCellIndex.x , highestValueCoinCellIndex.y);

                        if(coinValue == highestValueCoinValue)
                        {
                            highestValueCellIndicesList.Add(highestValueCoinCellIndex);
                        }
                    }

                    highestValueCellIndicesList.Sort((a, b) =>
                    {
                        int coinValueA = _gridManager.CoinValueData.GetValue(a.x , a.y);
                        int coinValueB = _gridManager.CoinValueData.GetValue(b.x , b.y);
                        return coinValueB.CompareTo(coinValueA);
                    });

                    Debug.Log("Coin Value of the Highest Value Coin of the LesserCoinValuesList : " + highestValueCoinValue);

                    foreach(Vector2Int cellIndex in highestValueCellIndicesList)
                    {
                        Debug.Log("Highest Value Coin of LesserCoinValuesList Found at Cell Index : " + cellIndex);

                        List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(cellIndex);

                        foreach(Vector2Int adjacentCellIndex in adjacentCellIndicesList)
                        {
                            if(adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                            {
                                int adjacentCellCoinValue = _gridManager.CoinValueData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y);
                                Debug.Log("The coin value of the coin on this adjacent cell " + adjacentCellIndex + " is : " + adjacentCellCoinValue);
                                targetCellIndex = adjacentCellIndex;
                            }
                        }
                    }
                    
                    Debug.Log("Attack Block -> Chosen Cell Index: " + targetCellIndex);
                    return targetCellIndex;
                }
            }
            
            if(buffUpCellIndicesList.Count > 0)
            {
                Debug.Log("Buff Up");
            
                int highestValue = int.MinValue;
                int highestValueIndex = -1;
            
                for(int i = 0; i < _gameManager.SelfCoinValuesList.Count; i++)
                {
                    int coinValue = _gameManager.SelfCoinValuesList[i];
            
                    if(coinValue > highestValue)
                    {
                        highestValue = coinValue;
                        highestValueIndex = i;
                    }
                }
            
                if(highestValueIndex != -1)
                {
                    int highestValueCoinValue = _gameManager.SelfCoinValuesList[highestValueIndex];
            
                    List<Vector2Int> highestValueCellIndicesList = new List<Vector2Int>();
            
                    for(int i = 0; i < _gameManager.SelfCoinsCellIndicesList.Count; i++)
                    {
                        Vector2Int highestValueCoinCellIndex = _gameManager.SelfCoinsCellIndicesList[i];
            
                        int coinValue = _gridManager.CoinValueData.GetValue(highestValueCoinCellIndex.x , highestValueCoinCellIndex.y);
            
                        if(coinValue == highestValueCoinValue)
                        {
                            highestValueCellIndicesList.Add(highestValueCoinCellIndex);
                        }
                    }
            
                    highestValueCellIndicesList.Sort((a, b) =>
                    {
                        int coinValueA = _gridManager.CoinValueData.GetValue(a.x , a.y);
                        int coinValueB = _gridManager.CoinValueData.GetValue(b.x , b.y);
                        return coinValueB.CompareTo(coinValueA);
                    });
            
                    Debug.Log("Coin Value of the Highest Value Coin of the LesserCoinValuesList : " + highestValueCoinValue);
            
                    foreach(Vector2Int cellIndex in highestValueCellIndicesList)
                    {
                        Debug.Log("Highest Value Coin of LesserCoinValuesList Found at Cell Index : " + cellIndex);
            
                        List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(cellIndex);
            
                        foreach(Vector2Int adjacentCellIndex in adjacentCellIndicesList)
                        {
                            if(adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                            {
                                int adjacentCellCoinValue = _gridManager.CoinValueData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y);
                                Debug.Log("The coin value of the coin on this adjacent cell " + adjacentCellIndex + " is : " + adjacentCellCoinValue);
                                targetCellIndex = adjacentCellIndex;
                            }
                        }
                    }
                    
                    Debug.Log("Buff Up Block -> Chosen Cell Index: " + targetCellIndex);
                    return targetCellIndex;
                }
            }

            if(_gameManager.UnblockedCellIndicesList.Count > 0)
            {
                //Debug.Log("Random Cell");
                
                foreach(Vector2Int cellIndex in _gameManager.UnblockedCellIndicesList)
                {
                    List<Vector2Int> adjacentCells = GetAdjacentCellIndicesList(cellIndex);

                    int unblockedAdjacentCount = adjacentCells.Count(adjacentCell => adjacentCell != _gridManager.InvalidCellIndex &&
                    !_gridManager.IsCellBlockedData.GetValue(adjacentCell.x , adjacentCell.y));

                    if(unblockedAdjacentCount <= 3 &&(_gameManager.CoinValue >= _gameManager.MinHigherCoinValue && _gameManager.CoinValue <= _gameManager.MaxHigherCoinValue))
                    {
                        //Debug.Log("Current Coin Value : " + _gameManager.CoinValue);
                        //Debug.Log("Random Block -> Cell Index : " + cellIndex + " -> Total adjacent unblocked cells : " + unblockedAdjacentCount);
                        return cellIndex;
                    }
                }

                int index = Random.Range(0 , _gameManager.UnblockedCellIndicesList.Count);
                targetCellIndex = _gameManager.UnblockedCellIndicesList[index];
                
                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    //Debug.Log("Random Block -> Chosen Cell Index: " + targetCellIndex);
                    return targetCellIndex;
                }
            }

            return _gridManager.InvalidCellIndex;
        }
        
        #endregion
    }
}
