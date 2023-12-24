namespace Managers
{
    using UnityEngine;
    
    public class LobbyManager : MonoBehaviour
    {
        public LobbyManager(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        private GameManager _gameManager;
        
        public void CreateLobby(int maxPlayers)
        {
            maxPlayers = _gameManager.NumberOfPlayers;
            Debug.Log($"Creating a new lobby with max players: {maxPlayers}");
        }

        public void JoinLobby(int lobbyId)
        {
            Debug.Log($"Joining lobby with ID: {lobbyId}");
        }

        public void LeaveLobby()
        {
            Debug.Log("Leaving lobby");
        }
    }
}