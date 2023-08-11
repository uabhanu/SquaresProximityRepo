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
            
            //Debug.Log("GetAdjacentCoinValues() -> Cell Index Passed In : " + cellIndex);

            if(cellIndex != _gridManager.InvalidCellIndex)
            {
                List<Vector2Int> adjacentCellIndices = GetAdjacentCellIndicesList(cellIndex);

                foreach(Vector2Int adjacentIndex in adjacentCellIndices)
                {
                    int adjacentCellValue = _gridManager.CoinValueData.GetValue(adjacentIndex.x , adjacentIndex.y);
                    sum += adjacentCellValue;
                }
            }
            
            return sum;
        }

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
            List<Vector2Int> attackCellIndicesList = new List<Vector2Int>();
            List<Vector2Int> buffUpCellIndicesList = new List<Vector2Int>();

            if(_gameManager.LesserCoinValuesList.Count > 0)
            {
                //Debug.Log("Attack");
                attackCellIndicesList = FindAdjacentCellIndicesList(_gameManager.LesserCoinValuesList);
            }

            if(_gameManager.SelfCoinValuesList.Count > 0)
            {
                //Debug.Log("Buff Up");
                buffUpCellIndicesList = FindAdjacentCellIndicesList(_gameManager.SelfCoinValuesList);
            }

            if(attackCellIndicesList.Count > 0)
            {
                int highestValue = int.MinValue;
                Vector2Int highestValueCellIndex = Vector2Int.zero;

                foreach(Vector2Int cellIndex in attackCellIndicesList)
                {
                    int coinValue = _gridManager.CoinValueData.GetValue(cellIndex.x , cellIndex.y);

                    if(coinValue > highestValue)
                    {
                        highestValue = coinValue;
                        highestValueCellIndex = cellIndex;
                        //Debug.Log("FindCellToPlaceCoinOn() -> Attack -> Highest Value Cell Index : " + highestValueCellIndex);
                    }
                }

                List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(highestValueCellIndex);
                int highestAdjacentSum = int.MinValue;

                foreach(Vector2Int adjacentCellIndex in adjacentCellIndicesList)
                {
                    //Debug.Log("FindCellToPlaceCoinOn() -> Attack -> First Adjacent Cell Index from the Adjacent Cell Indices List : " + targetCellIndex);
                    
                    int adjacentSum = GetAdjacentCoinValues(adjacentCellIndex);

                    if(adjacentSum > highestAdjacentSum && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                    {
                        highestAdjacentSum = adjacentSum;
                        targetCellIndex = adjacentCellIndex;
                        //Debug.Log("FindCellToPlaceCoinOn() -> Attack -> Target Cell Index : " + targetCellIndex);
                    }
                }

                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    return targetCellIndex;
                }
            }

            if(buffUpCellIndicesList.Count > 0)
            {
                int highestValue = int.MinValue;
                Vector2Int highestValueCellIndex = Vector2Int.zero;

                foreach(Vector2Int cellIndex in buffUpCellIndicesList)
                {
                    int coinValue = _gridManager.CoinValueData.GetValue(cellIndex.x , cellIndex.y);

                    if(coinValue > highestValue)
                    {
                        highestValue = coinValue;
                        highestValueCellIndex = cellIndex;
                        //Debug.Log("FindCellToPlaceCoinOn() -> Buff Up -> Highest Value Cell Index : " + highestValueCellIndex);
                    }
                }

                List<Vector2Int> adjacentCellIndicesList = GetAdjacentCellIndicesList(highestValueCellIndex);
                int highestAdjacentSum = int.MinValue;

                foreach(Vector2Int adjacentCellIndex in adjacentCellIndicesList)
                {
                    //Debug.Log("FindCellToPlaceCoinOn() -> Buf Up -> First Adjacent Cell Index from the Adjacent Cell Indices List : " + targetCellIndex);
                    
                    int adjacentSum = GetAdjacentCoinValues(adjacentCellIndex);

                    if(adjacentSum > highestAdjacentSum && !_gridManager.IsCellBlockedData.GetValue(adjacentCellIndex.x , adjacentCellIndex.y))
                    {
                        highestAdjacentSum = adjacentSum;
                        targetCellIndex = adjacentCellIndex;
                        //Debug.Log("FindCellToPlaceCoinOn() -> Buff Up -> Target Cell Index : " + targetCellIndex);
                    }
                }

                if(targetCellIndex != _gridManager.InvalidCellIndex)
                {
                    return targetCellIndex;
                }
            }

            if(_gameManager.UnblockedCellIndicesList.Count > 0)
            {
                //Debug.Log("Random Cell");

                int index = Random.Range(0 , _gameManager.UnblockedCellIndicesList.Count);
                targetCellIndex = _gameManager.UnblockedCellIndicesList[index];

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
