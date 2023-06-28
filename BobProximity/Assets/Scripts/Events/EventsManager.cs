using System;

namespace Events
{
    public class EventsManager
    {
        #region Actions
        
        protected static event Action GameOverAction;
        protected static event Action<int , string> PlayerNamesUpdatedAction;

        #endregion

        #region Subscribe Functions
		
        public static void SubscribeToEvent(Event gameEvent , Action actionFunction)
        {
            switch(gameEvent)
            {
                case Event.GameOver:
                    GameOverAction += actionFunction;
                break;
            }
        }
        
        public static void SubscribeToEvent(Event gameEvent , Action<int , string> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.PlayerNamesUpdated:
                    PlayerNamesUpdatedAction += actionFunction;
                break;
            }
        }

        #endregion
        
        #region Unsubscribe Functions

        public static void UnsubscribeFromEvent(Event gameEvent , Action actionFunction)
        {
            switch(gameEvent)
            {
                case Event.GameOver:
                    GameOverAction -= actionFunction;
                break;
            }
        }
        
        public static void UnsubscribeFromEvent(Event gameEvent , Action<int , string> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.PlayerNamesUpdated:
                    PlayerNamesUpdatedAction -= actionFunction;
                break;
            }
        }

        #endregion
        
        #region Invoke Functions

        public static void Invoke(Event gameEvent)
        {
            switch(gameEvent)
            {
                case Event.GameOver:
                    GameOverAction?.Invoke();
                break;
            }
        }
        public static void Invoke(Event gameEvent , int playerID , string playerName)
        {
            switch(gameEvent)
            {
                case Event.PlayerNamesUpdated:
                    PlayerNamesUpdatedAction?.Invoke(playerID , playerName);
                break;
            }
        }

        #endregion
    }
}