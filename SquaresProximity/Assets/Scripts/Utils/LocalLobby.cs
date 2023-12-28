namespace Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    
    [Flags]
    public enum LobbyState
    {
        Lobby = 1,
        CountDown = 2,
        InGame = 4
    }

    public enum LobbyColor
    {
        None = 0,
        Orange = 1,
        Green = 2,
        Blue = 3
    }
    
    [Serializable]
    public class LocalLobby
    {
        List<LocalPlayer> _localPlayers;
        private ServerAddress _relayServer;
        
        public Action<LocalPlayer> OnUserJoined;
        public Action<int> OnUserLeft;
        public Action<int> OnUserReadyChange;

        public CallbackValue<string> LobbyID = new CallbackValue<string>();
        public CallbackValue<string> LobbyCode = new CallbackValue<string>();
        public CallbackValue<string> RelayCode = new CallbackValue<string>();
        public CallbackValue<ServerAddress> RelayServer = new CallbackValue<ServerAddress>();
        public CallbackValue<string> LobbyName = new CallbackValue<string>();
        public CallbackValue<string> HostID = new CallbackValue<string>();
        public CallbackValue<LobbyState> LocalLobbyState = new CallbackValue<LobbyState>();
        public CallbackValue<bool> Locked = new CallbackValue<bool>();
        public CallbackValue<bool> Private = new CallbackValue<bool>();
        public CallbackValue<int> AvailableSlots = new CallbackValue<int>();
        public CallbackValue<int> MaxPlayerCount = new CallbackValue<int>();
        public CallbackValue<LobbyColor> LocalLobbyColor = new CallbackValue<LobbyColor>();
        public CallbackValue<long> LastUpdated = new CallbackValue<long>();
        public int PlayerCount => _localPlayers.Count;
        public List<LocalPlayer> LocalPlayers => _localPlayers;

        public void ResetLobby()
        {
            _localPlayers.Clear();

            LobbyName.Value = "";
            LobbyID.Value = "";
            LobbyCode.Value = "";
            Locked.Value = false;
            Private.Value = false;
            LocalLobbyColor.Value = LobbyColor.None;
            AvailableSlots.Value = 4;
            MaxPlayerCount.Value = 4;
            OnUserJoined = null;
            OnUserLeft = null;
        }

        public LocalLobby()
        {
            LastUpdated.Value = DateTime.Now.ToFileTimeUtc();
            HostID.OnChanged += OnHostChanged;
        }

        ~LocalLobby()
        {
            HostID.OnChanged -= OnHostChanged;
        }

        public LocalPlayer GetLocalPlayer(int index)
        {
            return PlayerCount > index ? _localPlayers[index] : null;
        }

        private void OnHostChanged(string newHostId)
        {
            foreach(var player in _localPlayers)
            {
                player.IsHost.Value = player.ID.Value == newHostId;
            }
        }
        
        public void AddPlayer(int index , LocalPlayer user)
        {
            _localPlayers.Insert(index, user);
            user.UserStatus.OnChanged += OnUserChangedStatus;
            OnUserJoined?.Invoke(user);
            Debug.Log($"Added User: {user.DisplayName.Value} - {user.ID.Value} to slot {index + 1}/{PlayerCount}");
        }

        public void RemovePlayer(int playerIndex)
        {
            _localPlayers[playerIndex].UserStatus.OnChanged -= OnUserChangedStatus;
            _localPlayers.RemoveAt(playerIndex);
            OnUserLeft?.Invoke(playerIndex);
        }

        void OnUserChangedStatus(PlayerStatus status)
        {
            int readyCount = 0;
            foreach (var player in _localPlayers)
            {
                if (player.UserStatus.Value == PlayerStatus.Ready)
                    readyCount++;
            }

            OnUserReadyChange?.Invoke(readyCount);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Lobby : ");
            sb.AppendLine(LobbyName.Value);
            sb.Append("ID: ");
            sb.AppendLine(LobbyID.Value);
            sb.Append("Code: ");
            sb.AppendLine(LobbyCode.Value);
            sb.Append("Locked: ");
            sb.AppendLine(Locked.Value.ToString());
            sb.Append("Private: ");
            sb.AppendLine(Private.Value.ToString());
            sb.Append("AvailableSlots: ");
            sb.AppendLine(AvailableSlots.Value.ToString());
            sb.Append("Max Players: ");
            sb.AppendLine(MaxPlayerCount.Value.ToString());
            sb.Append("LocalLobbyState: ");
            sb.AppendLine(LocalLobbyState.Value.ToString());
            sb.Append("Lobby LocalLobbyState Last Edit: ");
            sb.AppendLine(new DateTime(LastUpdated.Value).ToString());
            sb.Append("LocalLobbyColor: ");
            sb.AppendLine(LocalLobbyColor.Value.ToString());
            sb.Append("RelayCode: ");
            sb.AppendLine(RelayCode.Value);

            return sb.ToString();
        }
    }
}