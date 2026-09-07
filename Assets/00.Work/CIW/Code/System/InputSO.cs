using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CIW.Code.System
{
    [CreateAssetMenu(fileName = "Input", menuName = "SO/Input", order = 1)]
    public class InputSO : ScriptableObject, Controls.IPlayerActions
    {
        public event Action OnInteractPressed;
        public event Action OnJumpPressed;
        public event Action<Vector2> OnMovePressed;

        Controls _controls;
        
        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }

            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Disable();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInteractPressed?.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnJumpPressed?.Invoke();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 vec = context.ReadValue<Vector2>();
            OnMovePressed?.Invoke(vec);
        }
    }
}
