using Event = Managers.Event;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float mouseThreshold;
    
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

            if(Mouse.current.delta.ReadValue().magnitude > mouseThreshold)
            {
                EventsManager.Invoke(Event.MouseMoved);
            }

            if(Gamepad.current == null) return;
            
            if(Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.JoystickXPressed);
            }
            
            if(Gamepad.current.dpad.down.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.JoystickDownPressed);
            }
            
            if(Gamepad.current.dpad.left.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.JoystickLeftPressed);
            }

            if(Gamepad.current.dpad.right.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.JoystickRightPressed);
            }
            
            if(Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                EventsManager.Invoke(Event.JoystickUpPressed);
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
}