using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
        private List<NetworkConnection> _playersNetworkConnectionsList;
        
        public static LobbyManager Instance;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void CreateLobby()
        {
            Debug.Log("Creating a new lobby...");
        }
        
        public void JoinLobby(int lobbyId)
        {
            Debug.Log($"Player joining lobby {lobbyId}...");
        }
        
        public void OnPlayerJoined(NetworkConnection conn)
        {
            _playersNetworkConnectionsList.Add(conn);
        }
        
        public void OnPlayerLeft(NetworkConnection conn)
        {
            _playersNetworkConnectionsList.Remove(conn);
        }
        
        public void StartGame()
        {
            Debug.Log("Starting game with players in lobby...");
        }
    }
}