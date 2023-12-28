namespace Utils
{
    using Managers;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Unity.Netcode;
    using Unity.Netcode.Transports.UTP;
    using Unity.Networking.Transport;
    using Unity.Services.Relay;
    using Unity.Services.Relay.Models;
    using UnityEngine;
    
    public class SetupInGame : MonoBehaviour
    {
        private bool _doesNeedCleanup;
        private bool _hasConnectedViaNGO;
        private LocalLobby _localLobby;
        private InGameRunner _inGameRunner;
        
        [SerializeField] private GameObject ingameRunnerPrefab;
        [SerializeField] private GameObject[] disableGameObjectsArray;

        private void SetMenuVisibility(bool areVisible)
        {
            foreach(GameObject go in disableGameObjectsArray)
                go.SetActive(areVisible);
        }
        
        async Task CreateNetworkManager(LocalLobby localLobby , LocalPlayer localPlayer)
        {
            _localLobby = localLobby;
            _inGameRunner = Instantiate(ingameRunnerPrefab).GetComponentInChildren<InGameRunner>();
            _inGameRunner.Initialize(OnConnectionVerified , _localLobby.PlayerCount , OnGameBegin , OnGameEnd, localPlayer);
            
            if(localPlayer.IsHost.Value)
            {
                await SetRelayHostData();
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                await AwaitRelayCode(localLobby);
                await SetRelayClientData();
                NetworkManager.Singleton.StartClient();
            }
        }

        async Task AwaitRelayCode(LocalLobby lobby)
        {
            string relayCode = lobby.RelayCode.Value;
            lobby.RelayCode.OnChanged += (code) => relayCode = code;
            
            while(string.IsNullOrEmpty(relayCode))
            {
                await Task.Delay(100);
            }
        }

        async Task SetRelayHostData()
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

            var allocation = await Relay.Instance.CreateAllocationAsync(_localLobby.MaxPlayerCount.Value);
            var joincode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            OnlineMultiplayerUIManager.Instance.HostSetRelayCode(joincode);

            bool isSecure;
            var endpoint = GetEndpointForAllocation(allocation.ServerEndpoints , allocation.RelayServer.IpV4 , allocation.RelayServer.Port , out isSecure);
            transport.SetHostRelayData(AddressFromEndpoint(endpoint) , endpoint.Port , allocation.AllocationIdBytes , allocation.Key , allocation.ConnectionData , isSecure);
        }

        async Task SetRelayClientData()
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

            var joinAllocation = await Relay.Instance.JoinAllocationAsync(_localLobby.RelayCode.Value);
            bool isSecure;
            
            var endpoint = GetEndpointForAllocation(joinAllocation.ServerEndpoints , joinAllocation.RelayServer.IpV4 , joinAllocation.RelayServer.Port , out isSecure);
            transport.SetClientRelayData(AddressFromEndpoint(endpoint) , endpoint.Port , joinAllocation.AllocationIdBytes , joinAllocation.Key , joinAllocation.ConnectionData , joinAllocation.HostConnectionData , isSecure);
        }
        
        NetworkEndpoint GetEndpointForAllocation(List<RelayServerEndpoint> endpoints , string ip , int port , out bool isSecure)
        {
            #if ENABLE_MANAGED_UNITYTLS
                foreach(RelayServerEndpoint endpoint in endpoints)
                {
                    if(endpoint.Secure && endpoint.Network == RelayServerEndpoint.NetworkOptions.Udp)
                    {
                        isSecure = true;
                        return NetworkEndpoint.Parse(endpoint.Host , (ushort)endpoint.Port);
                    }
                }
            #endif
            
            isSecure = false;
            return NetworkEndpoint.Parse(ip , (ushort)port);
        }

        string AddressFromEndpoint(NetworkEndpoint endpoint)
        {
            return endpoint.Address.Split(':')[0];
        }

        void OnConnectionVerified()
        {
            _hasConnectedViaNGO = true;
        }

        public void StartNetworkedGame(LocalLobby localLobby, LocalPlayer localPlayer)
        {
            _doesNeedCleanup = true;
            SetMenuVisibility(false);
            #pragma warning disable 4014
                CreateNetworkManager(localLobby , localPlayer);
            #pragma warning restore 4014
        }

        public void OnGameBegin()
        {
            if(!_hasConnectedViaNGO)
            {
                LogHandlerSettings.Instance.SpawnErrorPopup("Failed to join the game.");
                OnGameEnd();
            }
        }
        
        public void OnGameEnd()
        {
            if(_doesNeedCleanup)
            {
                NetworkManager.Singleton.Shutdown(true);
                Destroy(_inGameRunner.transform.parent.gameObject);
                SetMenuVisibility(true);
                _localLobby.RelayCode.Value = "";
                OnlineMultiplayerUIManager.Instance.EndGame();
                _doesNeedCleanup = false;
            }
        }
    }
}