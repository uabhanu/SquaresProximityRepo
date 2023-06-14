using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private GridManager _gridManager;
    private InputActions _playerInputActions;

    private void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        _playerInputActions = new InputActions();
        _playerInputActions.ProximityMap.Enable();
    }

    private void Update()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.nearClipPlane;
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2Int cellIndexAtMouseWorldPos = _gridManager.WorldToCell(mouseWorldPos);
        
        Debug.Log("Cell Index : " + cellIndexAtMouseWorldPos);
    }
}
