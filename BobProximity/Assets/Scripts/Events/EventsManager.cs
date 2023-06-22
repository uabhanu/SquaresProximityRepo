using System;

namespace Events
{
    public class EventsManager
    {
        #region Actions
        
        protected static event Action GameOverAction;

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

        #endregion
    }
}