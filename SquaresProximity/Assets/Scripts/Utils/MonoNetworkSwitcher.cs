namespace Utils
{
    using System;
    using Managers;
    using UnityEngine;
    
    public class MonoNetworkSwitcher : MonoBehaviour
    {
        private MonoPlayerController _monoPlayerControllerInstance;
        private NetworkPlayerController _networkPlayerControllerInstance;
        
        [SerializeField] private MonoPlayerController monoPlayerControllerPrefab;
        [SerializeField] private NetworkPlayerController networkPlayerControllerPrefab;
        
        private void Start()
        {
            _monoPlayerControllerInstance = Instantiate(monoPlayerControllerPrefab.gameObject).GetComponent<MonoPlayerController>();
            _networkPlayerControllerInstance = Instantiate(networkPlayerControllerPrefab.gameObject).GetComponent<NetworkPlayerController>();
            
            _monoPlayerControllerInstance.gameObject.SetActive(false);
            _networkPlayerControllerInstance.gameObject.SetActive(false);
            
            ToggleEventSubscription(true);
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }
        
        private void OnPlayerIsOffline()
        {
            if(_monoPlayerControllerInstance != null) _monoPlayerControllerInstance.gameObject.SetActive(true);
            
            if(_networkPlayerControllerInstance != null) _networkPlayerControllerInstance.gameObject.SetActive(false);
        }

        private void OnPlayerIsOnline()
        {
            if(_monoPlayerControllerInstance != null) _monoPlayerControllerInstance.gameObject.SetActive(false);
            
            if(_networkPlayerControllerInstance != null) _networkPlayerControllerInstance.gameObject.SetActive(true);
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