namespace Utils.OnlineMultiplayer
{
    using Managers;
    using System;
    using Unity.Services.Authentication;
    using Unity.Services.Core;
    using UnityEngine;
    
    public class UnityServicesInitializer : MonoBehaviour
    {
        private void Start()
        {
            ToggleEventSubscription(true);
        }

        async void InitializeUnityServices()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services Initialized Successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            }
            
            if(!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously, User ID: " + AuthenticationService.Instance.PlayerId);
            }
        }

        private void OnDestroy()
        {
            ToggleEventSubscription(false);
        }

        private void OnPlayerOnline()
        {
            InitializeUnityServices();
        }

        private void ToggleEventSubscription(bool shouldSubscribe)
        {
            if(shouldSubscribe)
            {
                EventsManager.SubscribeToEvent(Managers.Event.PlayerIsOnline , new Action(OnPlayerOnline));
            }
            else
            {
                EventsManager.UnsubscribeFromEvent(Managers.Event.PlayerIsOnline , new Action(OnPlayerOnline));
            }
        }
    }
}