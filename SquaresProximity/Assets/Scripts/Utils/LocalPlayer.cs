namespace Utils
{
    using System;
    
    [Flags]
    public enum PlayerStatus
    {
        None = 0,
        Connecting = 1,
        Lobby = 2,
        Ready = 4,
        InGame = 8,
        Menu = 16
    }
    
    [Serializable]
    public class LocalPlayer
    {
        public CallbackValue<bool> IsHost = new CallbackValue<bool>(false);
        public CallbackValue<string> DisplayName = new CallbackValue<string>("");
        public CallbackValue<EmoteType> Emote = new CallbackValue<EmoteType>(EmoteType.None);
        public CallbackValue<PlayerStatus> UserStatus = new CallbackValue<PlayerStatus>((PlayerStatus)0);
        public CallbackValue<string> ID = new CallbackValue<string>("");
        public CallbackValue<int> Index = new CallbackValue<int>(0);

        public DateTime LastUpdated;

        public LocalPlayer(string id , int index , bool isHost , string displayName = default , EmoteType emote = default , PlayerStatus status = default)
        {
            ID.Value = id;
            IsHost.Value = isHost;
            Index.Value = index;
            DisplayName.Value = displayName;
            Emote.Value = emote;
            UserStatus.Value = status;
        }

        public void ResetState()
        {
            IsHost.Value = false;
            Emote.Value = EmoteType.None;
            UserStatus.Value = PlayerStatus.Menu;
        }
    }
}