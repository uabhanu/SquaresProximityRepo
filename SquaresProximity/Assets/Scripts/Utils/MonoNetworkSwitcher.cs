namespace Utils
{
    using System;
    using Managers;
    using UnityEngine;
    
    public class MonoNetworkSwitcher : MonoBehaviour
    {
        private MonoPlayerController _monoPlayerControllerInstance;
        
        [SerializeField] private MonoPlayerController monoPlayerControllerPrefab;
        
        private void Start()
        {
            _monoPlayerControllerInstance = Instantiate(monoPlayerControllerPrefab.gameObject).GetComponent<MonoPlayerController>();
            
            _monoPlayerControllerInstance.gameObject.SetActive(false);
            
            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }
        
        private void OnPlayerIsOffline()
        {
            _monoPlayerControllerInstance.gameObject.SetActive(true);
        }

        private void OnPlayerIsOnline()
        {
            _monoPlayerControllerInstance.gameObject.SetActive(false);
        }
    
        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Managers.Event.PlayerIsOffline , new Action(OnPlayerIsOffline));
                EventsManager.SubscribeToEvent(Managers.Event.PlayerIsOnline , new Action(OnPlayerIsOnline));
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Managers.Event.PlayerIsOffline , new Action(OnPlayerIsOffline));
                EventsManager.UnsubscribeFromEvent(Managers.Event.PlayerIsOnline , new Action(OnPlayerIsOnline));
            }
        }
    }
}