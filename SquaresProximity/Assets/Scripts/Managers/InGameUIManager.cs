namespace Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    
    public class InGameUIManager : MonoBehaviour
    {
        #region Variable Declarations

        private const string HolesKey = "Holes";
        private const string RandomTurnsKey = "Random Turns";

        private bool _holesToggleBool;
        private bool _randomTurnsToggleBool;
        private bool[] _aiHumanSelectionsBoolArray;
        private bool[] _numberOfPlayersSelectionsBoolArray;
        private bool[] _offlineOnlineSelectionsBoolArray;
        private int _highestScorePlayerID;
        private int _numberOfPlayers;
        private int[] _playerScoresArray;
        private int[] _playersTotalWinsArray;
        private int[] _totalReceivedArray;
        private string[] _playerNamesArray;

        [SerializeField] private GameObject coinUIObj;
        [SerializeField] private GameObject continueButtonObj;
        [SerializeField] private GameObject gameIntroPanelObj;
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
        [SerializeField] private TMP_Text[] gameTitleTMPTexts;
        [SerializeField] private TMP_Text[] totalReceivedTMPTexts;
        [SerializeField] private TMP_Text[] playerTotalWinsLabelsTMPTexts;
        [SerializeField] private TMP_Text[] playerWinsLabelsTMPTexts;
        [SerializeField] private TMP_Text[] coinScoreTMPTexts;
        [SerializeField] private Toggle holesToggle;
        [SerializeField] private Toggle randomTurnsToggle;
        [SerializeField] private Toggle[] aiHumanTogglesArray;
        [SerializeField] private Toggle[] numberOfPlayersSelectionTogglesArray;
        [SerializeField] private Toggle[] offlineOnlineSelectionTogglesArray;

        #endregion
    
        #region MonoBehaviour Functions
        
        private void Start()
        {
            continueButtonObj.SetActive(false);
            gameOverPanelsObj.SetActive(false);
            gameTiedPanelObj.SetActive(false);
            inGameUIPanelsObj.SetActive(false);
            leaderboardPanelObj.SetActive(false);
            numberOfPlayersSelectionPanelObj.SetActive(false);
            pauseMenuPanelObj.SetActive(false);
            playerInputPanelObj.SetActive(false);

            _numberOfPlayersSelectionsBoolArray = new bool[numberOfPlayersSelectionTogglesArray.Length];
            _offlineOnlineSelectionsBoolArray = new bool[offlineOnlineSelectionTogglesArray.Length];
            _playersTotalWinsArray = new int[_numberOfPlayers];
            _totalReceivedArray = new int[_numberOfPlayers];
            
            PlayerPrefsManager.LoadData(ref _holesToggleBool , HolesKey);
            holesToggle.isOn = _holesToggleBool;
            
            PlayerPrefsManager.LoadData(ref _randomTurnsToggleBool , RandomTurnsKey);
            randomTurnsToggle.isOn = _randomTurnsToggleBool;

            for(int i = 0; i < gameTitleTMPTexts.Length; i++)
            {
                gameTitleTMPTexts[i].enabled = false;
            }

            for(int i = 0; i < winsPanelObjs.Length; i++)
            {
                winsPanelObjs[i].SetActive(false);
            }
            
            string[] numberOfPlayersKeys = new string[numberOfPlayersSelectionTogglesArray.Length];
            
            for(int i = 0; i < numberOfPlayersSelectionTogglesArray.Length; i++)
            {
                numberOfPlayersKeys[i] = "Number Of Players" + i;
                PlayerPrefsManager.LoadData(ref _numberOfPlayersSelectionsBoolArray , numberOfPlayersKeys);
                numberOfPlayersSelectionTogglesArray[i].isOn = _numberOfPlayersSelectionsBoolArray[i];
            }

            string[] offlineOnlineKeys = new string[offlineOnlineSelectionTogglesArray.Length];
            
            for(int i = 0; i < offlineOnlineSelectionTogglesArray.Length; i++)
            {
                offlineOnlineKeys[i] = "Player" + i + "Offline or Online";
                PlayerPrefsManager.LoadData(ref _offlineOnlineSelectionsBoolArray , offlineOnlineKeys);
                offlineOnlineSelectionTogglesArray[i].isOn = _offlineOnlineSelectionsBoolArray[i];
            }
            
            SetPlayersNumber();

            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }
        
        #endregion

        #region Button Functions
        
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
            string[] aiKeys = new string[_numberOfPlayers];
            
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                aiKeys[i] = "Player" + i + "AI";
                PlayerPrefsManager.LoadData(ref _aiHumanSelectionsBoolArray , aiKeys);
                aiHumanTogglesArray[i].isOn = _aiHumanSelectionsBoolArray[i];
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
                #if UNITY_ANDROID || UNITY_IOS
                    totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 370;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 500;          
                #endif
            }
            
            else if(_numberOfPlayers == 3)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 45;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 145;          
                #endif
            }
            
            else if(_numberOfPlayers == 4)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = -60;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalReceivedPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 25;          
                #endif
            }
            
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                playerNameTMPInputFields[i].text = _playerNamesArray[i];
                totalReceivedPanelObjs[i].SetActive(true);
                totalReceivedTMPTexts[i].text = playerNameTMPInputFields[i].text + " received : " + _totalReceivedArray[i];
            }
            
            string[] winsKeys = new string[_numberOfPlayers];
            
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                winsKeys[i] = "Player" + i + "TotalWins";
            }
            
            PlayerPrefsManager.SaveData(_playersTotalWinsArray , winsKeys);
        }

        public void EnterButton()
        {
            string defaultAIPlayerName = "AI";
            
            string[] defaultPlayerNames =
            {
                "Player 1",
                "Player 2",
                "Player 3",
                "Player 4"
            };

            if(_numberOfPlayers == 2)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 500;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 650;          
                #endif
            }
            
            else if(_numberOfPlayers == 3)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 150;         
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 250;          
                #endif
            }
            
            else if(_numberOfPlayers == 4)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 25;         
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    inGameUIPlayerNamesDisplayPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 125;          
                #endif
            }

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                inGameUIPlayerNamesDisplayPanelObjs[i].SetActive(true);
                _playerNamesArray[i] = playerNameTMPInputFields[i].text;

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
                
                UpdateInGamePlayerNames(i);
            }
        
            inGameUIPanelsObj.SetActive(true);
            playerInputPanelObj.SetActive(false);
            EventsManager.Invoke(Event.GameStarted);
            
            for(int i = 0; i < gameTitleTMPTexts.Length; i++)
            {
                gameTitleTMPTexts[i].enabled = false;
            }
            
            string[] aiKeys = new string[_numberOfPlayers];
            string[] nameKeys = new string[_numberOfPlayers];
            
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                aiKeys[i] = "Player" + i + "AI";
                nameKeys[i] = "Player" + i + "Name";
            }
            
            string[] offlineOnlineKeys = new string[_offlineOnlineSelectionsBoolArray.Length];
            
            for(int i = 0; i < _offlineOnlineSelectionsBoolArray.Length; i++)
            {
                offlineOnlineKeys[i] = "Player" + i + "Offline or Online";
            }
            
            PlayerPrefsManager.SaveData(_aiHumanSelectionsBoolArray , aiKeys);
            PlayerPrefsManager.SaveData(_offlineOnlineSelectionsBoolArray , offlineOnlineKeys);
            PlayerPrefsManager.SaveData(_playerNamesArray , nameKeys);
        }

        public void LeaderboardButton()
        {
            leaderboardPanelObj.SetActive(true);
        
            if(_numberOfPlayers == 2)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;     
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 500;          
                #endif
            }
            
            else if(_numberOfPlayers == 3)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 100;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 145;          
                #endif
            }
            
            else if(_numberOfPlayers == 4)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = -15;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 25;          
                #endif
            }

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                leaderboardWinsPanelObjs[i].SetActive(true);
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
                #if UNITY_ANDROID || UNITY_IOS
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 400;     
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 500;          
                #endif
            }
            
            else if(_numberOfPlayers == 3)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 100;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 145;          
                #endif
            }
            
            else if(_numberOfPlayers == 4)
            {
                #if UNITY_ANDROID || UNITY_IOS
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = -15;          
                #endif
                
                #if UNITY_STANDALONE || UNITY_WEBGL
                    totalWinsPanelObj.GetComponent<HorizontalLayoutGroup>().spacing = 25;          
                #endif
            }
            
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                leaderboardWinsPanelObjs[i].SetActive(true);
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

        public void QuitButton()
        {
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #endif
            
            #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
                Application.Quit();
            #endif
        }

        public void ReadyButton()
        {
            gameIntroPanelObj.SetActive(false);
            numberOfPlayersSelectionPanelObj.SetActive(true);
            
            for(int i = 0; i < gameTitleTMPTexts.Length; i++)
            {
                gameTitleTMPTexts[i].enabled = true;
            }
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
                _offlineOnlineSelectionsBoolArray[i] = false;
                offlineOnlineSelectionTogglesArray[i].isOn = _offlineOnlineSelectionsBoolArray[i];
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
            
            string[] offlineOnlineKeys = new string[_offlineOnlineSelectionsBoolArray.Length];
            
            for(int i = 0; i < _offlineOnlineSelectionsBoolArray.Length; i++)
            {
                offlineOnlineKeys[i] = "Player" + i + "Offline or Online";
                
                PlayerPrefsManager.LoadData(ref _offlineOnlineSelectionsBoolArray , offlineOnlineKeys);
            }
            
            PlayerPrefsManager.LoadData(ref _holesToggleBool , HolesKey);
            PlayerPrefsManager.LoadData(ref _randomTurnsToggleBool , RandomTurnsKey);

            for(int i = 0; i < _numberOfPlayers; i++)
            {
                playerNameTMPInputFields[i].text = _playerNamesArray[i];
                playerTotalWinsLabelsTMPTexts[i].text = _playerNamesArray[i] + " Total Wins: " + _playersTotalWinsArray[i];
            }
        }

        public void RestartButton()
        {
            EventsManager.Invoke(Event.GameRestarted);
        }

        public void ResumeButton()
        {
            EventsManager.Invoke(Event.GameResumed);
            inGameUIPanelsObj.SetActive(true);
            pauseButtonObj.SetActive(true);
            pauseMenuPanelObj.SetActive(false);
        }
        
        #endregion
        
        #region Other Functions
        
        private void UpdateInGamePlayerNames(int playerID)
        {
            string playerName = playerNameTMPInputFields[playerID].text;
            EventsManager.Invoke(Event.PlayerNamesUpdated , playerID , playerName);
        }

        public void SetPlayersNumber()
        {
            int selectedPlayers = 0;

            for(int i = 0; i < numberOfPlayersSelectionTogglesArray.Length; i++)
            {
                if(numberOfPlayersSelectionTogglesArray[i].isOn)
                {
                    selectedPlayers = i + 2;
                    break;
                }
            }

            _numberOfPlayers = selectedPlayers;
            _aiHumanSelectionsBoolArray = new bool[_numberOfPlayers];
            _playerNamesArray = new string[_numberOfPlayers];
            _playersTotalWinsArray = new int[_numberOfPlayers];
            _totalReceivedArray = new int[_numberOfPlayers];

            for(int i = 0; i < playerNameTMPInputFields.Length; i++)
            {
                playerNameTMPInputFields[i].gameObject.SetActive(i < _numberOfPlayers);
            }

            EventsManager.Invoke(Event.NumberOfPlayersSelected , _numberOfPlayers);

            PlayerPrefs.SetInt("NumberOfPlayers" , _numberOfPlayers);
            PlayerPrefs.Save();
        }
        
        #endregion

        #region Toggle Functions

        public void AIHumanToggle()
        {
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                bool isAI = aiHumanTogglesArray[i].isOn;
                _aiHumanSelectionsBoolArray[i] = isAI;
                EventsManager.Invoke(Event.AIHumanToggled , i , isAI);
            }
        }
        
        public void HolesToggle()
        {
            EventsManager.Invoke(Event.HolesToggled);
            _holesToggleBool = holesToggle.isOn;
            PlayerPrefsManager.SaveData(_holesToggleBool , HolesKey);
        }
        
        public void NumberOfPlayersToggle()
        {
            for(int i = 0; i < numberOfPlayersSelectionTogglesArray.Length; i++)
            {
                bool numberOfPlayersToggled = numberOfPlayersSelectionTogglesArray[i].isOn;
                _numberOfPlayersSelectionsBoolArray[i] = numberOfPlayersToggled;
                
                string[] numberOfPlayersKeys = new string[numberOfPlayersSelectionTogglesArray.Length];
                numberOfPlayersKeys[i] = "Number Of Players" + i;
                
                PlayerPrefsManager.SaveData(_numberOfPlayersSelectionsBoolArray , numberOfPlayersKeys);
            }
        }

        public void OfflineOnlineToggle()
        {
            string[] offlineOnlineKeys = new string[_offlineOnlineSelectionsBoolArray.Length];
            
            for(int i = 0; i < _offlineOnlineSelectionsBoolArray.Length; i++)
            {
                bool isOfflineOnline = offlineOnlineSelectionTogglesArray[i].isOn;
                _offlineOnlineSelectionsBoolArray[i] = isOfflineOnline;
            }
            
            PlayerPrefsManager.SaveData(_offlineOnlineSelectionsBoolArray , offlineOnlineKeys);
        }

        public void RandomTurnsToggle()
        {
            EventsManager.Invoke(Event.RandomTurnsToggled);
            _randomTurnsToggleBool = randomTurnsToggle.isOn;
            PlayerPrefsManager.SaveData(_randomTurnsToggleBool , RandomTurnsKey);
        }

        #endregion

        #region Events Related Functions
        
        private void OnGameOver()
        {
            coinUIObj.SetActive(false);
            continueButtonObj.SetActive(true);
            pauseButtonObj.SetActive(false);
        }

        private void OnGamePaused()
        {
            for(int i = 0; i < gameTitleTMPTexts.Length; i++)
            {
                gameTitleTMPTexts[i].enabled = true;
            }
        }

        private void OnGameResumed()
        {
            for(int i = 0; i < gameTitleTMPTexts.Length; i++)
            {
                gameTitleTMPTexts[i].enabled = false;
            }
        }

        private void OnGameTied()
        {
            int maxScore = _playerScoresArray.Max();
            List<int> tiedPlayers = new List<int>();

            for(int i = 0; i < _playerScoresArray.Length; i++)
            {
                if(_playerScoresArray[i] == maxScore)
                {
                    tiedPlayers.Add(i);
                }
            }
            
            foreach(int playerIndex in tiedPlayers)
            {
                _playersTotalWinsArray[playerIndex]++;
                playerTotalWinsLabelsTMPTexts[playerIndex].text = playerNameTMPInputFields[playerIndex].text + " Wins!!!";
                winsPanelObjs[playerIndex].SetActive(true);
            }
            
            gameTiedPanelObj.SetActive(true);
        }

        private void OnKeyboardTabPressed()
        {
            for(int i = 0; i < playerNameTMPInputFields.Length; i++)
            {
                if(playerNameTMPInputFields[i].isFocused)
                {
                    playerNameTMPInputFields[i].DeactivateInputField();
                    int nextIndex = (i + 1) % playerNameTMPInputFields.Length;
                    playerNameTMPInputFields[nextIndex].ActivateInputField();
                    break;
                }
            }
        }

        private void OnNumberOfPlayersToggled()
        {
            SetPlayersNumber();
        }

        private void OnPlayerOffline()
        {
            Debug.Log("Player Offline");
        }

        private void OnPlayerOnline()
        {
            Debug.Log("Player Online");
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
        }

        private void OnScoreUpdated(int[] coinScoresArray)
        {
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                coinScoreTMPTexts[i].text = _playerNamesArray[i] + " : " + coinScoresArray[i];
                _playerScoresArray = coinScoresArray;
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
                EventsManager.SubscribeToEvent(Event.GamePaused , new Action(OnGamePaused));
                EventsManager.SubscribeToEvent(Event.GameResumed , new Action(OnGameResumed));
                EventsManager.SubscribeToEvent(Event.GameTied , new Action(OnGameTied));
                EventsManager.SubscribeToEvent(Event.KeyboardTabPressed , new Action(OnKeyboardTabPressed));
                EventsManager.SubscribeToEvent(Event.NumberOfPlayersToggled , new Action(OnNumberOfPlayersToggled));
                EventsManager.SubscribeToEvent(Event.PlayerOffline , new Action(OnPlayerOffline));
                EventsManager.SubscribeToEvent(Event.PlayerOnline , new Action(OnPlayerOnline));
                EventsManager.SubscribeToEvent(Event.PlayerWins , (Action<int>)OnPlayerWins);
                EventsManager.SubscribeToEvent(Event.ScoreUpdated , (Action<int[]>)OnScoreUpdated);
                EventsManager.SubscribeToEvent(Event.PlayerTotalReceived , (Action<int[]>)OnTotalReceived);    
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.GameOver , new Action(OnGameOver));
                EventsManager.UnsubscribeFromEvent(Event.GamePaused , new Action(OnGamePaused));
                EventsManager.UnsubscribeFromEvent(Event.GameResumed , new Action(OnGameResumed));
                EventsManager.UnsubscribeFromEvent(Event.GameTied , new Action(OnGameTied));
                EventsManager.UnsubscribeFromEvent(Event.KeyboardTabPressed , new Action(OnKeyboardTabPressed));
                EventsManager.UnsubscribeFromEvent(Event.NumberOfPlayersToggled , new Action(OnNumberOfPlayersToggled));
                EventsManager.UnsubscribeFromEvent(Event.PlayerOffline , new Action(OnPlayerOffline));
                EventsManager.UnsubscribeFromEvent(Event.PlayerOnline , new Action(OnPlayerOnline));
                EventsManager.UnsubscribeFromEvent(Event.PlayerWins , (Action<int>)OnPlayerWins);
                EventsManager.UnsubscribeFromEvent(Event.ScoreUpdated , (Action<int[]>)OnScoreUpdated);
                EventsManager.UnsubscribeFromEvent(Event.PlayerTotalReceived , (Action<int[]>)OnTotalReceived);
            }
        }

        #endregion
    }
}