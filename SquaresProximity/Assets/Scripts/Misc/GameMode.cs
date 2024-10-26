namespace Misc
{
    using Managers;
    
    public class GameMode
    {
        private bool _isOnlineMode;

        public bool IsOnlineMode
        {
            get => _isOnlineMode;
            set
            {
                if(_isOnlineMode != value)
                {
                    _isOnlineMode = value;
                    EventsManager.Invoke(Event.GameModeChanged , _isOnlineMode);
                }
            }
        }
    }
}