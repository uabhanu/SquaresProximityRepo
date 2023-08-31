using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class InGameUIManager : MonoBehaviour
    {
        #region Variable Declarations

        private const string HolesKey = "Holes";
        private const string RandomTurnsKey = "Random Turns";

        private bool _holesToggleBool;
        private bool _randomTurnsToggleBool;
        private bool[] _aiHumanSelectionsBoolArray;
        private int _highestScorePlayerID;
        private int _numberOfPlayers;
        private int[] _playersTotalWinsArray;
        private int[] _totalReceivedArray;
        private string[] _playerNamesArray;

        [SerializeField] private GameObject coinUIObj;
        [SerializeField] private GameObject continueButtonObj;
        [SerializeField] private GameObject gameOverPanelsObj;
        [SerializeField] private GameObject gameTiedPanelObj;
        [SerializeField] private GameObject inGameUIPanelsObj;
        [SerializeField] private GameObject inGameUIPlayerNamesDisplayPanelObj;
        [SerializeField] private GameObject leaderboardPanelObj;
        [SerializeField] private GameObject pauseButtonObj;
        [SerializeField] private GameObject pauseMenuPanelObj;
        [SerializeField] private GameObject numberOfPlayersSelectionPanelObj;
        [SerializeField] private GameObject playerInputPanelObj;
        [SerializeField] private GameObject totalReceivedPanelObj;
        [SerializeField] private GameObject totalWinsPanelObj;
        [SerializeField] private GameObject[] inGameUIPlayerNamesDisplayPanelObjs;
        [SerializeField] private GameObject[] leaderboardWinsPanelObjs;
        [SerializeField] private GameObject[] totalReceivedPanelObjs;
        [SerializeField] private GameObject[] winsPanelObjs;
        [SerializeField] private TMP_InputField[] playerNameTMPInputFields;
        [SerializeField] private TMP_Text backButtonTMPText;
        [SerializeField] private TMP_Text[] totalReceivedTMPTexts;
        [SerializeField] private TMP_Text[] playerTotalWinsLabelsTMPTexts;
        [SerializeField] private TMP_Text[] playerWinsLabelsTMPTexts;
        [SerializeField] private TMP_Text[] coinScoreTMPTexts;
        [SerializeField] private Toggle holesToggle;
        [SerializeField] private Toggle randomTurnsToggle;
        [SerializeField] private Toggle[] aiHumanTogglesArray;
        [SerializeField] private Toggle[] numberOfPlayersSelectionTogglesArray;

        #endregion
    
        #region MonoBehaviour Functions
        
        private void Start()
        {
            continueButtonObj.SetActive(false);
            gameOverPanelsObj.SetActive(false);
            gameTiedPanelObj.SetActive(false);
            inGameUIPanelsObj.SetActive(false);
            leaderboardPanelObj.SetActive(false);
            pauseMenuPanelObj.SetActive(false);
            playerInputPanelObj.SetActive(false);

            _playersTotalWinsArray = new int[_numberOfPlayers];
            _totalReceivedArray = new int[_numberOfPlayers];

            for(int i = 0; i < numberOfPlayersSelectionTogglesArray.Length; i++)
            {
                numberOfPlayersSelectionTogglesArray[i].isOn = false;
            }
            
            PlayerPrefsManager.LoadData(ref _holesToggleBool , HolesKey);
            holesToggle.isOn = _holesToggleBool;
            
            PlayerPrefsManager.LoadData(ref _randomTurnsToggleBool , RandomTurnsKey);
            randomTurnsToggle.isOn = _randomTurnsToggleBool;

            for(int i = 0; i < winsPanelObjs.Length; i++)
            {
                winsPanelObjs[i].SetActive(false);
            }

            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }
        
        #endregion

        #region User Defined Functions
        
        private void UpdateInGamePlayerNames(int playerID)
        {
            string playerName = playerNameTMPInputFields[playerID].text;
            EventsManager.Invoke(Event.PlayerNamesUpdated , playerID , playerName);
        }

        public void AIHumanToggle()
        {
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                bool isAI = aiHumanTogglesArray[i].isOn;
                _aiHumanSelectionsBoolArray[i] = isAI;
                EventsManager.Invoke(Event.AIHumanToggled , i , isAI);
            }
        }

        public void BackButton()
        {
            if(backButtonTMPText.text == "Back")
            {
                leaderboardPanelObj.SetActive(false);
                playerInputPanelObj.SetActive(true);   
            }
            else
            {
                EventsManager.Invoke(Event.GameResumed);
                leaderboardPanelObj.SetActive(false);
                inGameUIPanelsObj.SetActive(true);
                pauseButtonObj.SetActive(true);
                pauseMenuPanelObj.SetActive(false);
            }
        }
    
        public void BackButtonMain()
        {
            numberOfPlayersSelectionPanelObj.SetActive(true);
            playerInputPanelObj.SetActive(false);
        }

        public void ConfirmButton()
        {
            bool isAnyToggleOn = numberOfPlayersSelectionTogglesArray[0].isOn || numberOfPlayersSelectionTogglesArray[1].isOn;

            if(!isAnyToggleOn)
            {
                _numberOfPlayers = 2;
                numberOfPlayersSelectionTogglesArray[0].isOn = true;
                
                for(int i = 0; i < aiHumanTogglesArray.Length; i++)
                {
                    aiHumanTogglesArray[0].isOn = true;
                }
            }
        
            else if(numberOfPlayersSelectionTogglesArray[0].isOn && numberOfPlayersSelectionTogglesArray[1].isOn)
            {
                return;
            }
        
            else
            {
                _numberOfPlayers = numberOfPlayersSelectionTogglesArray[0].isOn ? 2 : 3;
                
                string[] aiKeys = new string[_numberOfPlayers];
                
                for(int i = 0; i < _numberOfPlayers; i++)
                {
                    aiKeys[i] = "Player" + i + "AI";
                    PlayerPrefsManager.LoadData(ref _aiHumanSelectionsBoolArray , aiKeys);
                    aiHumanTogglesArray[i].isOn = _aiHumanSelectionsBoolArray[i];
                }
            }
        
            numberOfPlayersSelectionPanelObj.SetActive(false);
            playerInputPanelObj.SetActive(true);
            
            string[] nameKeys = new string[_numberOfPlayers];
            string[] winsKeys = new string[_numberOfPlayers];
        
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                nameKeys[i] = "Player" + i + "Name";
                winsKeys[i] = "Player" + i + "TotalWins";
            }
            
            PlayerPrefsManager.LoadData(ref _playerNamesArray , nameKeys);
            PlayerPrefsManager.LoadData(ref _playersTotalWinsArray , winsKeys);

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                playerNameTMPInputFields[i].text = _playerNamesArray[i];
                playerTotalWinsLabelsTMPTexts[i].text = _playerNamesArray[i] + " Total Wins: " + _playersTotalWinsArray[i];
            }
        }

        public void ContinueButton()
        {
            continueButtonObj.SetActive(false);
            gameOverPanelsObj.SetActive(true);
        
            if(_numberOfPlayers == 2)
            {
                totalReceivedPanelObjs[_numberOfPlayers].SetActive(false);
                totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;

                winsPanelObjs[_numberOfPlayers].SetActive(false);
            }
        
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                playerNameTMPInputFields[i].text = _playerNamesArray[i];
                totalReceivedTMPTexts[i].text = playerNameTMPInputFields[i].text + " received : " + _totalReceivedArray[i];
            }
        
            string[] aiKeys = new string[_numberOfPlayers];
            string[] nameKeys = new string[_numberOfPlayers];
            string[] winsKeys = new string[_numberOfPlayers];
        
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                aiKeys[i] = "Player" + i + "AI";
                nameKeys[i] = "Player" + i + "Name";
                winsKeys[i] = "Player" + i + "TotalWins";
            }
        
            PlayerPrefsManager.SaveData(_aiHumanSelectionsBoolArray , aiKeys);
            PlayerPrefsManager.SaveData(_playerNamesArray , nameKeys);
            PlayerPrefsManager.SaveData(_playersTotalWinsArray , winsKeys);
        }

        public void EnterButton()
        {
            string defaultAIPlayerName = "AI";
            
            string[] defaultPlayerNames =
            {
                "Player 1",
                "Player 2",
                "Player 3"
            };

            if(_numberOfPlayers == 2)
            {
                inGameUIPlayerNamesDisplayPanelObjs[_numberOfPlayers].SetActive(false);
                inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 545;
            }

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                _playerNamesArray[i] = playerNameTMPInputFields[i].text;

                UpdateInGamePlayerNames(i);

                if(string.IsNullOrEmpty(playerNameTMPInputFields[i].text))
                {
                    if(!aiHumanTogglesArray[i].isOn)
                    {
                        _playerNamesArray[i] = defaultPlayerNames[i];
                        playerNameTMPInputFields[i].text = _playerNamesArray[i];
                        aiHumanTogglesArray[0].isOn = true;   
                    }
                    
                    if(aiHumanTogglesArray[i].isOn)
                    {
                        _playerNamesArray[i] = defaultAIPlayerName;
                        playerNameTMPInputFields[i].text = _playerNamesArray[i];
                        aiHumanTogglesArray[0].isOn = true;
                    }
                }
            }
        
            inGameUIPanelsObj.SetActive(true);
            playerInputPanelObj.SetActive(false);
            EventsManager.Invoke(Event.GameStarted);
        }

        public void HolesToggle()
        {
            EventsManager.Invoke(Event.HolesToggled);
            _holesToggleBool = holesToggle.isOn;
            PlayerPrefsManager.SaveData(_holesToggleBool , HolesKey);
        }

        public void LeaderboardButton()
        {
            leaderboardPanelObj.SetActive(true);
        
            if(_numberOfPlayers == 2)
            {
                leaderboardWinsPanelObjs[_numberOfPlayers].SetActive(false);
                totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;
            }

            playerInputPanelObj.SetActive(false);
        }

        public void LeaderBoardButtonPauseMenu()
        {
            backButtonTMPText.text = "Back to Game";
            inGameUIPanelsObj.SetActive(false);
            leaderboardPanelObj.SetActive(true);
        
            if(_numberOfPlayers == 2)
            {
                leaderboardWinsPanelObjs[_numberOfPlayers].SetActive(false);
                totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;
            }
        
            pauseButtonObj.SetActive(false);
        }

        public void OkButton()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void PauseButton()
        {
            EventsManager.Invoke(Event.GamePaused);
            pauseButtonObj.SetActive(false);
            pauseMenuPanelObj.SetActive(true);
        }

        public void RandomTurnsToggle()
        {
            EventsManager.Invoke(Event.RandomTurnsToggled);
            _randomTurnsToggleBool = randomTurnsToggle.isOn;
            PlayerPrefsManager.SaveData(_randomTurnsToggleBool , RandomTurnsKey);
        }

        public void ResetButton()
        {
            PlayerPrefsManager.DeleteAll();
        
            EventsManager.Invoke(Event.GameDataReset);

            for(int i = 0; i < numberOfPlayersSelectionTogglesArray.Length; i++)
            {
                numberOfPlayersSelectionTogglesArray[i].isOn = false;
            }
        
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                _aiHumanSelectionsBoolArray[i] = false;
                aiHumanTogglesArray[i].isOn = _aiHumanSelectionsBoolArray[i];
                _holesToggleBool = false;
                holesToggle.isOn = _holesToggleBool;
                _randomTurnsToggleBool = false;
                randomTurnsToggle.isOn = _randomTurnsToggleBool;
                playerNameTMPInputFields[i].text = "";
                _playersTotalWinsArray[i] = 0;
                playerTotalWinsLabelsTMPTexts[i].text = "";
            }
        
            string[] aiKeys = new string[_numberOfPlayers];
            string[] nameKeys = new string[_numberOfPlayers];
            string[] winsKeys = new string[_numberOfPlayers];
            
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                aiKeys[i] = "Player" + i + "AI";
                nameKeys[i] = "Player" + i + "Name";
                winsKeys[i] = "Player" + i + "TotalWins";
                
                PlayerPrefsManager.LoadData(ref _aiHumanSelectionsBoolArray , aiKeys);
                PlayerPrefsManager.LoadData(ref _playerNamesArray , nameKeys);
                PlayerPrefsManager.LoadData(ref _playersTotalWinsArray , winsKeys);
            }
            
            PlayerPrefsManager.LoadData(ref _holesToggleBool , HolesKey);
            PlayerPrefsManager.LoadData(ref _randomTurnsToggleBool , RandomTurnsKey);

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                playerNameTMPInputFields[i].text = _playerNamesArray[i];
                playerTotalWinsLabelsTMPTexts[i].text = _playerNamesArray[i] + " Total Wins: " + _playersTotalWinsArray[i];
            }
        }

        public void ResumeButton()
        {
            EventsManager.Invoke(Event.GameResumed);
            inGameUIPanelsObj.SetActive(true);
            pauseButtonObj.SetActive(true);
            pauseMenuPanelObj.SetActive(false);
        }

        public void SetPlayersNumber()
        {
            if(numberOfPlayersSelectionTogglesArray[0].isOn)
            {
                _numberOfPlayers = 2;
                _aiHumanSelectionsBoolArray = new bool[_numberOfPlayers];
                _playerNamesArray = new string[_numberOfPlayers];
                _playersTotalWinsArray = new int[_numberOfPlayers];
                _totalReceivedArray = new int[_numberOfPlayers];
                playerNameTMPInputFields[_numberOfPlayers].gameObject.SetActive(false);
                EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
            }
        
            else if(numberOfPlayersSelectionTogglesArray[1].isOn)
            {
                _numberOfPlayers = 3;
                _aiHumanSelectionsBoolArray = new bool[_numberOfPlayers];
                _playerNamesArray = new string[_numberOfPlayers];
                _playersTotalWinsArray = new int[_numberOfPlayers];
                _totalReceivedArray = new int[_numberOfPlayers];
                playerNameTMPInputFields[_numberOfPlayers - 1].gameObject.SetActive(true);
                EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);
            }
        }
        
        #endregion

        #region Events Related Functions
        
        private void OnGameOver()
        {
            coinUIObj.SetActive(false);
            continueButtonObj.SetActive(true);
            pauseButtonObj.SetActive(false);
        }

        private void OnGameTied()
        {
            gameTiedPanelObj.SetActive(true);
        }

        private void OnPlayerWins(int highestScorePlayerID)
        {
            _highestScorePlayerID = highestScorePlayerID;

            for(int i = 0; i < _playersTotalWinsArray.Length; i++)
            {
                if(i == _highestScorePlayerID)
                {
                    _playersTotalWinsArray[i]++;   
                }
            }
        
            playerWinsLabelsTMPTexts[_highestScorePlayerID].text = playerNameTMPInputFields[_highestScorePlayerID].text + " Wins!!!";
            winsPanelObjs[_highestScorePlayerID].SetActive(true);
        
            // string[] nameKeys = new string[_numberOfPlayers];
            // string[] winsKeys = new string[_numberOfPlayers];
            //
            // for(int i = 0; i < _numberOfPlayers; i++)
            // {
            //     nameKeys[i] = "Player" + i + "Name";
            //     winsKeys[i] = "Player" + i + "TotalWins";
            // }
            //
            // PlayerPrefsManager.SaveData(_playerNamesArray , nameKeys);
            // PlayerPrefsManager.SaveData(_playersTotalWinsArray , winsKeys);
        }

        private void OnScoreUpdated(int[] coinScoresArray)
        {
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                coinScoreTMPTexts[i].text = _playerNamesArray[i] + " : " + coinScoresArray[i];
            }
        }

        private void OnTotalReceived(int[] totalReceivedArray)
        {
            _totalReceivedArray = totalReceivedArray;
        }

        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.SubscribeToEvent(Event.GameTied , new Action(OnGameTied));
                EventsManager.SubscribeToEvent(Event.PlayerWins , (Action<int>)OnPlayerWins);
                EventsManager.SubscribeToEvent(Event.ScoreUpdated , (Action<int[]>)OnScoreUpdated);
                EventsManager.SubscribeToEvent(Event.PlayerTotalReceived , (Action<int[]>)OnTotalReceived);    
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.UnsubscribeFromEvent(Event.GameTied , new Action(OnGameTied));
                EventsManager.UnsubscribeFromEvent(Event.PlayerWins , (Action<int>)OnPlayerWins);
                EventsManager.UnsubscribeFromEvent(Event.ScoreUpdated , (Action<int[]>)OnScoreUpdated);
                EventsManager.UnsubscribeFromEvent(Event.PlayerTotalReceived , (Action<int[]>)OnTotalReceived);
            }
        }

        #endregion
    }
}