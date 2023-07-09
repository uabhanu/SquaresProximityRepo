using Event = Events.Event;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float mouseMovementThreshold;
    
    private void Update()
    {
        #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
        
            if(Mouse.current.leftButton.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.MouseLeftClicked);
            }

            if(Mouse.current.delta.ReadValue().magnitude > mouseMovementThreshold)
            {
                EventsManager.Invoke(Event.MouseMoved);
            }
            
        #endif

        #if UNITY_IOS || UNITY_ANDROID

            if(Touchscreen.current.primaryTouch.tap.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.TouchscreenTapped);
            }
        
        #endif
    }
}