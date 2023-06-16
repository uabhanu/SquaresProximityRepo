using System;

namespace Events
{
    public class EventsManager
    {
        #region Actions
        
        protected static event Action<int , int> CoinPlacedAction;

        #endregion

        #region Subscribe Functions
		
        public static void SubscribeToEvent(Event gameEvent , Action<int , int> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.CoinPlaced:
                    CoinPlacedAction += actionFunction;
                break;
            }
        }

        #endregion
        
        #region Unsubscribe Functions

        public static void UnsubscribeFromEvent(Event gameEvent , Action<int , int> actionFunction)
        {
            switch(gameEvent)
            {
                case Event.CoinPlaced:
                    CoinPlacedAction -= actionFunction;
                break;
            }
        }

        #endregion
        
        #region Invoke Functions

        public static void Invoke(Event gameEvent , int coinValue , int currentPlayer)
        {
            switch(gameEvent)
            {
                case Event.CoinPlaced:
                    CoinPlacedAction?.Invoke(coinValue , currentPlayer);    
                break;
            }
        }

        #endregion
    }
}