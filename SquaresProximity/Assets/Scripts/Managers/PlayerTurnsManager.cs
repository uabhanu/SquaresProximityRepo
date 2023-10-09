using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class PlayerTurnsManager : IPlayerTurnsManager
    {
        #region Constructor
        
        public PlayerTurnsManager(GameManager gameManager , GridManager gridManager)
        {
            _gameManager = gameManager;
            _gridManager = gridManager;
        }
        
        #endregion

        #region Variables Declarations
        
        private GameManager _gameManager;
        private GridManager _gridManager;

        #endregion

        #region Functions
        
        private void UpdateCoinValueAfterPlacement()
        {
            int currentPlayerID = _gameManager.CurrentPlayerID;
        
            if(_gameManager.PlayerNumbersList[currentPlayerID].Count > 0)
            {
                _gameManager.CoinValue = _gameManager.GetCurrentCoinValue();
                TMP_Text coinUITMP = _gameManager.CoinUIObj.GetComponentInChildren<TMP_Text>();
                coinUITMP.text = _gameManager.CoinValue.ToString();
    
                for(int i = 0; i < _gameManager.TotalReceivedArray.Length; i++)
                {
                    if(currentPlayerID == i)
                    {
                        _gameManager.TotalReceivedArray[i] += _gameManager.CoinValue;
                    }
                }
    
                _gameManager.PlayerNumbersList[currentPlayerID].RemoveAt(0);
            }
        }

        public void EndPlayerTurn()
        {
            _gameManager.CurrentPlayerID = (_gameManager.CurrentPlayerID + 1) % _gameManager.NumberOfPlayers;
        }

        public void StartPlayerTurn()
        {
            if(_gameManager.IsRandomTurns)
            {
                int remainingPlayersCount = _gameManager.PlayersRemainingList.Count;
                int randomIndex = Random.Range(0 , remainingPlayersCount);
                _gameManager.CurrentPlayerID = _gameManager.PlayersRemainingList[randomIndex];
                _gameManager.PlayersRemainingList.RemoveAt(randomIndex);
            }

            bool foundUnblockedCell = false;
            int maxIterations = _gridManager.GridInfo.Cols * _gridManager.GridInfo.Rows;
            int currentIteration = 0;

            UpdateCoinValueAfterPlacement();

            if(_gameManager.PlayersRemainingList.Count == 0)
            {
                _gameManager.ResetPlayersRemaining();
            }

            while(!foundUnblockedCell && currentIteration < maxIterations)
            {
                for(int i = 0; i < _gameManager.IsAIArray.Length; i++)
                {
                    if(_gameManager.IsAIArray[i] && _gameManager.CurrentPlayerID == i)
                    {
                        _gameManager.CellIndexToUse = _gameManager.IAIManager.FindCellToPlaceCoinOn();

                        if(_gameManager.CellIndexToUse != _gridManager.InvalidCellIndex)
                        {
                            _gameManager.StartCoroutine(_gameManager.IAIManager.AIPlaceCoinCoroutine());
                            foundUnblockedCell = true;
                        }
                    }
                }

                currentIteration++;
            }

            UpdateCoinUIImageColors();
            UpdateTrailColor();
        }

        public void UpdateAdjacentCoinText(int x , int y , int newCoinValue)
        {
            GameObject adjacentCoinObj = _gridManager.CoinOnTheCellData.GetValue(x , y);
            TMP_Text adjacentCoinValueText;

            if(adjacentCoinObj != null)
            {
                adjacentCoinValueText = adjacentCoinObj.GetComponentInChildren<TMP_Text>();

                if(adjacentCoinValueText == null)
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
                    adjacentCoinValueText.text = newCoinValue.ToString();
                }
                else
                {
                    Debug.LogWarning("Could not find adjacent cell with TMP_Text component.");
                }
            }
        }

        public void UpdateCoinColor(int x , int y , int playerIndex)
        {
            GameObject coin = _gridManager.CoinOnTheCellData.GetValue(x , y);

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
            
                for(int i = 0; i < _gameManager.IsAIArray.Length; i++)
                {
                    if(_gameManager.IsAIArray[i])
                    {
                        _gameManager.StartCoroutine(_gameManager.IAIManager.AnimateCoinEffect(coinRenderer , coinRenderer.color));       
                    }
                }
            }
        }

        public void UpdateCoinUIImageColors()
        {
            if(_gameManager.CoinUIObj != null)
            {
                Color coinUIColour = _gameManager.GetCoinUIBackgroundColour(_gameManager.CurrentPlayerID);
                Image coinUIImage = _gameManager.CoinUIObj.GetComponent<Image>();
                coinUIImage.color = coinUIColour;
            
                TMP_Text coinUIText = _gameManager.CoinUIObj.GetComponentInChildren<TMP_Text>();
        
                switch(_gameManager.CurrentPlayerID)
                {
                    case 0:
                        coinUIText.color = _gameManager.GetCoinUIForegroundColour(_gameManager.CurrentPlayerID);
                    break;

                    case 1:
                        coinUIText.color = _gameManager.GetCoinUIForegroundColour(_gameManager.CurrentPlayerID);
                    break;

                    case 2:
                        coinUIText.color = _gameManager.GetCoinUIForegroundColour(_gameManager.CurrentPlayerID);
                    break;

                    default:
                        coinUIText.color = Color.black;
                    break;
                }
            }
        }

        public void UpdateTrailColor()
        {
            if(_gameManager.MouseTrailObj != null)
            {
                SpriteRenderer trailRenderer = _gameManager.MouseTrailObj.GetComponentInChildren<SpriteRenderer>();
                Color playerColour = _gameManager.GetCoinUIBackgroundColour(_gameManager.CurrentPlayerID);

                playerColour.a *= 0.5f;
                trailRenderer.color = playerColour;
            }
        }
        
        #endregion
    }
}