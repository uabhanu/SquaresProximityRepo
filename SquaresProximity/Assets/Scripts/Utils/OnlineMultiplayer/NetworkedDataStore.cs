namespace Utils.OnlineMultiplayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.Netcode;
    using UnityEngine.Events;
    
    public class NetworkedDataStore : NetworkBehaviour
    {
        private Action<PlayerData> _onGetCurrentCallback;
        private Dictionary<ulong , PlayerData> _playerData;
        private ulong _localId;
        private UnityEvent<PlayerData> _onEachPlayerCallback;
        
        public static NetworkedDataStore Instance;

        public void Awake()
        {
            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            
            if(Instance == this) Instance = null;
        }
        
        [ClientRpc]
        private void GetAllPlayerData_ClientRpc(ulong callerId, PlayerData[] sortedData)
        {
            if (callerId != _localId)
                return;

            int rank = 1;
            foreach (var data in sortedData)
            {
                _onEachPlayerCallback.Invoke(data);
                rank++;
            }
            _onEachPlayerCallback = null;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetAllPlayerData_ServerRpc(ulong callerId)
        {
            var sortedData = _playerData.Select(kvp => kvp.Value).OrderByDescending(data => data.Score);
            GetAllPlayerData_ClientRpc(callerId , sortedData.ToArray());
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void GetPlayerData_ServerRpc(ulong id, ulong callerId)
        {
            if (_playerData.ContainsKey(id))
                GetPlayerData_ClientRpc(callerId, _playerData[id]);
            else
                GetPlayerData_ClientRpc(callerId, new PlayerData(null, 0));
        }
        
        public int UpdateScore(ulong id , int delta)
        {
            if(!IsServer) return int.MinValue;

            if(_playerData.ContainsKey(id))
            {
                _playerData[id].Score += delta;
                return _playerData[id].Score;
            }
            
            return int.MinValue;
        }

        public override void OnNetworkSpawn()
        {
            _localId = NetworkManager.Singleton.LocalClientId;
        }

        public void AddPlayer(ulong id , string name)
        {
            if(!IsServer) return;

            if(!_playerData.ContainsKey(id))
                _playerData.Add(id, new PlayerData(name , id , 0));
            else
                _playerData[id] = new PlayerData(name , id , 0);
        }
        
        public void GetAllPlayerData(UnityEvent<PlayerData> onEachPlayer)
        {
            _onEachPlayerCallback = onEachPlayer;
            GetAllPlayerData_ServerRpc(_localId);
        }
        
        public void GetPlayerData(ulong targetId , Action<PlayerData> onGet)
        {
            _onGetCurrentCallback = onGet;
            GetPlayerData_ServerRpc(targetId, _localId);
        }

        [ClientRpc]
        public void GetPlayerData_ClientRpc(ulong callerId , PlayerData data)
        {
            if(callerId == _localId)
            {   _onGetCurrentCallback?.Invoke(data);
                _onGetCurrentCallback = null;
            }
        }
    }
}