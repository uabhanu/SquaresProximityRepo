namespace Utils
{
    using System.Collections.Generic;
    using Unity.Services.Lobbies.Models;
    using UnityEngine;
    
    public static class LobbyConverters
    {
        private const string _keyRelayCode = nameof(LocalLobby.RelayCode);
        private const string _keyLobbyState = nameof(LocalLobby.LocalLobbyState);
        private const string _keyLobbyColor = nameof(LocalLobby.LocalLobbyColor);
        private const string _keyLastEdit = nameof(LocalLobby.LastUpdated);
        private const string _keyDisplayname = nameof(LocalPlayer.DisplayName);
        private const string _keyUserstatus = nameof(LocalPlayer.UserStatus);
        private const string _keyEmote = nameof(LocalPlayer.Emote);
        
        private static LocalLobby RemoteToNewLocal(Lobby lobby)
        {
            LocalLobby data = new LocalLobby();
            RemoteToLocal(lobby , data);
            return data;
        }

        public static Dictionary<string , string> LocalToRemoteLobbyData(LocalLobby lobby)
        {
            Dictionary<string , string> data = new Dictionary<string , string>();
            data.Add(_keyRelayCode , lobby.RelayCode.Value);
            data.Add(_keyLobbyState , ((int)lobby.LocalLobbyState.Value).ToString());
            data.Add(_keyLobbyColor , ((int)lobby.LocalLobbyColor.Value).ToString());
            data.Add(_keyLastEdit , lobby.LastUpdated.Value.ToString());

            return data;
        }
        
        public static List<LocalLobby> QueryToLocalList(QueryResponse response)
        {
            List<LocalLobby> retLst = new List<LocalLobby>();
            
            foreach(var lobby in response.Results)
                retLst.Add(RemoteToNewLocal(lobby));
            
            return retLst;
        }

        public static Dictionary<string , string> LocalToRemoteUserData(LocalPlayer user)
        {
            Dictionary<string , string> data = new Dictionary<string , string>();
            
            if(user == null || string.IsNullOrEmpty(user.ID.Value)) return data;
            
            data.Add(_keyDisplayname , user.DisplayName.Value);
            data.Add(_keyUserstatus , ((int)user.UserStatus.Value).ToString());
            data.Add(_keyEmote , ((int)user.Emote.Value).ToString());
            
            return data;
        }
        
        public static void RemoteToLocal(Lobby remoteLobby , LocalLobby localLobby)
        {
            if(remoteLobby == null)
            {
                Debug.LogError("Remote lobby is null , cannot convert.");
                return;
            }

            if(localLobby == null)
            {
                Debug.LogError("Local Lobby is null , cannot convert");
                return;
            }

            localLobby.LobbyID.Value = remoteLobby.Id;
            localLobby.HostID.Value = remoteLobby.HostId;
            localLobby.LobbyName.Value = remoteLobby.Name;
            localLobby.LobbyCode.Value = remoteLobby.LobbyCode;
            localLobby.Private.Value = remoteLobby.IsPrivate;
            localLobby.AvailableSlots.Value = remoteLobby.AvailableSlots;
            localLobby.MaxPlayerCount.Value = remoteLobby.MaxPlayers;
            localLobby.LastUpdated.Value = remoteLobby.LastUpdated.ToFileTimeUtc();
            
            localLobby.RelayCode.Value = remoteLobby.Data?.ContainsKey(_keyRelayCode) == true ? remoteLobby.Data[_keyRelayCode].Value : localLobby.RelayCode.Value;
            localLobby.LocalLobbyState.Value = remoteLobby.Data?.ContainsKey(_keyLobbyState) == true ? (LobbyState)int.Parse(remoteLobby.Data[_keyLobbyState].Value) : LobbyState.Lobby;
            localLobby.LocalLobbyColor.Value = remoteLobby.Data?.ContainsKey(_keyLobbyColor) == true ? (LobbyColor)int.Parse(remoteLobby.Data[_keyLobbyColor].Value) : LobbyColor.None;
            
            List<string> remotePlayerIDsList = new List<string>();
            int index = 0;
            
            foreach(var player in remoteLobby.Players)
            {
                var id = player.Id;
                remotePlayerIDsList.Add(id);
                var isHost = remoteLobby.HostId.Equals(player.Id);
                var displayName = player.Data?.ContainsKey(_keyDisplayname) == true ? player.Data[_keyDisplayname].Value : default;
                var emote = player.Data?.ContainsKey(_keyEmote) == true ? (EmoteType)int.Parse(player.Data[_keyEmote].Value) : EmoteType.None;
                var userStatus = player.Data?.ContainsKey(_keyUserstatus) == true ? (PlayerStatus)int.Parse(player.Data[_keyUserstatus].Value) : PlayerStatus.Lobby;

                LocalPlayer localPlayer = localLobby.GetLocalPlayer(index);

                if(localPlayer == null)
                {
                    localPlayer = new LocalPlayer(id , index , isHost , displayName , emote , userStatus);
                    localLobby.AddPlayer(index , localPlayer);
                }
                else
                {
                    localPlayer.ID.Value = id;
                    localPlayer.Index.Value = index;
                    localPlayer.IsHost.Value = isHost;
                    localPlayer.DisplayName.Value = displayName;
                    localPlayer.Emote.Value = emote;
                    localPlayer.UserStatus.Value = userStatus;
                }

                index++;
            }
        }
    }
}