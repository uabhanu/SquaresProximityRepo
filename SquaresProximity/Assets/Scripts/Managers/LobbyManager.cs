namespace Managers
{
    using Unity.Netcode;
    using Unity.Services.Authentication;
    using Unity.Services.Core;
    using Unity.Services.Lobbies;
    using Unity.Services.Lobbies.Models;
    using UnityEngine;
    
    public class LobbyManager : MonoBehaviour
    {
        private Lobby _joinedLobby;

        private void Awake()
        {
            InitializeUnityServices();
        }

        private async void InitializeUnityServices()
        {
            if(UnityServices.State != ServicesInitializationState.Initialized)
            {
                InitializationOptions initOptions = new InitializationOptions();
                initOptions.SetProfile(Random.Range(0 , 10000).ToString());
                await UnityServices.InitializeAsync(initOptions);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        public async void CreateLobby(string lobbyName , bool isPrivate , int numberOfPlayers)
        {
            try
            {
                _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName , numberOfPlayers , new CreateLobbyOptions
                {
                    IsPrivate = isPrivate
                });

                NetworkManager.Singleton.StartHost();
            }
            catch(LobbyServiceException lse)
            {
                Debug.Log(lse);
                throw;
            }
        }

        public async void QuickJoin()
        {
            try
            {
                _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                NetworkManager.Singleton.StartClient();
            }
            catch(LobbyServiceException lse)
            {
                Debug.Log(lse);
                throw;
            }
        }
    }
}
