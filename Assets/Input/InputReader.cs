using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "InputReader")]
public class InputReader : ScriptableObject, GameInput.IPlayerActions, GameInput.IPlayer2Actions
{
    private GameInput _gameInput;

    void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new GameInput();
            _gameInput.Player.SetCallbacks(this);
            _gameInput.Player.Enable();
            _gameInput.Player2.SetCallbacks(this);
        }
    }

    public void SetPlayer2()
    {
        _gameInput.Player.Disable();
        _gameInput.Player2.Enable();
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
        BombEvent?.Invoke();
    }

}
