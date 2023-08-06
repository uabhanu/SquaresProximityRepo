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
        
        private int GetAdjacentCoinValues(Vector2Int cellIndex)
        {
            int sum = 0;
            
            List<Vector2Int> adjacentCellIndices = GetAdjacentCellIndices(cellIndex);

            foreach(Vector2Int adjacentIndex in adjacentCellIndices)
            {
                if(adjacentIndex.x >= 0 && adjacentIndex.x < _gridManager.GridInfo.Cols && adjacentIndex.y >= 0 && adjacentIndex.y < _gridManager.GridInfo.Rows)
                {
                    sum += _gridManager.CoinValueData.GetValue(adjacentIndex.x , adjacentIndex.y);
                }
            }

            return sum;
        }
        
        private int GetUnblockedAdjacentCellsCount(Vector2Int cellIndex)
        {
            int count = 0;
            List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndices(cellIndex);

            foreach(Vector2Int adjacentCellIndex in adjacentCellIndicesList)
            {
                if(adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
                adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
                !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                {
                    count++;
                }
            }

            return count;
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
            List<Vector2Int> validAdjacentCellIndicesList = new List<Vector2Int>();
            
            coinValuesList.Sort((a, b) => b.CompareTo(a));
            
            int highestAdjacentCoinValuesSum = 0;
            Vector2Int bestAdjacentCell = default(Vector2Int);
            
            foreach(int coinValue in coinValuesList)
            {
                List<Vector2Int> coinCellIndicesList = new List<Vector2Int>();
                
                if(_gameManager.LesserCoinValuesList.Contains(coinValue))
                {
                    if((_gameManager.CoinValue - coinValue) <= _gameManager.MaxDifferenceAttack || (_gameManager.CoinValue >= 14 && (_gameManager.CoinValue - coinValue) <= 1))
                    {
                        coinCellIndicesList = _gameManager.LesserCoinsCellIndicesList
                        .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue)
                        .ToList();
                    }
                }
                
                else if(_gameManager.SelfCoinValuesList.Contains(coinValue))
                {
                    coinCellIndicesList = _gameManager.SelfCoinsCellIndicesList
                    .Where(position => _gridManager.CoinValueData.GetValue(position.x , position.y) == coinValue)
                    .ToList();
                }

                foreach(Vector2Int coinCellIndex in coinCellIndicesList)
                {
                    List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndices(coinCellIndex);

                    adjacentCellIndicesList = adjacentCellIndicesList
                    .Where(adjacentCellIndex => adjacentCellIndex.x >= 0 && adjacentCellIndex.x < _gridManager.GridInfo.Cols &&
                    adjacentCellIndex.y >= 0 && adjacentCellIndex.y < _gridManager.GridInfo.Rows &&
                    !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                    .ToList();

                    if(adjacentCellIndicesList.Count == 1 && _gameManager.CoinValue <= coinValue)
                    {
                        bestAdjacentCell = adjacentCellIndicesList[0];
                        return bestAdjacentCell;
                    }

                    if(bestAdjacentCell == default && coinValue < _gameManager.MaxCoinValue)
                    {
                        int highestAdjacentValueUnder20 = 0;
                        int highestAdjacentSumUnder20 = 0;

                        int highestAdjacentValue20OrMore = 0;
                        int highestAdjacentSum20OrMore = 0;

                        foreach(Vector2Int adjacentCell in adjacentCellIndicesList)
                        {
                            int adjacentCellValue = _gridManager.CoinValueData.GetValue(adjacentCell.x , adjacentCell.y);
                            int adjacentSum = GetAdjacentCoinValues(adjacentCell);

                            if(adjacentCellValue < _gameManager.MaxCoinValue)
                            {
                                if(adjacentCellValue > highestAdjacentValueUnder20 || (adjacentCellValue == highestAdjacentValueUnder20 && adjacentSum > highestAdjacentSumUnder20))
                                {
                                    highestAdjacentValueUnder20 = adjacentCellValue;
                                    highestAdjacentSumUnder20 = adjacentSum;
                                    bestAdjacentCell = adjacentCell;
                                }
                            }
                            else
                            {
                                if(adjacentCellValue > highestAdjacentValue20OrMore || (adjacentCellValue == highestAdjacentValue20OrMore && adjacentSum > highestAdjacentSum20OrMore))
                                {
                                    highestAdjacentValue20OrMore = adjacentCellValue;
                                    highestAdjacentSum20OrMore = adjacentSum;
                                    bestAdjacentCell = adjacentCell;
                                }
                            }
                        }
                    }

                    validAdjacentCellIndicesList.AddRange(adjacentCellIndicesList);
                }
            }
            
            if(validAdjacentCellIndicesList.Count == 0)
            {
                return _gridManager.InvalidCellIndex;
            }
            
            foreach(Vector2Int cellIndex in validAdjacentCellIndicesList)
            {
                int adjacentCoinValuesSum = GetAdjacentCoinValues(cellIndex);
                bool isHumanPlayer = !_gameManager.IsAIArray[_gridManager.PlayerIndexData.GetValue(cellIndex.x , cellIndex.y)];

                if(adjacentCoinValuesSum > highestAdjacentCoinValuesSum ||
                (isHumanPlayer && adjacentCoinValuesSum >= highestAdjacentCoinValuesSum &&
                adjacentCoinValuesSum - highestAdjacentCoinValuesSum <= _gameManager.MaxDifferenceAIHumanPoints))
                {
                    highestAdjacentCoinValuesSum = adjacentCoinValuesSum;
                    bestAdjacentCell = cellIndex;
                }
            }

            return bestAdjacentCell;
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
                    bool hasAdjacentUnblockedCell = GetAdjacentCellIndices(new Vector2Int(x , y)).Any(adjacentIndex =>
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
        
                        if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) != _gameManager.CurrentPlayerID)
                        {
                            _gameManager.OtherPlayerCoinsCellIndicesList.Add(new Vector2Int(x , y));
                            int coinValue = _gridManager.CoinValueData.GetValue(x , y);
                            _gameManager.OtherPlayerCoinValuesList.Add(coinValue);

                            if(coinValue < _gameManager.CoinValue && hasAdjacentUnblockedCell)
                            {
                                _gameManager.LesserCoinsCellIndicesList.Add(new Vector2Int(x , y));
                                _gameManager.LesserCoinValuesList.Add(coinValue);
                            }
                        }
        
                        if(coin != null && _gridManager.PlayerIndexData.GetValue(x , y) == _gameManager.CurrentPlayerID && hasAdjacentUnblockedCell)
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
        
            if(_gameManager.LesserCoinValuesList.Count > 0)
            {
                Debug.Log("Attack");
            
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
                Debug.Log("Buff Up");
            
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
