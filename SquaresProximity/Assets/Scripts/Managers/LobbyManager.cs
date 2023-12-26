namespace Managers
{
    using Interfaces;
    using System;
    using System.Threading.Tasks;
    using Unity.Services.Authentication;
    using Unity.Services.Lobbies;
    using UnityEngine;

    public class LobbyManager : MonoBehaviour , ILobbyManager
    {
        #region Constructor
        
        public LobbyManager(InGameUIManager inGameUIManager)
        {
            _inGameUIManager = inGameUIManager;
        }
        
        #endregion
        
        #region Variables Declaration
        
        private InGameUIManager _inGameUIManager;
        private string _lobbyID;
        private string _playerID;
        
        #endregion
        
        #region Functions

        public async Task CreateLobby()
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
                var lobby = await Lobbies.Instance.CreateLobbyAsync("Bhanu's Lobby" , _inGameUIManager.NumberOfPlayers);
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

        public async Task LeaveLobby()
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
        
        #endregion
    }
}