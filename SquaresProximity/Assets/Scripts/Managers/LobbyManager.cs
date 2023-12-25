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
            _gameManager = FindObjectOfType<GameManager>();
        }

        private void OnDestroy()
        {
            DeleteLobby();
        }

        private async void DeleteLobby()
        {
            if(!string.IsNullOrEmpty(_lobbyID))
            {
                try
                {
                    await Lobbies.Instance.DeleteLobbyAsync(_lobbyID);
                    Debug.Log($"Lobby with ID: {_lobbyID} deleted");
                    _lobbyID = null;
                    _playerID = null;
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to delete lobby: {e.Message}");
                }
            }
        }

        public async void CreateLobby()
        { 
            if(!string.IsNullOrEmpty(_lobbyID))
            {
                Debug.Log("Lobby already exists, attempting to join instead.");
                JoinLobby();
                return;
            }

            try
            {
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
            if(string.IsNullOrEmpty(_lobbyID))
            {
                Debug.LogError("No lobby ID found to join.");
                return;
            }

            try
            {
                Debug.Log($"Joined lobby with ID: {_lobbyID}");
                _playerID = AuthenticationService.Instance.PlayerId;
                EventsManager.Invoke(Event.PlayerJoinedLobby);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to join lobby: {e.Message}");
            }
        }

        public async void LeaveLobby()
        {
            if(string.IsNullOrEmpty(_lobbyID) || string.IsNullOrEmpty(_playerID))
            {
                Debug.LogError("No lobby or player ID found to leave.");
                return;
            }

            try
            {
                await Lobbies.Instance.RemovePlayerAsync(_lobbyID , _playerID);
                Debug.Log($"Left the lobby with ID: {_lobbyID}");
                EventsManager.Invoke(Event.PlayerLeftLobby);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to leave lobby: {e.Message}");
            }
        }
    }
}