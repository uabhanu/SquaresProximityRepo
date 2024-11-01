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
    using UnityEngine;
    
    public class MultiplayerManager : MonoBehaviour
    {
        private const int MaxPlayers = 4;

        private bool _isLeavingLobby;
        private Lobby _currentLobby;
        private string _lobbyName;

        private async void Start()
        {
            await InitializeUnityServices();
            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
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

        private async Task CreateLobbyWithRelay()
        {
            try
            {
                var allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                Debug.Log($"Relay created with join code: {joinCode}");
                
                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName , MaxPlayers , new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "joinCode" , new DataObject(DataObject.VisibilityOptions.Public , joinCode) },
                        { "lobbyName" , new DataObject(DataObject.VisibilityOptions.Public , _lobbyName) }
                    }
                });

                Debug.Log($"Lobby created with name: {_lobbyName} and ID: {_currentLobby.Id}");
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to create lobby with Relay: {e.Message}");
            }
        }
        
        private async Task JoinLobbyByName()
        {
            try
            {
                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                    {
                        new(field: QueryFilter.FieldOptions.Name ,  op: QueryFilter.OpOptions.EQ , value: _lobbyName)
                    }
                });
                
                if(queryResponse.Results.Count > 0)
                {
                    var lobby = queryResponse.Results[0];
                    Debug.Log($"Found lobby with name: {_lobbyName} , ID: {lobby.Id}");
                    await JoinLobbyWithRelay(lobby.Id);
                }
                else
                {
                    Debug.LogError($"No lobby found with the name: {_lobbyName}");
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to join lobby by name: {e.Message}");
            }
        }

        private async Task JoinLobbyWithRelay(string lobbyId)
        {
            try
            {
                _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
                Debug.Log($"Joined lobby with ID: {_currentLobby.Id}");
                
                if(_currentLobby.Data.TryGetValue("joinCode" , out var joinCodeData))
                {
                    var joinCode = joinCodeData.Value;
                    
                    await RelayService.Instance.JoinAllocationAsync(joinCode);
                    Debug.Log("Joined Relay server.");
                }
                else
                {
                    Debug.LogError("No Relay join code found in the lobby data.");
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to join lobby with Relay: {e.Message}");
            }
        }
        
        private async Task LeaveLobby()
        {
            if(_currentLobby == null)
            {
                Debug.LogWarning("No active lobby to leave.");
                return;
            }
    
            if(_isLeavingLobby)
            {
                Debug.LogWarning("Leave operation is already in progress.");
                return;
            }
    
            _isLeavingLobby = true;

            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                Debug.Log($"Successfully left the lobby with ID: {_currentLobby.Id}");
                _currentLobby = null;
            }
            catch(LobbyServiceException e)
            {
                if(e.Message.Contains("rate limit exceeded" , StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning("Rate limit exceeded while trying to leave the lobby. Please try again later.");
                }
                
                else if(e.Message.Contains("lobby not found" , StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning("Lobby not found on the server. Clearing local reference.");
                    _currentLobby = null;
                }
                
                else
                {
                    Debug.LogError($"Failed to leave the lobby: {e.Message}");
                }
            }
            finally
            {
                _isLeavingLobby = false;
            }
        }

        private async void OnLobbyCreated()
        {
            if(!string.IsNullOrEmpty(_lobbyName))
            {
                await CreateLobbyWithRelay();
            }
            else
            {
                Debug.LogError("Lobby name is empty.");
            }
        }
        
        private async void OnLobbyJoined()
        {
            if(!string.IsNullOrEmpty(_lobbyName))
            {
                await JoinLobbyByName();
            }
            else
            {
                Debug.LogError("Lobby name is empty.");
            }
        }
        
        private async void OnLobbyLeft()
        {
            await LeaveLobby();
        }

        private void OnLobbyNameUpdated(string lobbyName)
        {
            _lobbyName = lobbyName;
        }
        
        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Event.LobbyCreate , new Action(OnLobbyCreated));
                EventsManager.SubscribeToEvent(Event.LobbyJoin , new Action(OnLobbyJoined));
                EventsManager.SubscribeToEvent(Event.LobbyLeave , new Action(OnLobbyLeft));
                EventsManager.SubscribeToEvent(Event.LobbyNameUpdated , new Action<string>(OnLobbyNameUpdated));
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Event.LobbyCreate , new Action(OnLobbyCreated));
                EventsManager.UnsubscribeFromEvent(Event.LobbyJoin , new Action(OnLobbyJoined));
                EventsManager.UnsubscribeFromEvent(Event.LobbyLeave , new Action(OnLobbyLeft));
                EventsManager.UnsubscribeFromEvent(Event.LobbyNameUpdated , new Action<string>(OnLobbyNameUpdated));
            }
        }
    }
}