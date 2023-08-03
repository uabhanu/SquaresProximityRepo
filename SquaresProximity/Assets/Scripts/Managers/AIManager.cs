using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;

namespace Managers
{
    public class AIManager : IAIManager
    {
        #region Variables Declarations
        
        private GameManager _gameManager;
        private GridManager _gridManager;
        
        #endregion

        #region Constructor
        
        public AIManager(GameManager gameManager , GridManager gridManager)
        {
            _gameManager = gameManager;
            _gridManager = gridManager;
        }
        
        #endregion
        
        #region Custom Functions
        
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
        
        private void ListAdjacentCellsForLesserCoins()
        {
            if(_gameManager.LesserCoinValuesList.Count > 0)
            {
                bool foundCoinWithUnblockedCells = false;

                foreach(int coinValue in _gameManager.LesserCoinValuesList.OrderByDescending(x => x))
                {
                    if(foundCoinWithUnblockedCells) break;

                    List<Vector2Int> highestValueCoinCellIndicesList = _gameManager.LesserCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue).ToList();

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
            if(_gameManager.SelfCoinValuesList.Count > 0)
            {
                bool foundCoinWithUnblockedCells = false;

                foreach(int coinValue in _gameManager.SelfCoinValuesList.OrderByDescending(x => x))
                {
                    if(foundCoinWithUnblockedCells) break;

                    List<Vector2Int> highestValueCoinCellIndicesList = _gameManager.SelfCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue).ToList();

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
    
        private void PopulateLists()
        {
            for(int x = 0; x < _gridManager.GridInfo.Cols; x++)
            {
                for(int y = 0; y < _gridManager.GridInfo.Rows; y++)
                {
                    if(!_gridManager.IsCellBlockedData.GetValue(x , y))
                    {
                        _gameManager.UnblockedCellIndicesList.Add(new Vector2Int(x , y));
                    }
                
                    else if(_gridManager.IsCellBlockedData.GetValue(x , y))
                    {
                        GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);

                        if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) != _gameManager.CurrentPlayerID)
                        {
                            _gameManager.OtherPlayerCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                            _gameManager.OtherPlayerCoinValuesList.Add(coinValue);

                            if(coinValue < _gameManager.CoinValue)
                            {
                                _gameManager.LesserCoinsCellIndicesList.Add(new Vector2Int(x , y));
                                _gameManager.LesserCoinValuesList.Add(coinValue);
                            }
                        }

                        if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) == _gameManager.CurrentPlayerID)
                        {
                            _gameManager.SelfCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                            _gameManager.SelfCoinValuesList.Add(coinValue);
                        }
                    }
                }
            }
        }
        
        public IEnumerator AIPlaceCoinCoroutine()
        {
            yield return new WaitForSeconds(_gameManager.AICoinPlaceDelay);
            _gameManager.PlaceCoin();
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
        
        private Vector2Int FindBestAdjacentCell(List<int> coinValuesList)
        {
            if(_gameManager.IsTestingMode)
            {
                ListAdjacentCellsForLesserCoins();
                ListAdjacentCellsForSelfCoins();   
            }

            List<Vector2Int> validAdjacentCellIndicesList = new List<Vector2Int>();
        
            foreach(int coinValue in coinValuesList.OrderByDescending(x => x))
            {
                List<Vector2Int> highestValueCoinCellIndicesList = new List<Vector2Int>();
        
                if(_gameManager.LesserCoinValuesList.Count > 0 && coinValue == _gameManager.LesserCoinValuesList.Max())
                {
                    highestValueCoinCellIndicesList.AddRange(_gameManager.LesserCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue));
                }
        
                else if(_gameManager.SelfCoinValuesList.Count > 0 && coinValue == _gameManager.SelfCoinValuesList.Max())
                {
                    highestValueCoinCellIndicesList.AddRange(_gameManager.SelfCoinsCellIndicesList.Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue));
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
        
            if(_gameManager.LesserCoinValuesList.Count > 0)
            {
                //Debug.Log("Attack");
            
                _gameManager.LesserCoinValuesList.Sort((a, b) => b.CompareTo(a));
                targetCellIndex = FindBestAdjacentCell(_gameManager.LesserCoinValuesList);
            
                //Debug.Log("FindCellToPlaceCoinOn() Attack Block -> Target Cell Index : " + targetCellIndex);
    
                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    return targetCellIndex;
                }
            }

            if(_gameManager.SelfCoinValuesList.Count > 0)
            {
                //Debug.Log("Buff Up");
            
                _gameManager.SelfCoinValuesList.Sort((a, b) => b.CompareTo(a));
                targetCellIndex = FindBestAdjacentCell(_gameManager.SelfCoinValuesList);
            
                //Debug.Log("FindCellToPlaceCoinOn() Buff Up Block -> Target Cell Index : " + targetCellIndex);
            
                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    return targetCellIndex;
                }
            }

            if(_gameManager.UnblockedCellIndicesList.Count > 0)
            {
                //Debug.Log("Random cell");
            
                int index = Random.Range(0 , _gameManager.UnblockedCellIndicesList.Count);
                targetCellIndex = _gameManager.UnblockedCellIndicesList[index];
            
                //Debug.Log("FindCellToPlaceCoinOn() Random Block -> Target Cell Index : " + targetCellIndex);
            
                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    return targetCellIndex;
                }
            }

            return _gridManager.InvalidCellIndex;
        }
        
        #endregion
    }
}
