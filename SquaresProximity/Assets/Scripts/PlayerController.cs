using Event = Managers.Event;
using Managers;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    //TODO This approach may not be the best especially when playing Single Player but works for now
    
    [SerializeField] private float mouseMovementThreshold;

    private void Start()
    {
        ToggleEventSubscription(true);
    }

    public override void OnDestroy()
    {
        ToggleEventSubscription(false);
    }

    private void Update()
    {
        #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL

            if(Keyboard.current.tabKey.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.KeyboardTabPressed);
            }
            
            if(Mouse.current.leftButton.wasPressedThisFrame)
            {
                if(EventSystem.current.IsPointerOverGameObject())
                {
                    return; // Ignore the click on UI elements.
                }
                
                EventsManager.Invoke(Event.MouseLeftClicked);
            }

            if(Mouse.current.delta.ReadValue().magnitude > mouseMovementThreshold)
            {
                EventsManager.Invoke(Event.MouseMoved);
            }
            
        #endif

        #if UNITY_IOS || UNITY_ANDROID

            if(Touchscreen.current.primaryTouch.press.isPressed)
            {
                Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                EventsManager.Invoke(Event.TouchscreenTapped , touchPosition);
            }
        
        #endif
    }
    
    private void OnPlayerIsOffline()
    {
        Destroy(gameObject.GetComponent<NetworkObject>());
    }

    private void OnPlayerIsOnline()
    {
        gameObject.AddComponent<NetworkObject>();
    }
    
    private void ToggleEventSubscription(bool shouldSubscribe)
    {
        if(shouldSubscribe)
        {
            EventsManager.SubscribeToEvent(Event.PlayerIsOffline , new Action(OnPlayerIsOffline));
            EventsManager.SubscribeToEvent(Event.PlayerIsOnline , new Action(OnPlayerIsOnline));
        }
        else
        {
            EventsManager.UnsubscribeFromEvent(Event.PlayerIsOffline , new Action(OnPlayerIsOffline));
            EventsManager.UnsubscribeFromEvent(Event.PlayerIsOnline , new Action(OnPlayerIsOnline));
        }
    }
}