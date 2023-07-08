using Event = Events.Event;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float mouseMovementThreshold;
    
    private void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            EventsManager.Invoke(Event.MouseLeftClicked);
        }
        
        if(Mouse.current.delta.ReadValue().magnitude > mouseMovementThreshold)
        {
            EventsManager.Invoke(Event.MouseMoved);
        }
    }
}