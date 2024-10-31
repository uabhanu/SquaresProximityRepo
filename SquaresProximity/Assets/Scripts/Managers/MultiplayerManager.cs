namespace Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TMPro;
    using Unity.Services.Core;
    using Unity.Services.Authentication;
    using Unity.Services.Lobbies;
    using Unity.Services.Lobbies.Models;
    using Unity.Services.Relay;
    using UnityEngine;
    
    public class MultiplayerManager : MonoBehaviour
    {
        private const int MaxPlayers = 4;
        private Lobby _currentLobby;
        private string _lobbyName;
        
        [SerializeField] private TMP_InputField lobbyNameInputField;

        private async void Start()
        {
            await InitializeUnityServices();
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
            catch (Exception e)
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
            try
            {
                if(_currentLobby != null)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                    Debug.Log("Left the lobby.");
                    _currentLobby = null;
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to leave lobby: {e.Message}");
            }
        }
        
        public async void CreateLobbyButton()
        {
            _lobbyName = lobbyNameInputField != null ? lobbyNameInputField.text : "DefaultLobbyName";
            await CreateLobbyWithRelay();
        }

        public async void JoinLobbyButton()
        {
            if(lobbyNameInputField != null && !string.IsNullOrEmpty(lobbyNameInputField.text))
            {
                _lobbyName = lobbyNameInputField.text;
                await JoinLobbyByName();
            }
            else
            {
                Debug.LogError("Lobby name input field is empty or not assigned.");
            }
        }

        public async void LeaveLobbyButton()
        {
            await LeaveLobby();
        }

        public void UpdateValue()
        {
            _lobbyName = lobbyNameInputField.text;
        }
    }
}