using CIW.Code.Player;
using CIW.Code.System;
using CIW.Code.System.Interface;
using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    [DefaultExecutionOrder(-100)]
    public class SpringBounce2D : MonoBehaviour
    {
        [SerializeField] private InputSO playerInput;

        private Player _player;
        private Rigidbody2D _body;
        private bool _jumpHeld;
        private float _jumpBuffer;
        private float _boostTimer;
        private float _boostPower;
        private Vector2 _gravity;

        private void OnEnable()
        {
            if (playerInput == null) return;
            playerInput.OnJumpPressed += HandleJumpPressed;
            playerInput.OnJumpReleased += HandleJumpReleased;
        }

        public bool TryBounce(Player player, float bouncePower, float boostPower, float boostWindow)
        {
            if (isActiveAndEnabled == false || player == null || player.IsAlive == false ||
                player.Motor == null || player.Rules == null || player.Motor.isActiveAndEnabled == false ||
                !float.IsFinite(bouncePower) || !float.IsFinite(boostPower) || !float.IsFinite(boostWindow) ||
                bouncePower <= 0f || boostPower < bouncePower || boostWindow < 0f)
                return false;

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body == null || body.simulated == false) return false;

            EndBounce();
            _player = player;
            _body = body;
            _gravity = player.Rules.GravityDirection;
            _boostPower = boostPower;
            bool boosted = player.PlayerInput == playerInput && player.Rules.CanJump &&
                           (_jumpHeld || _jumpBuffer > 0f);
            _boostTimer = boosted ? 0f : boostWindow;
            _jumpBuffer = 0f;

            // 기존 공개 API로 접지/탑승/점프 버퍼를 비우고, 이동 입력과 수평 속도는 보존합니다.
            Vector2 velocity = body.linearVelocity;
            float moveInput = player.Motor.MoveInput;
            player.Motor.ResetMotion(body.position);
            player.Motor.SetMoveInput(moveInput);
            body.linearVelocity = velocity - _gravity * Vector2.Dot(velocity, _gravity);
            SetVerticalSpeed(boosted ? boostPower : bouncePower);

            player.Life.Died += HandleDeath;
            player.Life.Respawned += ResetBounce;
            player.Life.Escaped += ResetBounce;
            return true;
        }

        private void FixedUpdate()
        {
            if (_player != null)
            {
                if (_player.IsAlive == false || _body == null || _body.simulated == false ||
                    _player.Motor.isActiveAndEnabled == false ||
                    Vector2.Dot(_gravity, _player.Rules.GravityDirection) < 0.99f ||
                    _player.Motor.GetVerticalSpeed() <= 0f)
                    EndBounce();
                else
                {
                    // 플레이어 이동 처리보다 먼저 실행해 스프링 상승 중 버튼 해제가 높이를 깎지 않게 합니다.
                    _player.Motor.ClearJumpRequest();
                    if (_boostTimer > 0f && _jumpBuffer > 0f && _player.Rules.CanJump &&
                        _player.PlayerInput == playerInput)
                    {
                        SetVerticalSpeed(_boostPower);
                        _boostTimer = 0f;
                        _jumpBuffer = 0f;
                    }
                    else
                        _boostTimer = Mathf.Max(0f, _boostTimer - Time.fixedDeltaTime);
                }
            }
            _jumpBuffer = Mathf.Max(0f, _jumpBuffer - Time.fixedDeltaTime);
        }

        private void SetVerticalSpeed(float speed)
        {
            float current = -Vector2.Dot(_body.linearVelocity, _gravity);
            _player.Motor.AddImpulse(-_gravity * ((speed - current) * _body.mass));
        }

        private void HandleJumpPressed()
        {
            _jumpHeld = true;
            _jumpBuffer = 0.1f;
        }

        private void HandleJumpReleased() => _jumpHeld = false;

        private void HandleDeath(DeathContext context) => ResetBounce();

        private void ResetBounce()
        {
            _jumpHeld = false;
            _jumpBuffer = 0f;
            EndBounce();
        }

        private void EndBounce()
        {
            if (_player != null && _player.Life != null)
            {
                _player.Life.Died -= HandleDeath;
                _player.Life.Respawned -= ResetBounce;
                _player.Life.Escaped -= ResetBounce;
            }
            _player = null;
            _body = null;
            _boostTimer = 0f;
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.OnJumpPressed -= HandleJumpPressed;
                playerInput.OnJumpReleased -= HandleJumpReleased;
            }
            _jumpHeld = false;
            _jumpBuffer = 0f;
            EndBounce();
        }
    }
}
