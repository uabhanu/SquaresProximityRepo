namespace Utils
{
    using System;
    using System.Collections.Generic;
    
    public enum LobbyQueryState
    {
        Empty,
        Fetching,
        Error,
        Fetched
    }
    
    [Serializable]
    public class LocalLobbyList
    {
        private Dictionary<string , LocalLobby> _currentLobbiesDictionary;
        
        public Action<Dictionary<string , LocalLobby>> OnLobbyListChange;
        public CallbackValue<LobbyQueryState> QueryState;
        
        public Dictionary<string , LocalLobby> CurrentLobbiesDictionary
        {
            get { return _currentLobbiesDictionary; }
            
            set
            {
                _currentLobbiesDictionary = value;
                OnLobbyListChange?.Invoke(_currentLobbiesDictionary);
            }
        }

        public void Clear()
        {
            CurrentLobbiesDictionary = new Dictionary<string , LocalLobby>();
            QueryState.Value = LobbyQueryState.Fetched;
        }
    }
}