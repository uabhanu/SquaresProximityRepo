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
                int highestValueSum = int.MinValue;
            
                if(_gameManager.CoinValue > _gameManager.MinCoinValue)
                {
                     for(int i = 0; i < _gameManager.LesserCoinValuesList.Count; i++) 
                     {
                        int coinValue = _gameManager.LesserCoinValuesList[i];
                
                        if(coinValue > highestValueSum)
                        {
                            int highestValueCoinValue = _gameManager.LesserCoinValuesList[i];
                
                            List<Vector2Int> highestValueCellIndicesList = new List<Vector2Int>();
                
                            for(int j = 0; j < _gameManager.LesserCoinsCellIndicesList.Count; j++)
                            {
                                Vector2Int highestValueCoinCellIndex = _gameManager.LesserCoinsCellIndicesList[j];
                
                                coinValue = _gridManager.CoinValueData.GetValue(highestValueCoinCellIndex.x , highestValueCoinCellIndex.y);
                
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
                
                            foreach(Vector2Int cellIndex in highestValueCellIndicesList)
                            {
                                List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(cellIndex);
                
                                foreach(Vector2Int adjacentCellIndex in adjacentCellIndicesList)
                                {
                                    if(adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                                    {
                                        int adjacentCellCoinValue = _gridManager.CoinValueData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y);
                                        int currentSum = coinValue + adjacentCellCoinValue;
                
                                        if((_gameManager.CoinValue - highestValueCoinValue) <= _gameManager.MaxDifferenceAttack && currentSum > highestValueSum)
                                        {
                                            if(_gameManager.CoinValue > _gameManager.MinCoinValue)
                                            {
                                                if(_gameManager.IsAIArray[i])
                                                {
                                                    Debug.Log("Attack Block -> Coin Value is greater than or equal to " + _gameManager.MinCoinValue + " and Player is Human");
                                                    Debug.Log("Attack Block -> Difference between Coin Value and Highest Coin Value of the list is less than or equal to  " + _gameManager.MaxDifferenceAttack + " and Player is Human");
                                                
                                                    targetCellIndex = adjacentCellIndex;
                                                    Debug.Log("Attack Block -> Chosen Cell Index: " + targetCellIndex + " because this has maximum points and Player is Human");
                                                
                                                    return targetCellIndex;   
                                                }

                                                Debug.Log("Attack Block -> Coin Value is greater than or equal to " + _gameManager.MinCoinValue);
                                                Debug.Log("Attack Block -> Difference between Coin Value and Highest Coin Value of the list is less than or equal to  " + _gameManager.MaxDifferenceAttack);
                                                
                                                targetCellIndex = adjacentCellIndex;
                                                Debug.Log("Attack Block -> Chosen Cell Index: " + targetCellIndex + " because this has maximum points");
                                                
                                                return targetCellIndex;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                     }
                
                     if(targetCellIndex != _gridManager.InvalidCellIndex)
                     {
                         Debug.Log("Attack Block -> Chosen Cell Index: " + targetCellIndex);
                         return targetCellIndex;
                     }   
                }
            }
            
            if(buffUpCellIndicesList.Count > 0)
            {
                Vector2Int bestAdjacentCellIndex = _gridManager.InvalidCellIndex;

                foreach(int coinValue in _gameManager.SelfCoinValuesList.OrderByDescending(value => value))
                {
                    List<Vector2Int> validAdjacentCellIndicesList = new List<Vector2Int>();

                    foreach(Vector2Int cellIndex in _gameManager.SelfCoinsCellIndicesList)
                    {
                        int cellCoinValue = _gridManager.CoinValueData.GetValue(cellIndex.x , cellIndex.y);

                        if(cellCoinValue == coinValue && cellCoinValue < _gameManager.MaxCoinValue)
                        {
                            validAdjacentCellIndicesList.AddRange(GetAdjacentCellIndicesList(cellIndex).Where(adjacentCellIndex =>
                            adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y)));
                        }
                    }

                    validAdjacentCellIndicesList = validAdjacentCellIndicesList.Distinct().ToList();

                    if(validAdjacentCellIndicesList.Count == 1)
                    {
                        bestAdjacentCellIndex = validAdjacentCellIndicesList[0];
                        targetCellIndex = bestAdjacentCellIndex;
                        Debug.Log("Buff Up Block -> Chosen the Cell Index " + targetCellIndex + " because this is the only unblocked adjacent cell");
                        return targetCellIndex;
                    }
                }

                foreach(int coinValue in _gameManager.SelfCoinValuesList.OrderByDescending(value => value))
                {
                    List<Vector2Int> validAdjacentCellIndicesList = new List<Vector2Int>();
                
                    foreach(Vector2Int cellIndex in _gameManager.SelfCoinsCellIndicesList)
                    {
                        int cellCoinValue = _gridManager.CoinValueData.GetValue(cellIndex.x , cellIndex.y);
                
                        if(cellCoinValue == coinValue)
                        {
                            validAdjacentCellIndicesList.AddRange(GetAdjacentCellIndicesList(cellIndex).Where(adjacentCellIndex => 
                            adjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue
                            (adjacentCellIndex.x , adjacentCellIndex.y)));
                        }
                    }
                
                    validAdjacentCellIndicesList = validAdjacentCellIndicesList.Distinct().ToList();
                
                    foreach(Vector2Int adjacentCellIndex in validAdjacentCellIndicesList)
                    {
                        int totalBuffedCoins = 0;
                        List<Vector2Int> buffedCoinsCellIndicesList = new List<Vector2Int>();
                
                        foreach(Vector2Int adjacentAdjacentCellIndex in GetAdjacentCellIndicesList(adjacentCellIndex))
                        {
                            if(adjacentAdjacentCellIndex != _gridManager.InvalidCellIndex && _gameManager.CurrentPlayerID == _gridManager.PlayerIDData.GetValue(adjacentAdjacentCellIndex.x , adjacentAdjacentCellIndex.y))
                            {
                                totalBuffedCoins++;
                                buffedCoinsCellIndicesList.Add(adjacentAdjacentCellIndex);
                            }
                        }
                
                        if(totalBuffedCoins > 0 && totalBuffedCoins > _gameManager.CoinValue - coinValue)
                        {
                            if(coinValue <= _gameManager.MaxCoinValue && coinValue >= _gameManager.MinHigherCoinValue)
                            {
                                if(bestAdjacentCellIndex == _gridManager.InvalidCellIndex)
                                {
                                    bestAdjacentCellIndex = adjacentCellIndex;
                                    Debug.Log("Buff Up Block -> Chosen this Cell Index : " + bestAdjacentCellIndex + " because this has maximum points and priority went to numbers below 20 but 20 may still be adjacent");
                                    return bestAdjacentCellIndex;
                                }
                
                                int totalBuffedCoinsForBestCell = 0;
                
                                foreach(Vector2Int adjacentAdjacentCellIndex in GetAdjacentCellIndicesList(bestAdjacentCellIndex))
                                {
                                    if(adjacentAdjacentCellIndex != _gridManager.InvalidCellIndex && _gameManager.CurrentPlayerID == _gridManager.PlayerIDData.GetValue(adjacentAdjacentCellIndex.x , adjacentAdjacentCellIndex.y))
                                    {
                                        totalBuffedCoinsForBestCell++;
                                    }
                                }
                
                                if(totalBuffedCoins > totalBuffedCoinsForBestCell)
                                {
                                    Debug.Log("Buff Up Block -> Chosen this Cell Index : " + bestAdjacentCellIndex + " because this has maximum points");
                                    bestAdjacentCellIndex = adjacentCellIndex;
                                    return bestAdjacentCellIndex;
                                }
                            }
                            else
                            {
                                int totalBuffedCoinsForCurrentCell = 0;
                
                                foreach(Vector2Int adjacentAdjacentCellIndex in GetAdjacentCellIndicesList(adjacentCellIndex))
                                {
                                    if(adjacentAdjacentCellIndex != _gridManager.InvalidCellIndex && _gameManager.CurrentPlayerID == _gridManager.PlayerIDData.GetValue(adjacentAdjacentCellIndex.x , adjacentAdjacentCellIndex.y))
                                    {
                                        totalBuffedCoinsForCurrentCell++;
                                    }
                                }
                
                                if(totalBuffedCoins > totalBuffedCoinsForCurrentCell)
                                {
                                    Debug.Log("Buff Up Block -> Chosen this Cell Index : " + adjacentCellIndex + " because this has maximum points");
                                    bestAdjacentCellIndex = adjacentCellIndex;
                                    return bestAdjacentCellIndex;
                                }
                            }
                        }
                    }
                }
                
                if(bestAdjacentCellIndex != _gridManager.InvalidCellIndex && !_gridManager.IsCellBlockedData.GetValue(bestAdjacentCellIndex.x , bestAdjacentCellIndex.y))
                {
                    Debug.Log("Buff Up Block -> Chosen Cell Index : " + bestAdjacentCellIndex);
                    return bestAdjacentCellIndex;
                }
            }
            
            if(_gameManager.UnblockedCellIndicesList.Count > 0)
            {
                foreach(Vector2Int cellIndex in _gameManager.UnblockedCellIndicesList)
                {
                    List<Vector2Int> adjacentCells = GetAdjacentCellIndicesList(cellIndex);
            
                    int unblockedAdjacentCount = adjacentCells.Count(adjacentCell => adjacentCell != _gridManager.InvalidCellIndex &&
                    !_gridManager.IsCellBlockedData.GetValue(adjacentCell.x , adjacentCell.y));
                    
                    if((unblockedAdjacentCount <= 1 && _gameManager.CoinValue >= _gameManager.MinCoinValue && _gameManager.CoinValue <= 13) || _gameManager.CoinValue == 18 || _gameManager.CoinValue == 19)
                    {
                        targetCellIndex = cellIndex;
                        Debug.Log("Random Block -> Chosen Cell Index: " + targetCellIndex + " because the number of unblocked adjacent cells are 1 or less and " + _gameManager.CoinValue + " is between " + _gameManager.MinCoinValue + " & 13 or 18 or 19");
                        return targetCellIndex;
                    }
                    
                    if(unblockedAdjacentCount <= 3 && _gameManager.CoinValue >= _gameManager.MinHigherCoinValue && _gameManager.CoinValue <= _gameManager.MaxHigherCoinValue)
                    {
                        targetCellIndex = cellIndex;
                        Debug.Log("Random Block -> Chosen Cell Index: " + targetCellIndex + " because the number of unblocked adjacent cells are 3 or less and " + _gameManager.CoinValue + " is greater than or equal to " + _gameManager.MinHigherCoinValue + " & less than or equal to " + _gameManager.MaxHigherCoinValue);
                        return targetCellIndex;
                    }
                }
            
                int index = Random.Range(0 , _gameManager.UnblockedCellIndicesList.Count);
                targetCellIndex = _gameManager.UnblockedCellIndicesList[index];
                
                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    Debug.Log("Random Block -> Chosen Cell Index: " + targetCellIndex);
                    return targetCellIndex;
                }
            }

            return _gridManager.InvalidCellIndex;
        }
        
        #endregion
    }
}
