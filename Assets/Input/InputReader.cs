using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, PlayerInput.IPlayerActions
{
    private PlayerInput _gameInput;//Refactor PlayerInput class to GameInput

    void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new PlayerInput();
            _gameInput.Player.SetCallbacks(this);
            _gameInput.Player.Enable();
        }
    }

    public event Action<Vector2> MoveEvent;
    public event Action MoveCancelledEvent;

    public event Action BombEvent;

    public void OnMovement(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.phase == UnityEngine.InputSystem.InputActionPhase.Performed)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
        else if (context.phase == UnityEngine.InputSystem.InputActionPhase.Canceled)
        {
            MoveCancelledEvent?.Invoke();
        }
    }

    public void OnBomb(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

}
