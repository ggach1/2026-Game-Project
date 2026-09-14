using System.Collections;
using UnityEngine;

namespace CIW.Code.Player
{
    public class PlayerInputController : DevLib.ModuleSystem.Module
    {
        PlayerMotor2D _motor;
        CIW.Code.System.InputSO _input;

        bool _inputEnabled = true;
        bool _subscribed;

        public override void Initialize(DevLib.ModuleSystem.ModuleOwner owner)
        {
            base.Initialize(owner);

            Player player = (Player)owner;
            _motor = owner.GetModule<PlayerMotor2D>();
            _input = player.PlayerInput;
            SubscribeInput();
        }

        private void OnEnable()
        {
            SubscribeInput();
        }

        private void OnDisable()
        {
            UnsubscribeInput();
            ClearInput();
        }

        private void Update()
        {
            if (_inputEnabled && _input != null && _motor != null)
                _motor.SetMoveInput(_input.MoveDir.x);
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;

            if (!enabled)
            {
                _motor.SetMoveInput(0f);
                _motor.ClearJumpRequest();
            }
        }

        public void ClearInput()
        {
            if (_motor == null)
                return;

            _motor.SetMoveInput(0f);
            _motor.ClearJumpRequest();
        }

        private void SubscribeInput()
        {
            if (_input == null || _subscribed)
                return;

            _input.OnJumpPressed += HandleJumpPressed;
            _input.OnJumpReleased += HandleJumpReleased;
            _subscribed = true;
        }

        private void UnsubscribeInput()
        {
            if (_input == null || !_subscribed)
                return;

            _input.OnJumpPressed -= HandleJumpPressed;
            _input.OnJumpReleased -= HandleJumpReleased;
            _subscribed = false;
        }

        private void HandleJumpPressed()
        {
            if (_inputEnabled)
                _motor.RequestJump();
        }

        private void HandleJumpReleased()
        {
            if (_inputEnabled)
                _motor.ReleaseJump();
        }
    }
}
