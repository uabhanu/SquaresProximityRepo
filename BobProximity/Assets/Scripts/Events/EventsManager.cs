using System;

namespace Events
{
    public class EventsManager
    {
        #region Actions
        
        protected static event Action<int[]> GameDataLoadedAction;
        protected static event Action GameDataResetAction;
        protected static event Action GameOverAction;
        protected static event Action GamePausedAction;
        protected static event Action GameResumedAction;
        protected static event Action GameStartedAction;
        protected static event Action GameTiedAction;
        protected static event Action<int , string> PlayerNamesUpdatedAction;
        protected static event Action<int[]> PlayerTotalReceivedAction;
        protected static event Action<int> PlayerWinsAction;

        #endregion

        #region Subscribe Functions
		
        public static void SubscribeToEvent(Event gameEvent , Action actionFunction)
        {
            switch(gameEvent)
            {
                case Event.GameDataReset:
                    GameDataResetAction += actionFunction;
                break;
                
                case Event.GameOver:
                    GameOverAction += actionFunction;
                break;
                
                case Event.GamePaused:
                    GamePausedAction += actionFunction;
                break;
                
                case Event.GameResumed:
                    GameResumedAction += actionFunction;
                break;
                
                case Event.GameStarted:
                    GameStartedAction += actionFunction;
                break;
                
                case Event.GameTied:
                    GameTiedAction += actionFunction;
                break;
            }
        }
        
        public static void SubscribeToEvent(Event gameEvent , Action<int> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.PlayerWins:
                    PlayerWinsAction += actionFunction;
                break;
            }
        }
        
        public static void SubscribeToEvent(Event gameEvent , Action<int[]> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.GameDataLoaded:
                    GameDataLoadedAction += actionFunction;
                break;
                
                case Event.PlayerTotalReceived:
                    PlayerTotalReceivedAction += actionFunction;
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
                case Event.GameDataReset:
                    GameDataResetAction -= actionFunction;
                break;
                
                case Event.GameOver:
                    GameOverAction -= actionFunction;
                break;
                
                case Event.GamePaused:
                    GamePausedAction -= actionFunction;
                break;
                
                case Event.GameResumed:
                    GameResumedAction -= actionFunction;
                break;
                
                case Event.GameStarted:
                    GameStartedAction -= actionFunction;
                break;
                
                case Event.GameTied:
                    GameTiedAction -= actionFunction;
                break;
            }
        }
        
        public static void UnsubscribeFromEvent(Event gameEvent , Action<int> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.PlayerWins:
                    PlayerWinsAction -= actionFunction;
                break;
            }
        }
        
        public static void UnsubscribeFromEvent(Event gameEvent , Action<int[]> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.GameDataLoaded:
                    GameDataLoadedAction -= actionFunction;
                break;
                
                case Event.PlayerTotalReceived:
                    PlayerTotalReceivedAction -= actionFunction;
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
                case Event.GameDataReset:
                    GameDataResetAction?.Invoke();
                break;
                
                case Event.GameOver:
                    GameOverAction?.Invoke();
                break;
                
                case Event.GamePaused:
                    GamePausedAction?.Invoke();
                break;
                
                case Event.GameResumed:
                    GameResumedAction?.Invoke();
                break;
                
                case Event.GameStarted:
                    GameStartedAction?.Invoke();
                break;
                
                case Event.GameTied:
                    GameTiedAction?.Invoke();
                break;
            }
        }
        
        public static void Invoke(Event gameEvent , int highestScorePlayerID)
        {
            switch(gameEvent)
            {
                case Event.PlayerWins:
                    PlayerWinsAction?.Invoke(highestScorePlayerID);
                break;
            }
        }
        
        public static void Invoke(Event gameEvent , int[] valueArray)
        {
            switch(gameEvent)
            {
                case Event.GameDataLoaded:
                    GameDataLoadedAction?.Invoke(valueArray);
                break;
                
                case Event.PlayerTotalReceived:
                    PlayerTotalReceivedAction?.Invoke(valueArray);
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