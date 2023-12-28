using Utils;

namespace Managers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Unity.Services.Authentication;
    using Unity.Services.Lobbies;
    using UnityEngine;
    
    [Flags]
    public enum GameState
    {
        Menu = 1,
        Lobby = 2,
        JoinMenu = 4,
    }
    
    public class OnlineMultiplayerUIManager : MonoBehaviour
    {
        private LobbyColor _lobbyColorFilter;
        private LocalPlayer _localPlayer;
        private LocalLobby _localLobby;
        private VivoxSetup _vivoxSetup = new VivoxSetup();
        private static OnlineMultiplayerUIManager _onlineMultiplayerUIManager;
        
        [SerializeField] Countdown m_countdown;
        [SerializeField] List<VivoxUserHandler> vivoxUserHandlersList;
        [SerializeField] SetupInGame m_setupInGame;
        
        public Action<GameState> OnGameStateChanged;
        public LocalLobby LocalLobby => _localLobby;
        public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();
        public GameState LocalGameState { get; private set; }
        public LobbyManager LobbyManager { get; private set; }

        public static OnlineMultiplayerUIManager Instance
        {
            get
            {
                if(_onlineMultiplayerUIManager != null) return _onlineMultiplayerUIManager;
                _onlineMultiplayerUIManager = FindObjectOfType<OnlineMultiplayerUIManager>();
                return _onlineMultiplayerUIManager;
            }
        }
        
        public void SetLobbyColorFilter(int color)
        {
            _lobbyColorFilter = (LobbyColor)color;
        }

        public async Task<LocalPlayer> AwaitLocalUserInitialization()
        {
            while(_localPlayer == null)
                await Task.Delay(100);
            return _localPlayer;
        }

        public async void CreateLobby(string name , bool isPrivate , string password = null , int maxPlayers = 4)
        {
            try
            {
                var lobby = await LobbyManager.CreateLobbyAsync(name , maxPlayers , isPrivate , _localPlayer , password);
                LobbyConverters.RemoteToLocal(lobby , _localLobby);
                await CreateLobby();
            }
            catch(LobbyServiceException exception)
            {
                SetGameState(GameState.JoinMenu);
                LogHandlerSettings.Instance.SpawnErrorPopup($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
            }
        }

        public async void JoinLobby(string lobbyID, string lobbyCode, string password = null)
        {
            try
            {
                var lobby = await LobbyManager.JoinLobbyAsync(lobbyID , lobbyCode , _localPlayer , password:password);
                LobbyConverters.RemoteToLocal(lobby , _localLobby);
                await JoinLobby();
            }
            catch(LobbyServiceException exception)
            {
                SetGameState(GameState.JoinMenu);
                LogHandlerSettings.Instance.SpawnErrorPopup($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
            }
        }

        public async void QueryLobbies()
        {
            LobbyList.QueryState.Value = LobbyQueryState.Fetching;
            
            var qr = await LobbyManager.RetrieveLobbyListAsync(_lobbyColorFilter);
            
            if(qr == null)
            {
                return;
            }

            SetCurrentLobbies(LobbyConverters.QueryToLocalList(qr));
        }

        public async void QuickJoin()
        {
            var lobby = await LobbyManager.QuickJoinLobbyAsync(_localPlayer , _lobbyColorFilter);
            
            if(lobby != null)
            {
                LobbyConverters.RemoteToLocal(lobby , _localLobby);
                await JoinLobby();
            }
            else
            {
                SetGameState(GameState.JoinMenu);
            }
        }

        public void SetLocalUserName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                LogHandlerSettings.Instance.SpawnErrorPopup("Empty Name not allowed.");
                return;
            }

            _localPlayer.DisplayName.Value = name;
            SendLocalUserData();
        }

        public void SetLocalUserEmote(EmoteType emote)
        {
            _localPlayer.Emote.Value = emote;
            SendLocalUserData();
        }

        public void SetLocalUserStatus(PlayerStatus status)
        {
            _localPlayer.UserStatus.Value = status;
            SendLocalUserData();
        }

        public void SetLocalLobbyColor(int color)
        {
            if(_localLobby.PlayerCount < 1) return;
            _localLobby.LocalLobbyColor.Value = (LobbyColor)color;
            SendLocalLobbyData();
        }

        bool updatingLobby;

        async void SendLocalLobbyData()
        {
            await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(_localLobby));
        }

        async void SendLocalUserData()
        {
            await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localPlayer));
        }

        public void UIChangeMenuState(GameState state)
        {
            var isQuittingGame = LocalGameState == GameState.Lobby && _localLobby.LocalLobbyState.Value == LobbyState.InGame;

            if(isQuittingGame)
            {
                state = GameState.Lobby;
                ClientQuitGame();
            }
            
            SetGameState(state);
        }

        public void HostSetRelayCode(string code)
        {
            _localLobby.RelayCode.Value = code;
            SendLocalLobbyData();
        }
        
        void OnPlayersReady(int readyCount)
        {
            if(readyCount == _localLobby.PlayerCount && _localLobby.LocalLobbyState.Value != LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.CountDown;
                SendLocalLobbyData();
            }
            
            else if(_localLobby.LocalLobbyState.Value == LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
                SendLocalLobbyData();
            }
        }

        void OnLobbyStateChanged(LobbyState state)
        {
            if(state == LobbyState.Lobby) CancelCountDown();
            
            if(state == LobbyState.CountDown) BeginCountDown();
        }

        void BeginCountDown()
        {
            Debug.Log("Beginning Countdown.");
            m_countdown.StartCountDown();
        }

        void CancelCountDown()
        {
            Debug.Log("Countdown Cancelled.");
            m_countdown.CancelCountDown();
        }

        public void FinishedCountDown()
        {
            _localPlayer.UserStatus.Value = PlayerStatus.InGame;
            _localLobby.LocalLobbyState.Value = LobbyState.InGame;
            m_setupInGame.StartNetworkedGame(_localLobby , _localPlayer);
        }

        public void BeginGame()
        {
            if(_localPlayer.IsHost.Value)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.InGame;
                _localLobby.Locked.Value = true;
                SendLocalLobbyData();
            }
        }

        public void ClientQuitGame()
        {
            EndGame();
            m_setupInGame?.OnGameEnd();
        }

        public void EndGame()
        {
            if(_localPlayer.IsHost.Value)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
                _localLobby.Locked.Value = false;
                SendLocalLobbyData();
            }

            SetLobbyView();
        }

        #region Setup

        private async void Awake()
        {
            Application.wantsToQuit += OnWantToQuit;
            _localPlayer = new LocalPlayer("" , 0 , false , "LocalPlayer");
            _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
            LobbyManager = new LobbyManager();

            await InitializeServices();
            AuthenticatePlayer();
            StartVivoxLogin();
        }

        private async Task InitializeServices()
        {
            string serviceProfileName = "player";
            
            #if UNITY_EDITOR
                serviceProfileName = $"{serviceProfileName}{LocalProfileTool.LocalProfileSuffix}";
            #endif
            
            await UnityServiceAuthenticator.TrySignInAsync(serviceProfileName);
        }

        private void AuthenticatePlayer()
        {
            var localId = AuthenticationService.Instance.PlayerId;
            var randomName = NameGenerator.GetName(localId);

            _localPlayer.ID.Value = localId;
            _localPlayer.DisplayName.Value = randomName;
        }

        #endregion

        private void SetGameState(GameState state)
        {
            var isLeavingLobby = (state == GameState.Menu || state == GameState.JoinMenu) && LocalGameState == GameState.Lobby;
            
            LocalGameState = state;

            Debug.Log($"Switching Game State to : {LocalGameState}");

            if(isLeavingLobby) LeaveLobby();
            
            OnGameStateChanged.Invoke(LocalGameState);
        }

        private void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
        {
            var newLobbyDict = new Dictionary<string, LocalLobby>();
            
            foreach(var lobby in lobbies)
                newLobbyDict.Add(lobby.LobbyID.Value, lobby);

            LobbyList.CurrentLobbiesDictionary = newLobbyDict;
            LobbyList.QueryState.Value = LobbyQueryState.Fetched;
        }

        private async Task CreateLobby()
        {
            _localPlayer.IsHost.Value = true;
            _localLobby.OnUserReadyChange = OnPlayersReady;
            
            try
            {
                await BindLobby();
            }
            catch (LobbyServiceException exception)
            {
                SetGameState(GameState.JoinMenu);
                LogHandlerSettings.Instance.SpawnErrorPopup($"Couldn't join Lobby : ({exception.ErrorCode}) {exception.Message}");
            }
        }

        private async Task JoinLobby()
        {
            _localPlayer.IsHost.ForceSet(false);
            await BindLobby();
        }

        private async Task BindLobby()
        {
            await LobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value , _localLobby);
            _localLobby.LocalLobbyState.OnChanged += OnLobbyStateChanged;
            SetLobbyView();
            StartVivoxJoin();
        }

        public void LeaveLobby()
        {
            _localPlayer.ResetState();
            
            #pragma warning disable 4014
                LobbyManager.LeaveLobbyAsync();
            #pragma warning restore 4014
            
            ResetLocalLobby();
            _vivoxSetup.LeaveLobbyChannel();
            LobbyList.Clear();
        }

        private void StartVivoxLogin()
        {
            _vivoxSetup.Initialize(vivoxUserHandlersList , OnVivoxLoginComplete);

            void OnVivoxLoginComplete(bool didSucceed)
            {
                if(!didSucceed)
                {
                    Debug.LogError("Vivox login failed! Retrying in 5s...");
                    StartCoroutine(RetryConnection(StartVivoxLogin , _localLobby.LobbyID.Value));
                }
            }
        }

        private void StartVivoxJoin()
        {
            _vivoxSetup.JoinLobbyChannel(_localLobby.LobbyID.Value , OnVivoxJoinComplete);

            void OnVivoxJoinComplete(bool didSucceed)
            {
                if(!didSucceed)
                {
                    Debug.LogError("Vivox connection failed! Retrying in 5s...");
                    StartCoroutine(RetryConnection(StartVivoxJoin , _localLobby.LobbyID.Value));
                }
            }
        }

        private IEnumerator RetryConnection(Action doConnection, string lobbyId)
        {
            yield return new WaitForSeconds(5);
            if(_localLobby != null && _localLobby.LobbyID.Value == lobbyId && !string.IsNullOrEmpty(lobbyId)) doConnection?.Invoke();
        }

        private void SetLobbyView()
        {
            Debug.Log($"Setting Lobby user state {GameState.Lobby}");
            SetGameState(GameState.Lobby);
            SetLocalUserStatus(PlayerStatus.Lobby);
        }

        private void ResetLocalLobby()
        {
            _localLobby.ResetLobby();
            _localLobby.RelayServer = null;
        }

        #region Teardown
        
        private IEnumerator LeaveBeforeQuit()
        {
            ForceLeaveAttempt();
            yield return null;
            Application.Quit();
        }

        private bool OnWantToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(_localLobby?.LobbyID.Value);
            StartCoroutine(LeaveBeforeQuit());
            return canQuit;
        }

        private void OnDestroy()
        {
            ForceLeaveAttempt();
            LobbyManager.Dispose();
        }

        private void ForceLeaveAttempt()
        {
            if(!string.IsNullOrEmpty(_localLobby?.LobbyID.Value))
            {
                #pragma warning disable 4014
                    LobbyManager.LeaveLobbyAsync();
                #pragma warning restore 4014
                
                _localLobby = null;
            }
        }

        #endregion
    }
}