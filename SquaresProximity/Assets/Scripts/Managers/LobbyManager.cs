namespace Managers
{
    using System;
    using Unity.Services.Authentication;
    using Unity.Services.Lobbies;
    using UnityEngine;

    public class LobbyManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private string _lobbyID;
        private string _playerID;

        private void Awake()
        {
            Debug.Log("LobbyManager is waking up.");
            _gameManager = FindObjectOfType<GameManager>();
            Debug.Log("GameManager found: " + (_gameManager != null));
        }

        private void OnDestroy()
        {
            Debug.Log("LobbyManager is being destroyed. Attempting to delete lobby.");
            DeleteLobby();
        }

        private async void DeleteLobby()
        {
            Debug.Log($"Attempting to delete lobby: {_lobbyID}");
            
            if(!string.IsNullOrEmpty(_lobbyID))
            {
                try
                {
                    await Lobbies.Instance.DeleteLobbyAsync(_lobbyID);
                    Debug.Log($"Lobby with ID: {_lobbyID} deleted");
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to delete lobby: {e.Message}");
                }
            }
            else
            {
                Debug.Log("No lobby to delete (_lobbyID is null or empty).");
            }
        }

        public async void CreateLobby()
        {
            Debug.Log("CreateLobby called.");
            
            if(!string.IsNullOrEmpty(_lobbyID))
            {
                Debug.Log("Lobby already exists with ID: " + _lobbyID + " , attempting to join instead.");
                JoinLobby();
                return;
            }

            try
            {
                Debug.Log("Attempting to create a new lobby.");
                var lobby = await Lobbies.Instance.CreateLobbyAsync("Bhanu's Lobby" , _gameManager.NumberOfPlayers);
                _lobbyID = lobby.Id;
                Debug.Log($"Lobby created with ID: {_lobbyID}");
            }
            catch(LobbyServiceException e)
            {
                Debug.LogError($"Failed to create lobby: {e.Message}");
            }
        }

        public void JoinLobby()
        {
            Debug.Log($"JoinLobby called with current lobby ID: {_lobbyID}");
            
            if(string.IsNullOrEmpty(_lobbyID))
            {
                Debug.LogError("No lobby ID found to join.");
                return;
            }

            try
            {
                Debug.Log($"Attempting to join lobby with ID: {_lobbyID}");
                _playerID = AuthenticationService.Instance.PlayerId;
                Debug.Log($"Joined lobby with ID: {_lobbyID} as player {_playerID}");
                EventsManager.Invoke(Event.PlayerJoinedLobby);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to join lobby: {e.Message}");
            }
        }

        public async void LeaveLobby()
        {
            Debug.Log($"LeaveLobby called. Attempting to leave lobby: {_lobbyID} with player ID: {_playerID}");

            if(string.IsNullOrEmpty(_lobbyID) || string.IsNullOrEmpty(_playerID))
            {
                Debug.LogError("No lobby or player ID found to leave.");
                return;
            }

            try
            {
                Debug.Log($"Attempting to remove player {_playerID} from lobby {_lobbyID}.");
                await Lobbies.Instance.RemovePlayerAsync(_lobbyID , _playerID);
                Debug.Log($"Left the lobby with ID: {_lobbyID}");
                _playerID = null;
                EventsManager.Invoke(Event.PlayerLeftLobby);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to leave lobby: {e.Message}");
                Debug.LogError($"Current lobby ID: {_lobbyID} , Current player ID: {_playerID}");
                EventsManager.Invoke(Event.LobbyDeleted);
            }
        }
    }
}