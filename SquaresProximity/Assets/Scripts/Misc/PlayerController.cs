namespace Misc
{
    using Event = Managers.Event;
    using Managers;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    public class PlayerController : MonoBehaviour
    {
        private GameMode _gameMode;
        
        [SerializeField] private float mouseThreshold;

        private void Awake()
        {
            InitializeGameMode();
        }

        private void Update()
        {
            if(_gameMode == null)
            {
                InitializeGameMode();
                if(_gameMode == null) return;  // Skip Update if GameMode is still not available
            }

            if(_gameMode.IsOnlineMode && !IsLocalPlayerTurn())
            {
                // Ignore inputs if it’s not the local player’s turn in Online Multiplayer
                return;
            }

            HandleInput();
        }

        private void InitializeGameMode()
        {
            _gameMode = ServiceLocator.Get<GameMode>();
            
            if(_gameMode == null)
            {
                Debug.LogWarning("GameMode is not yet registered. Ensure it's registered before PlayerController accesses it.");
            }
        }

        private void HandleInput()
        {
            #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL

                if(Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    ProcessEvent(Event.KeyboardTabPressed);
                }

                if(Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if(EventSystem.current.IsPointerOverGameObject()) return;

                    ProcessEvent(Event.MouseLeftClicked);
                }

                if(Mouse.current.delta.ReadValue().magnitude > mouseThreshold)
                {
                    ProcessEvent(Event.MouseMoved);
                }

                if(Gamepad.current != null)
                {
                    if(Gamepad.current.buttonSouth.wasPressedThisFrame) ProcessEvent(Event.JoystickXPressed);
                    if(Gamepad.current.dpad.down.wasPressedThisFrame) ProcessEvent(Event.JoystickDownPressed);
                    if(Gamepad.current.dpad.left.wasPressedThisFrame) ProcessEvent(Event.JoystickLeftPressed);
                    if(Gamepad.current.dpad.right.wasPressedThisFrame) ProcessEvent(Event.JoystickRightPressed);
                    if(Gamepad.current.dpad.up.wasPressedThisFrame) ProcessEvent(Event.JoystickUpPressed);
                }

            #endif

            #if UNITY_IOS || UNITY_ANDROID

                if(Touchscreen.current.primaryTouch.press.isPressed)
                {
                    Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                    ProcessEvent(Event.TouchscreenTapped , touchPosition);
                }

            #endif
        }

        private bool IsLocalPlayerTurn()
        {
            // Implement turn-checking logic
            return true;
        }

        private void ProcessEvent(Event eventType , Vector2? touchPosition = null)
        {
            if(_gameMode.IsOnlineMode)
            {
                SendRPC(eventType , touchPosition);
            }
            else
            {
                if(touchPosition.HasValue)
                {
                    EventsManager.Invoke(eventType , touchPosition.Value);
                }
                else
                {
                    EventsManager.Invoke(eventType);
                }
            }
        }

        private void SendRPC(Event eventType , Vector2? touchPosition = null)
        {
            Debug.Log($"Sending RPC for event: {eventType} , position: {touchPosition}");
            // Implement RPC calls here
        }
    }
}