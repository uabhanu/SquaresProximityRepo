namespace Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Unity.Services.Authentication;
    using Unity.Services.Lobbies;
    using Unity.Services.Lobbies.Models;
    using UnityEngine;
    using Utils;
    
    public class LobbyManager : IDisposable
    {
        private Lobby _currentLobby;
        private LobbyEventCallbacks _lobbyEventCallbacks;
        private Task _heartBeatTask;
        
        private const int _maxLobbiesToShow = 16;
        private const string _keyRelayCode = nameof(LocalLobby.RelayCode);
        private const string _keyLobbyState = nameof(LocalLobby.LocalLobbyState);
        private const string _keyLobbyColor = nameof(LocalLobby.LocalLobbyColor);
        private const string _keyDisplayname = nameof(LocalPlayer.DisplayName);
        private const string _keyUserstatus = nameof(LocalPlayer.UserStatus);
        private const string _keyEmote = nameof(LocalPlayer.Emote);
        
        private ServiceRateLimiter m_QueryCooldown = new(1 , 1f);
        private ServiceRateLimiter m_CreateCooldown = new(2 , 6f);
        private ServiceRateLimiter m_JoinCooldown = new(2 , 6f);
        private ServiceRateLimiter m_QuickJoinCooldown = new(1 , 10f);
        private ServiceRateLimiter m_GetLobbyCooldown = new(1 , 1f);
        private ServiceRateLimiter m_DeleteLobbyCooldown = new(2 , 1f);
        private ServiceRateLimiter m_UpdateLobbyCooldown = new(5 , 5f);
        private ServiceRateLimiter m_UpdatePlayerCooldown = new(5 , 5f);
        private ServiceRateLimiter m_LeaveLobbyOrRemovePlayer = new(5 , 1);
        private ServiceRateLimiter m_HeartBeatCooldown = new(5 , 30);

        public Lobby CurrentLobby => _currentLobby;

        #region Rate Limiting

        public enum RequestType
        {
            Query = 0,
            Join,
            QuickJoin,
            Host
        }

        public bool InLobby()
        {
            if(_currentLobby == null)
            {
                Debug.LogWarning("LobbyManager not currently in a lobby. Did you CreateLobbyAsync or JoinLobbyAsync?");
                return false;
            }

            return true;
        }

        public ServiceRateLimiter GetRateLimit(RequestType type)
        {
            if(type == RequestType.Join) return m_JoinCooldown;

            if(type == RequestType.QuickJoin) return m_QuickJoinCooldown;
            
            if(type == RequestType.Host) return m_CreateCooldown;
            
            return m_QueryCooldown;
        }

        #endregion

        Dictionary<string , PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
        {
            Dictionary<string , PlayerDataObject> data = new Dictionary<string , PlayerDataObject>();
            var displayNameObject = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member , user.DisplayName.Value);
            data.Add("DisplayName", displayNameObject);
            return data;
        }

        public async Task<Lobby> CreateLobbyAsync(string lobbyName , int maxPlayers , bool isPrivate , LocalPlayer localUser , string password)
        {
            if(m_CreateCooldown.IsCoolingDown)
            {
                Debug.LogWarning("Create Lobby hit the rate limit.");
                return null;
            }

            await m_CreateCooldown.QueueUntilCooldown();

            string uasId = AuthenticationService.Instance.PlayerId;

            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = new Player(id: uasId , data: CreateInitialPlayerData(localUser)) , Password = password
            };
            
            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName , maxPlayers , createOptions);
            StartHeartBeat();

            return _currentLobby;
        }

        public async Task<Lobby> JoinLobbyAsync(string lobbyId , string lobbyCode , LocalPlayer localUser , string password = null)
        {
            if(m_JoinCooldown.IsCoolingDown || (lobbyId == null && lobbyCode == null))
            {
                return null;
            }

            await m_JoinCooldown.QueueUntilCooldown();

            string uasId = AuthenticationService.Instance.PlayerId;
            var playerData = CreateInitialPlayerData(localUser);

            if(!string.IsNullOrEmpty(lobbyId))
            {
                JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
                {
                    Player = new Player(id: uasId, data: playerData) , Password = password
                };
                
                _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId , joinOptions);
            }
            else
            {
                JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions
                {
                    Player = new Player(id: uasId , data: playerData) , Password = password
                };
                
                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode , joinOptions);
            }

            return _currentLobby;
        }

        public async Task<Lobby> QuickJoinLobbyAsync(LocalPlayer localUser , LobbyColor limitToColor = LobbyColor.None)
        {
            if(m_QuickJoinCooldown.IsCoolingDown)
            {
                Debug.LogWarning("Quick Join Lobby hit the rate limit.");
                return null;
            }

            await m_QuickJoinCooldown.QueueUntilCooldown();
            var filters = LobbyColorToFilters(limitToColor);
            string uasId = AuthenticationService.Instance.PlayerId;

            var joinRequest = new QuickJoinLobbyOptions
            {
                Filter = filters,
                Player = new Player(id: uasId , data: CreateInitialPlayerData(localUser))
            };

            return _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinRequest);
        }

        public async Task<QueryResponse> RetrieveLobbyListAsync(LobbyColor limitToColor = LobbyColor.None)
        {
            var filters = LobbyColorToFilters(limitToColor);

            if(m_QueryCooldown.TaskQueued) return null;
            
            await m_QueryCooldown.QueueUntilCooldown();

            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Count = _maxLobbiesToShow,
                Filters = filters
            };

            return await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
        }

        public async Task BindLocalLobbyToRemote(string lobbyID , LocalLobby localLobby)
        {
            _lobbyEventCallbacks.LobbyDeleted += async () =>
            {
                await LeaveLobbyAsync();
            };

            _lobbyEventCallbacks.DataChanged += changes =>
            {
                foreach(var change in changes)
                {
                    var changedValue = change.Value;
                    var changedKey = change.Key;

                    if(changedKey == _keyRelayCode) localLobby.RelayCode.Value = changedValue.Value.Value;

                    if(changedKey == _keyLobbyState) localLobby.LocalLobbyState.Value = (LobbyState)int.Parse(changedValue.Value.Value);

                    if(changedKey == _keyLobbyColor) localLobby.LocalLobbyColor.Value = (LobbyColor)int.Parse(changedValue.Value.Value);
                }
            };

            _lobbyEventCallbacks.DataAdded += changes =>
            {
                foreach(var change in changes)
                {
                    var changedValue = change.Value;
                    var changedKey = change.Key;

                    if(changedKey == _keyRelayCode) localLobby.RelayCode.Value = changedValue.Value.Value;

                    if(changedKey == _keyLobbyState) localLobby.LocalLobbyState.Value = (LobbyState)int.Parse(changedValue.Value.Value);

                    if(changedKey == _keyLobbyColor) localLobby.LocalLobbyColor.Value = (LobbyColor)int.Parse(changedValue.Value.Value);
                }
            };

            _lobbyEventCallbacks.DataRemoved += changes =>
            {
                foreach(var change in changes)
                {
                    var changedKey = change.Key;
                    
                    if(changedKey == _keyRelayCode) localLobby.RelayCode.Value = "";
                }
            };

            _lobbyEventCallbacks.PlayerLeft += players =>
            {
                foreach(var leftPlayerIndex in players)
                {
                    localLobby.RemovePlayer(leftPlayerIndex);
                }
            };

            _lobbyEventCallbacks.PlayerJoined += players =>
            {
                foreach(var playerChanges in players)
                {
                    Player joinedPlayer = playerChanges.Player;

                    var id = joinedPlayer.Id;
                    var index = playerChanges.PlayerIndex;
                    var isHost = localLobby.HostID.Value == id;

                    var newPlayer = new LocalPlayer(id , index , isHost);

                    foreach(var dataEntry in joinedPlayer.Data)
                    {
                        var dataObject = dataEntry.Value;
                        ParseCustomPlayerData(newPlayer , dataEntry.Key , dataObject.Value);
                    }

                    localLobby.AddPlayer(index , newPlayer);
                }
            };

            _lobbyEventCallbacks.PlayerDataChanged += changes =>
            {
                foreach(var lobbyPlayerChanges in changes)
                {
                    var playerIndex = lobbyPlayerChanges.Key;
                    var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                    
                    if(localPlayer == null) continue;
                    
                    var playerChanges = lobbyPlayerChanges.Value;
                    
                    foreach(var playerChange in playerChanges)
                    {
                        var changedValue = playerChange.Value;
                        var playerDataObject = changedValue.Value;
                        ParseCustomPlayerData(localPlayer , playerChange.Key , playerDataObject.Value);
                    }
                }
            };

            _lobbyEventCallbacks.PlayerDataAdded += changes =>
            {
                foreach(var lobbyPlayerChanges in changes)
                {
                    var playerIndex = lobbyPlayerChanges.Key;
                    var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                    
                    if(localPlayer == null) continue;
                    
                    var playerChanges = lobbyPlayerChanges.Value;
                    
                    foreach(var playerChange in playerChanges)
                    {
                        var changedValue = playerChange.Value;
                        var playerDataObject = changedValue.Value;
                        ParseCustomPlayerData(localPlayer , playerChange.Key , playerDataObject.Value);
                    }
                }
            };

            _lobbyEventCallbacks.PlayerDataRemoved += changes =>
            {
                foreach(var lobbyPlayerChanges in changes)
                {
                    var playerIndex = lobbyPlayerChanges.Key;
                    var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                    
                    if(localPlayer == null) continue;
                    
                    var playerChanges = lobbyPlayerChanges.Value;
                    
                    if(playerChanges == null) continue;

                    foreach(var playerChange in playerChanges.Values)
                    {
                        Debug.LogWarning("This Sample does not remove Player Values currently.");
                    }
                }
            };

            _lobbyEventCallbacks.LobbyChanged += async changes =>
            {
                if(changes.Name.Changed) localLobby.LobbyName.Value = changes.Name.Value;
                
                if(changes.HostId.Changed) localLobby.HostID.Value = changes.HostId.Value;
                
                if(changes.IsPrivate.Changed) localLobby.Private.Value = changes.IsPrivate.Value;
                
                if(changes.IsLocked.Changed) localLobby.Locked.Value = changes.IsLocked.Value;
                
                if(changes.AvailableSlots.Changed) localLobby.AvailableSlots.Value = changes.AvailableSlots.Value;
                
                if(changes.MaxPlayers.Changed) localLobby.MaxPlayerCount.Value = changes.MaxPlayers.Value;

                if(changes.LastUpdated.Changed) localLobby.LastUpdated.Value = changes.LastUpdated.Value.ToFileTimeUtc();

                if(changes.PlayerData.Changed) PlayerDataChanged();

                void PlayerDataChanged()
                {
                    foreach(var lobbyPlayerChanges in changes.PlayerData.Value)
                    {
                        var playerIndex = lobbyPlayerChanges.Key;
                        var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                        
                        if(localPlayer == null) continue;
                        var playerChanges = lobbyPlayerChanges.Value;
                        
                        if(playerChanges.ConnectionInfoChanged.Changed)
                        {
                            var connectionInfo = playerChanges.ConnectionInfoChanged.Value;
                            Debug.Log($"ConnectionInfo for player {playerIndex} changed to {connectionInfo}");
                        }

                        if(playerChanges.LastUpdatedChanged.Changed) { }
                    }
                }
            };

            _lobbyEventCallbacks.LobbyEventConnectionStateChanged += lobbyEventConnectionState =>
            {
                Debug.Log($"Lobby ConnectionState Changed to {lobbyEventConnectionState}");
            };

            _lobbyEventCallbacks.KickedFromLobby += () =>
            {
                Debug.Log("Left Lobby");
                Dispose();
            };
            
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyID , _lobbyEventCallbacks);
        }

        void ParseCustomPlayerData(LocalPlayer player , string dataKey , string playerDataValue)
        {
            if(dataKey == _keyEmote) player.Emote.Value = (EmoteType)int.Parse(playerDataValue);
            
            else if(dataKey == _keyUserstatus) player.UserStatus.Value = (PlayerStatus)int.Parse(playerDataValue);
            
            else if(dataKey == _keyDisplayname) player.DisplayName.Value = playerDataValue;
        }

        public async Task<Lobby> GetLobbyAsync(string lobbyId = null)
        {
            if(!InLobby()) return null;
            await m_GetLobbyCooldown.QueueUntilCooldown();
            lobbyId ??= _currentLobby.Id;
            return _currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
        }

        public async Task LeaveLobbyAsync()
        {
            await m_LeaveLobbyOrRemovePlayer.QueueUntilCooldown();
            
            if(!InLobby()) return;
            
            string playerId = AuthenticationService.Instance.PlayerId;

            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id , playerId);
            
            Dispose();
        }

        public async Task UpdatePlayerDataAsync(Dictionary<string, string> data)
        {
            if(!InLobby()) return;

            string playerId = AuthenticationService.Instance.PlayerId;
            Dictionary<string , PlayerDataObject> dataCurr = new Dictionary<string , PlayerDataObject>();
            
            foreach(var dataNew in data)
            {
                PlayerDataObject dataObj = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member , value: dataNew.Value);
                
                if(dataCurr.ContainsKey(dataNew.Key))
                    dataCurr[dataNew.Key] = dataObj;
                else
                    dataCurr.Add(dataNew.Key , dataObj);
            }

            if(m_UpdatePlayerCooldown.TaskQueued) return;
            
            await m_UpdatePlayerCooldown.QueueUntilCooldown();

            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = dataCurr,
                AllocationId = null,
                ConnectionInfo = null
            };
            
            _currentLobby = await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id , playerId , updateOptions);
        }

        public async Task UpdatePlayerRelayInfoAsync(string lobbyID , string allocationId , string connectionInfo)
        {
            if(!InLobby()) return;

            string playerId = AuthenticationService.Instance.PlayerId;

            if(m_UpdatePlayerCooldown.TaskQueued) return;
            
            await m_UpdatePlayerCooldown.QueueUntilCooldown();

            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>(),
                AllocationId = allocationId,
                ConnectionInfo = connectionInfo
            };
            
            _currentLobby = await LobbyService.Instance.UpdatePlayerAsync(lobbyID , playerId , updateOptions);
        }

        public async Task UpdateLobbyDataAsync(Dictionary<string , string> data)
        {
            if(!InLobby()) return;

            Dictionary<string , DataObject> dataCurr = _currentLobby.Data ?? new Dictionary<string , DataObject>();

            var shouldLock = false;
            
            foreach(var dataNew in data)
            {
                DataObject.IndexOptions index = dataNew.Key == "LocalLobbyColor" ? DataObject.IndexOptions.N1 : 0;
                DataObject dataObj = new DataObject(DataObject.VisibilityOptions.Public , dataNew.Value , index);
                
                if(dataCurr.ContainsKey(dataNew.Key))
                    dataCurr[dataNew.Key] = dataObj;
                else
                    dataCurr.Add(dataNew.Key , dataObj);
                
                if(dataNew.Key == "LocalLobbyState")
                {
                    Enum.TryParse(dataNew.Value, out LobbyState lobbyState);
                    shouldLock = lobbyState != LobbyState.Lobby;
                }
            }
            
            if(m_UpdateLobbyCooldown.TaskQueued) return;
            
            await m_UpdateLobbyCooldown.QueueUntilCooldown();

            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = dataCurr , IsLocked = shouldLock };
            _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id , updateOptions);
        }

        public async Task DeleteLobbyAsync()
        {
            if(!InLobby()) return;
            
            await m_DeleteLobbyCooldown.QueueUntilCooldown();
            await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
        }

        public void Dispose()
        {
            _currentLobby = null;
            _lobbyEventCallbacks = new LobbyEventCallbacks();
        }

        #region HeartBeat

        List<QueryFilter> LobbyColorToFilters(LobbyColor limitToColor)
        {
            List<QueryFilter> queryFiltersList = new List<QueryFilter>();
            
            if(limitToColor == LobbyColor.Orange)
                queryFiltersList.Add(new QueryFilter(QueryFilter.FieldOptions.N1 , ((int)LobbyColor.Orange).ToString() , QueryFilter.OpOptions.EQ));
            
            else if(limitToColor == LobbyColor.Green)
                queryFiltersList.Add(new QueryFilter(QueryFilter.FieldOptions.N1 , ((int)LobbyColor.Green).ToString() , QueryFilter.OpOptions.EQ));
            
            else if(limitToColor == LobbyColor.Blue)
                queryFiltersList.Add(new QueryFilter(QueryFilter.FieldOptions.N1 , ((int)LobbyColor.Blue).ToString() , QueryFilter.OpOptions.EQ));
            
            return queryFiltersList;
        }

        private async Task SendHeartbeatPingAsync()
        {
            if(!InLobby()) return;
            
            if(m_HeartBeatCooldown.IsCoolingDown) return;
            
            await m_HeartBeatCooldown.QueueUntilCooldown();
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }

        private void StartHeartBeat()
        {
            #pragma warning disable 4014
                _heartBeatTask = HeartBeatLoop();
            #pragma warning restore 4014
        }

        private async Task HeartBeatLoop()
        {
            while(_currentLobby != null)
            {
                await SendHeartbeatPingAsync();
                await Task.Delay(8000);
            }
        }

        #endregion
    }
    
    public class ServiceRateLimiter
    {
        private bool _coolingDown;
        private int _taskCounter;
        private readonly int _serviceCallTimes;
        
        public Action<bool> OnCooldownChange;
        public readonly int CoolDownMS;
        public bool TaskQueued { get; private set; }
        
        public ServiceRateLimiter(int callTimes , float coolDown , int pingBuffer = 100)
        {
            _serviceCallTimes = callTimes;
            _taskCounter = _serviceCallTimes;
            CoolDownMS = Mathf.CeilToInt(coolDown * 1000) + pingBuffer;
        }

        public async Task QueueUntilCooldown()
        {
            if(!_coolingDown)
            {
                #pragma warning disable 4014
                    ParallelCooldownAsync();
                #pragma warning restore 4014
            }

            _taskCounter--;

            if(_taskCounter > 0)
            {
                return;
            }

            if(!TaskQueued)
                TaskQueued = true;
            else
                return;

            while(_coolingDown)
            {
                await Task.Delay(10);
            }
        }

        private async Task ParallelCooldownAsync()
        {
            IsCoolingDown = true;
            await Task.Delay(CoolDownMS);
            IsCoolingDown = false;
            TaskQueued = false;
            _taskCounter = _serviceCallTimes;
        }

        public bool IsCoolingDown
        {
            get => _coolingDown;
            private set
            {
                if(_coolingDown != value)
                {
                    _coolingDown = value;
                    OnCooldownChange?.Invoke(_coolingDown);
                }
            }
        }
    }
}