namespace Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Unity.Services.Core;
    using Unity.Services.Authentication;
    using Unity.Services.Lobbies;
    using Unity.Services.Lobbies.Models;
    using Unity.Services.Relay;
    using System.Linq;
    using UnityEngine;

    public class MultiplayerManager : MonoBehaviour
    {
        private const int MaxPlayers = 4;
        
        private readonly int _maxRetries = 5;
        private readonly int _baseDelayMs = 1000;

        private bool _isHost;
        private bool _isLeavingLobby;
        private bool _lobbyChecked;
        private Lobby _currentLobby;
        private string _lobbyName = "Lobby";
        private string _lobbyPlayerName;

        private async void Start()
        {
            await InitializeUnityServices();
            await CheckAndLeaveLobbyIfMember();
            await CreateLobbyIfNoneExists();
            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }
        
        private async void OnApplicationQuit()
        {
            await LeaveLobby();
        }
        
        private async Task CheckAndLeaveLobbyIfMember()
        {
            try
            {
                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                    {
                        new(QueryFilter.FieldOptions.Name , _lobbyName , QueryFilter.OpOptions.EQ)
                    }
                });
                
                foreach(var lobby in queryResponse.Results)
                {
                    if(lobby.Players.Any(player => player.Id == AuthenticationService.Instance.PlayerId))
                    {
                        _currentLobby = lobby;
                        await LeaveLobby();
                        _currentLobby = null;
                        Debug.Log("Left existing lobby on startup.");
                        break;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Error checking or leaving lobby: {e.Message}");
            }
        }
        
        private async Task CreateLobbyIfNoneExists()
        {
            int retryCount = 0;

            while(retryCount < _maxRetries)
            {
                try
                {
                    var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
                    {
                        Filters = new List<QueryFilter>
                        {
                            new(QueryFilter.FieldOptions.Name , _lobbyName , QueryFilter.OpOptions.EQ)
                        }
                    });

                    if(queryResponse.Results.Count == 0)
                    {
                        _isHost = true;
                        await CreateLobbyWithRelay();
                    }
                    else
                    {
                        Debug.Log("Lobby already exists; awaiting player to join.");
                    }

                    break;
                }
                catch(Exception e)
                {
                    if(e.Message.Contains("Rate limit" , StringComparison.OrdinalIgnoreCase))
                    {
                        retryCount++;
                        
                        if(retryCount >= _maxRetries)
                        {
                            Debug.LogError("Max retries reached. Could not create or find a lobby due to rate limits.");
                            break;
                        }

                        int delay = _baseDelayMs * (int)Math.Pow(2 , retryCount);
                        Debug.LogWarning($"Rate limit exceeded. Retrying in {delay / 1000} seconds... (Attempt {retryCount}/{_maxRetries})");
                        await Task.Delay(delay);
                    }
                    else
                    {
                        Debug.LogError($"Failed to create or check for the default lobby: {e.Message}");
                        break;
                    }
                }
            }
        }
        
        private async Task CreateLobbyWithRelay()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                if(string.IsNullOrEmpty(joinCode))
                {
                    Debug.LogError("Failed to get a valid join code from Relay.");
                    return;
                }

                Debug.Log($"Relay created with join code: {joinCode}");

                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName , MaxPlayers , new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "joinCode" , new DataObject(DataObject.VisibilityOptions.Public , joinCode) },
                        { "lobbyName" , new DataObject(DataObject.VisibilityOptions.Public , _lobbyName) }
                    }
                });

                if(_currentLobby == null)
                {
                    Debug.LogError("Lobby creation returned null.");
                }
                else
                {
                    Debug.Log($"Lobby created with ID: {_currentLobby.Id} and join code: {joinCode}");
                    await UpdateLobbyPlayerList();
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to create lobby with Relay: {e.Message}");
            }
        }

        private async Task InitializeUnityServices()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services Initialized Successfully.");

                if(!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log("User signed in anonymously");
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            }
        }

        private async Task LeaveLobby()
        {
            if(_currentLobby == null) return;
            if(_isLeavingLobby) return;

            _isLeavingLobby = true;

            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                Debug.Log($"Successfully left the lobby with ID: {_currentLobby.Id}");
                _currentLobby = null;
            }
            catch(LobbyServiceException e)
            {
                Debug.LogError($"Failed to leave the lobby: {e.Message}");
            }
            finally
            {
                _currentLobby = null;
                _isLeavingLobby = false;
            }
        }

        private async Task UpdateLobbyPlayerList()
        {
            if(_currentLobby == null || string.IsNullOrEmpty(_lobbyPlayerName)) return;

            var currentPlayerList = _currentLobby.Data.TryGetValue("playerList" , out var playerListData) ? playerListData.Value : "";

            var playerNames = currentPlayerList.Split(',');

            if(!playerNames.Any(name => name.Equals(_lobbyPlayerName, StringComparison.Ordinal)))
            {
                currentPlayerList += string.IsNullOrEmpty(currentPlayerList) ? _lobbyPlayerName : $" , {_lobbyPlayerName}";

                if(_isHost)
                {
                    await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id , new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { "playerList" , new DataObject(DataObject.VisibilityOptions.Public , currentPlayerList) }
                        }
                    });
                }

                EventsManager.Invoke(Event.PlayersListUpdated , new List<string>(currentPlayerList.Split(',')));
            }
        }

        private async void OnLobbyJoined()
        {
            if(string.IsNullOrEmpty(_lobbyPlayerName)) return;
            
            if(_currentLobby != null)
            {
                UpdateLobbyPlayerList();
                return;
            }
            
            await CreateLobbyIfNoneExists();
        }

        private async void OnLobbyLeft()
        {
            await LeaveLobby();
        }

        private void OnLobbyPlayerNameUpdated(string lobbyPlayerName)
        {
            _lobbyPlayerName = lobbyPlayerName;
        }

        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.LobbyJoin , new Action(OnLobbyJoined));
                EventsManager.SubscribeToEvent(Event.LobbyLeave , new Action(OnLobbyLeft));
                EventsManager.SubscribeToEvent(Event.LobbyPlayerNameUpdated , new Action<string>(OnLobbyPlayerNameUpdated));
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.LobbyJoin , new Action(OnLobbyJoined));
                EventsManager.UnsubscribeFromEvent(Event.LobbyLeave , new Action(OnLobbyLeft));
                EventsManager.UnsubscribeFromEvent(Event.LobbyPlayerNameUpdated , new Action<string>(OnLobbyPlayerNameUpdated));
            }
        }
    }
}