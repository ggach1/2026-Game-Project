using UnityEngine;

namespace CIW.Code.Player
{
    public class PlayerMotor2D : DevLib.ModuleSystem.Module
    {
        [SerializeField] Rigidbody2D rigid;
        [SerializeField] PlayerMovementDataSO movementData;

        float _moveInput;
        float _jumpBufferTimer;
        float _coyoteTimer;
        bool _jumpCutRequested;
        bool _simulationEnabled = true;

        PlayerGroundSensor _groundSensor;
        PlayerRuleController _rules;

        public Vector2 Velocity => rigid != null ? rigid.linearVelocity : Vector2.zero;
        public float MoveInput => _moveInput;

        public override void Initialize(DevLib.ModuleSystem.ModuleOwner owner)
        {
            base.Initialize(owner);

            rigid ??= GetComponentInParent<Rigidbody2D>();
            _groundSensor = owner.GetModule<PlayerGroundSensor>();
            _rules = owner.GetModule<PlayerRuleController>();

            // 방향 전환이 가능한 커스텀 중력을 사용하므로 Unity 기본 중력은 끕니다.
            if (rigid != null)
                rigid.gravityScale = 0f;
        }

        private void FixedUpdate()
        {
            if (!_simulationEnabled || rigid == null || movementData == null ||
                _groundSensor == null || _rules == null)
                return;

            float deltaTime = Time.fixedDeltaTime;
            Vector2 gravityDirection = _rules.GravityDirection;
            _groundSensor.CheckGround(gravityDirection);
            UpdateGraceTimers(deltaTime);

            Vector2 vel = rigid.linearVelocity;
            Vector2 horizontalAxis = GetHorizontalAxis(gravityDirection);

            ApplyHorizontalMovement(ref vel, horizontalAxis, deltaTime);
            TryApplyJump(ref vel, gravityDirection);
            ApplyJumpCut(ref vel, gravityDirection);
            ApplyGravity(ref vel, gravityDirection, deltaTime);

            rigid.linearVelocity = vel;
        }

        public void SetMoveInput(float input)
        {
            _moveInput = input;
        }

        public void RequestJump()
        {
            if (movementData != null)
                _jumpBufferTimer = movementData.JumpBufferTime;
        }

        public void ReleaseJump()
        {
            _jumpCutRequested = true;
        }

        public void ClearJumpRequest()
        {
            _jumpBufferTimer = 0f;
            _jumpCutRequested = false;
        }

        public void SetSimulationEnabled(bool enabled)
        {
            _simulationEnabled = enabled;

            if (rigid == null)
                return;

            if (!enabled)
            {
                rigid.linearVelocity = Vector2.zero;
                rigid.angularVelocity = 0f;
                ClearJumpRequest();
            }

            rigid.simulated = enabled;
        }

        public void AddImpulse(Vector2 force)
        {
            if (_simulationEnabled && rigid != null)
                rigid.AddForce(force, ForceMode2D.Impulse);
        }

        public void ResetMotion(Vector2 pos)
        {
            if (rigid == null)
                return;

            // 위치만 옮기면 사망 직전 속도가 남으므로 모든 물리 상태를 함께 초기화
            rigid.position = pos;
            rigid.linearVelocity = Vector2.zero;
            rigid.angularVelocity = 0f;
            _moveInput = 0f;
            _coyoteTimer = 0f;
            ClearJumpRequest();
        }

        public float GetVerticalSpeed()
        {
            if (rigid == null || _rules == null)
                return 0f;

            return -Vector2.Dot(rigid.linearVelocity, _rules.GravityDirection);
        }

        public float GetHorizontalSpeed()
        {
            if (rigid == null || _rules == null)
                return 0f;

            return Vector2.Dot(rigid.linearVelocity, GetHorizontalAxis(_rules.GravityDirection));
        }

        private void UpdateGraceTimers(float deltaTime)
        {
            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - deltaTime);
            _coyoteTimer = _groundSensor.IsGrounded
                ? movementData.CoyoteTime
                : Mathf.Max(0f, _coyoteTimer - deltaTime);
        }

        private void ApplyHorizontalMovement(ref Vector2 velocity, Vector2 horizontalAxis, float deltaTime)
        {
            float input = _rules.CanMove ? _moveInput : 0f;
            if (_rules.IsHorizontalInverted)
                input *= -1f;

            float targetSpeed = input * movementData.MoveSpeed * _rules.MoveMultiplier;
            float currentSpeed = Vector2.Dot(velocity, horizontalAxis);

            float acceleration;
            if (!_groundSensor.IsGrounded)
                acceleration = movementData.AirAcceleration;
            else
                acceleration = Mathf.Abs(targetSpeed) > Mathf.Epsilon
                    ? movementData.GroundAcceleration
                    : movementData.GroundDeceleration;

            float nextSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * deltaTime);
            velocity += horizontalAxis * (nextSpeed - currentSpeed);
        }

        private void TryApplyJump(ref Vector2 velocity, Vector2 gravityDirection)
        {
            if (_jumpBufferTimer <= 0f || _coyoteTimer <= 0f || !_rules.CanJump)
                return;

            // 낙하 속도를 먼저 지워야 점프를 누른 시점과 관계없이 점프 높이가 일정하게 된당
            float fallingSpeed = Vector2.Dot(velocity, gravityDirection);
            if (fallingSpeed > 0f)
                velocity -= gravityDirection * fallingSpeed;

            velocity -= gravityDirection * movementData.JumpPower * _rules.JumpMultiplier;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }

        private void ApplyJumpCut(ref Vector2 velocity, Vector2 gravityDirection)
        {
            if (!_jumpCutRequested)
                return;

            float fallingSpeed = Vector2.Dot(velocity, gravityDirection);
            if (fallingSpeed < 0f)
            {
                Vector2 risingVelocity = gravityDirection * fallingSpeed;
                velocity -= risingVelocity;
                velocity += risingVelocity * movementData.JumpCutMultiplier;
            }

            _jumpCutRequested = false;
        }

        private void ApplyGravity(ref Vector2 velocity, Vector2 gravityDirection, float deltaTime)
        {
            float fallingSpeed = Vector2.Dot(velocity, gravityDirection);
            float gravity = fallingSpeed < 0f
                ? movementData.RisingGravity
                : movementData.FallingGravity;

            velocity += gravityDirection * gravity * deltaTime;

            fallingSpeed = Vector2.Dot(velocity, gravityDirection);
            if (fallingSpeed > movementData.MaxFallSpeed)
                velocity -= gravityDirection * (fallingSpeed - movementData.MaxFallSpeed);
        }

        private static Vector2 GetHorizontalAxis(Vector2 gravityDirection)
        {
            Vector2 axis = new Vector2(-gravityDirection.y, gravityDirection.x).normalized;

            // 중력이 위아래로 뒤집혀도 오른쪽 입력은 항상 화면 오른쪽을 향하게 함
            if (axis.x < 0f)
                axis = -axis;

            return axis;
        }
    }
}
    
