using UnityEngine;

namespace CIW.Code.Player
{
    public class PlayerMotor2D : DevLib.ModuleSystem.Module
    {
        [SerializeField] Rigidbody2D rigid;
        [SerializeField] PlayerMovementDataSO movementData;
        [Header("Platform Riding")]
        [SerializeField, Min(0.01f)] float maxPlatformStep = 0.75f;
        [SerializeField, Min(0f)] float platformSkin = 0.01f;

        Collider2D _support;
        Vector3 _supportLocalPoint;
        Vector2 _supportWorldPoint;
        Vector2 _supportGravity;
        bool _jumpSeparating;
        readonly RaycastHit2D[] _carryHits = new RaycastHit2D[32];

        float _moveInput;
        float _jumpBufferTimer;
        float _coyoteTimer;
        bool _jumpCutRequested;
        bool _simulationEnabled = true;

        PlayerGroundSensor _groundSensor;
        PlayerRuleController _rules;

        public Vector2 Velocity => rigid != null ? rigid.linearVelocity : Vector2.zero;
        public float MoveInput => _moveInput;
        public Collider2D CurrentPlatform => _support;
        // 입력 횟수가 아니라 물리적으로 실행된 점프만 기록합니다. 착지와 재점프가 같은 틱이어도 View가 감지할 수 있습니다.
        public uint JumpSequence { get; private set; }

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
            // Transform 기반 발판의 최신 위치를 물리 쿼리에 반영한 뒤, 지난 틱의 운반량부터 적용합니다.
            Physics2D.SyncTransforms();
            CarryWithPlatform(gravityDirection);
            _groundSensor.CheckGround(gravityDirection);
            if (_jumpSeparating && Vector2.Dot(rigid.linearVelocity, gravityDirection) < 0f)
                _groundSensor.ResetContactState();
            else
                _jumpSeparating = false;
            UpdateGraceTimers(deltaTime);

            Vector2 vel = rigid.linearVelocity;
            Vector2 horizontalAxis = GetHorizontalAxis(gravityDirection);

            ApplyHorizontalMovement(ref vel, horizontalAxis, deltaTime);
            TryApplyJump(ref vel, gravityDirection);
            ApplyJumpCut(ref vel, gravityDirection);
            ApplyGravity(ref vel, gravityDirection, deltaTime);

            rigid.linearVelocity = vel;
            CapturePlatform(gravityDirection);
        }

        private void CarryWithPlatform(Vector2 gravityDirection)
        {
            if (_support == null || !_support.enabled || !_support.gameObject.activeInHierarchy ||
                Vector2.Dot(_supportGravity, gravityDirection) < 0.99f)
            {
                DetachPlatform();
                return;
            }

            Vector2 nextPoint = _support.transform.TransformPoint(_supportLocalPoint);
            Vector2 delta = nextPoint - _supportWorldPoint;
            if (delta.magnitude > maxPlatformStep)
            {
                // 순간이동/레벨 리셋을 정상 이동으로 취급하면 플레이어도 함께 날아가므로 연결을 해제합니다.
                DetachPlatform();
                return;
            }
            float distance = delta.magnitude;
            if (distance <= Mathf.Epsilon) return;

            Vector2 direction = delta / distance;
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            int count = rigid.Cast(direction, filter, _carryHits, distance + platformSkin);
            float allowed = distance;
            for (int i = 0; i < count; i++)
            {
                var hit = _carryHits[i];
                if (hit.collider == _support || (hit.rigidbody != null && hit.rigidbody == _support.attachedRigidbody))
                    continue;
                // 이동을 실제로 가로막는 면만 검사해 발밑 지면 접촉이 수평 운반을 막지 않게 합니다.
                if (Vector2.Dot(hit.normal, direction) < -0.01f)
                    allowed = Mathf.Min(allowed, Mathf.Max(0f, hit.distance - platformSkin));
            }
            // 운반은 위치에만 반영합니다. linearVelocity는 조작 속도를 유지해 가만히 서 있을 때 Idle이 됩니다.
            rigid.position += direction * allowed;
        }

        private void CapturePlatform(Vector2 gravityDirection)
        {
            _support = _groundSensor.GroundCollider;
            if (_support == null) return;
            // 매 틱 현재 위치를 새 기준점으로 잡아 걷기와 발판 이동을 독립적으로 계산합니다.
            _supportWorldPoint = rigid.position;
            _supportLocalPoint = _support.transform.InverseTransformPoint(rigid.position);
            _supportGravity = gravityDirection;
        }

        private void DetachPlatform()
        {
            _support = null;
            _supportLocalPoint = Vector3.zero;
            _supportWorldPoint = Vector2.zero;
        }

        private void OnDisable()
        {
            DetachPlatform();
            _groundSensor?.ResetContactState();
            _coyoteTimer = 0f;
            _jumpSeparating = false;
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
                DetachPlatform();
                _groundSensor?.ResetContactState();
                _coyoteTimer = 0f;
                _jumpSeparating = false;
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
            DetachPlatform();
            _groundSensor?.ResetContactState();
            _jumpSeparating = false;
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

        public float GetNormalizedHorizontalSpeed()
        {
            if (movementData == null || _rules == null)
                return 0f;

            // 규칙에 의해 달라진 최고 속도를 기준으로 정규화해 애니메이션이 물리 수치에 종속되지 않게 합니다.
            float referenceSpeed = movementData.MoveSpeed * _rules.MoveMultiplier;
            return referenceSpeed > Mathf.Epsilon
                ? Mathf.Clamp01(Mathf.Abs(GetHorizontalSpeed()) / referenceSpeed)
                : 0f;
        }

        public float GetNormalizedVerticalSpeed()
        {
            if (movementData == null || _rules == null)
                return 0f;

            float verticalSpeed = GetVerticalSpeed();
            float referenceSpeed = verticalSpeed >= 0f
                ? movementData.JumpPower * _rules.JumpMultiplier
                : movementData.MaxFallSpeed;

            // 하강 속도는 음수이므로 절댓값을 사용해 Animator가 역재생되지 않도록 합니다.
            return referenceSpeed > Mathf.Epsilon
                ? Mathf.Clamp01(Mathf.Abs(verticalSpeed) / referenceSpeed)
                : 0f;
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
            // 점프 직후 센서 범위 안에 발판이 남아 있어도 다시 탑승하거나 코요테 시간을 충전하지 않습니다.
            _jumpSeparating = true;
            _groundSensor.ResetContactState();
            DetachPlatform();
            unchecked { JumpSequence++; }
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
    
