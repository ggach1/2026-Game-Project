using CIW.Code.System.Interface;
using DevLib.ModuleSystem;
using UnityEngine;

namespace CIW.Code.Player
{
    /// <summary>
    /// 플레이어의 런타임 상태를 애니메이션 명령으로 변환합니다.
    /// Animator와 SpriteRenderer의 실제 제어는 EntityAnimator에 위임합니다.
    /// </summary>
    public class PlayerView : Module
    {
        [SerializeField] string groundedParameter = "Grounded";
        [SerializeField] string verticalSpeedParameter = "VertSpeed";
        [SerializeField] string moveSpeedParameter = "MoveSpeed";
        [SerializeField] string deathTrigger = "Death";
        [SerializeField] string respawnTrigger = "Respawn";
        [SerializeField] string jumpState = "Base Layer.player jump";

        [Header("Playback Speed")]
        [SerializeField] string runPlaybackParameter = "RunPlayback";
        [SerializeField] string airPlaybackParameter = "AirPlayback";
        [SerializeField, Min(0f)] float minRunPlaybackSpeed = 0.75f;
        [SerializeField, Min(0f)] float maxRunPlaybackSpeed = 1.25f;
        [SerializeField, Min(0f)] float minAirPlaybackSpeed = 0.75f;
        [SerializeField, Min(0f)] float maxAirPlaybackSpeed = 1.35f;

        CIW.Code.System.EntityAnimator _entityAnimator;
        PlayerMotor2D _motor;
        PlayerGroundSensor _groundSensor;
        PlayerDeathBurst _deathBurst;
        PlayerRuleController _rules;
        bool _dead;
        uint _seenJumpSequence;
        int _jumpStateHash;

        int _groundedHash;
        int _verticalSpeedHash;
        int _moveSpeedHash;
        int _deathHash;
        int _respawnHash;
        int _runPlaybackHash;
        int _airPlaybackHash;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _entityAnimator = owner.GetModule<CIW.Code.System.EntityAnimator>();
            _motor = owner.GetModule<PlayerMotor2D>();
            _groundSensor = owner.GetModule<PlayerGroundSensor>();
            _deathBurst = owner.GetComponent<PlayerDeathBurst>();
            _rules = owner.GetModule<PlayerRuleController>();
            _seenJumpSequence = _motor.JumpSequence;
            _jumpStateHash = ToHash(jumpState);

            // 문자열 해시는 초기화 때 한 번만 계산해 매 프레임 변환 비용을 만들지 않습니다.
            _groundedHash = ToHash(groundedParameter);
            _verticalSpeedHash = ToHash(verticalSpeedParameter);
            _moveSpeedHash = ToHash(moveSpeedParameter);
            _deathHash = ToHash(deathTrigger);
            _respawnHash = ToHash(respawnTrigger);
            _runPlaybackHash = ToHash(runPlaybackParameter);
            _airPlaybackHash = ToHash(airPlaybackParameter);
        }

        private void Update()
        {
            if (_dead || _motor == null || _groundSensor == null)
                return;

            SetMoveDirection(_motor.GetHorizontalSpeed());
            SetGrounded(_groundSensor.IsGrounded);
            SetVerticalSpeed(_motor.GetVerticalSpeed());
            SetPlaybackSpeed(
                _motor.GetNormalizedHorizontalSpeed(),
                _motor.GetNormalizedVerticalSpeed());

            // Animator 평가 전에 최신 파라미터를 전달합니다. 버퍼 점프는 Grounded=true인 렌더 프레임 없이
            // 일어날 수 있으므로 접지 전환 대신 실제 점프 번호로 Jump를 재시작합니다. 공중 연타는 번호가 바뀌지 않습니다.
            if (_seenJumpSequence != _motor.JumpSequence)
            {
                _seenJumpSequence = _motor.JumpSequence;
                _entityAnimator?.RestartState(_jumpStateHash);
            }
        }

        public void SetMoveDirection(float dir)
        {
            _entityAnimator?.SetFloat(_moveSpeedHash, Mathf.Abs(dir));

            if (Mathf.Abs(dir) <= 0.01f)
                return;

            _entityAnimator?.SetMovementDirection(Vector2.right * dir);
        }

        public void SetGrounded(bool grounded)
        {
            _entityAnimator?.SetBool(_groundedHash, grounded);
        }

        public void SetVerticalSpeed(float spd)
        {
            _entityAnimator?.SetFloat(_verticalSpeedHash, spd);
        }

        private void SetPlaybackSpeed(float horizontalRatio, float verticalRatio)
        {
            // 물리 속도 자체가 아닌 제한된 배율을 전달해 클립이 멈추거나 과속하는 것을 방지합니다.
            float runPlayback = Mathf.Lerp(
                minRunPlaybackSpeed, maxRunPlaybackSpeed, Mathf.Clamp01(horizontalRatio));
            float airPlayback = Mathf.Lerp(
                minAirPlaybackSpeed, maxAirPlaybackSpeed, Mathf.Clamp01(verticalRatio));

            _entityAnimator?.SetFloat(_runPlaybackHash, runPlayback);
            _entityAnimator?.SetFloat(_airPlaybackHash, airPlayback);
        }

        public void PlayDeath(DeathContext context)
        {
            _dead = true;
            // 렌더러를 숨기기 전에 마지막 위치를 기준으로 파편을 생성합니다.
            _deathBurst?.Play(_entityAnimator?.Renderer, _rules.GravityDirection);
            _entityAnimator?.ResetTrigger(_respawnHash);
            _entityAnimator?.SetTrigger(_deathHash);

            // 파편은 별도 렌더러로 재생하며 본체는 Animator의 빈 Dead 상태에서 숨겨 둡니다.
            _entityAnimator?.SetVisible(false);
        }
        public void PlayRespawn()
        {
            _entityAnimator?.ResetTrigger(_deathHash);
            _entityAnimator?.SetTrigger(_respawnHash);
        }

        public void ResetView(bool faceRight)
        {
            _dead = false;
            // 사망 직전 아직 화면에 반영되지 않은 점프를 리스폰 후 재생하지 않습니다.
            _seenJumpSequence = _motor.JumpSequence;
            _deathBurst?.Clear();
            if (_entityAnimator == null)
                return;

            _entityAnimator.ResetAnimator();
            _entityAnimator.SetVisible(true);
            _entityAnimator.SetMovementDirection(faceRight ? Vector2.right : Vector2.left);
            SetMoveDirection(0f);
            SetVerticalSpeed(0f);
            SetGrounded(false);
            SetPlaybackSpeed(0f, 0f);
        }

        private static int ToHash(string parameter)
        {
            return string.IsNullOrWhiteSpace(parameter)
                ? 0
                : Animator.StringToHash(parameter);
        }
    }
}
