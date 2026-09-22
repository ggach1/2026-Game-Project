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

            // 문자열 해시는 초기화 때 한 번만 계산해 매 프레임 변환 비용을 만들지 않습니다.
            _groundedHash = ToHash(groundedParameter);
            _verticalSpeedHash = ToHash(verticalSpeedParameter);
            _moveSpeedHash = ToHash(moveSpeedParameter);
            _deathHash = ToHash(deathTrigger);
            _respawnHash = ToHash(respawnTrigger);
            _runPlaybackHash = ToHash(runPlaybackParameter);
            _airPlaybackHash = ToHash(airPlaybackParameter);
        }

        private void LateUpdate()
        {
            if (_motor == null || _groundSensor == null)
                return;

            SetMoveDirection(_motor.GetHorizontalSpeed());
            SetGrounded(_groundSensor.IsGrounded);
            SetVerticalSpeed(_motor.GetVerticalSpeed());
            SetPlaybackSpeed(
                _motor.GetNormalizedHorizontalSpeed(),
                _motor.GetNormalizedVerticalSpeed());
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
            _entityAnimator?.SetTrigger(_deathHash);

            // 전용 사망 애니메이션이 추가되기 전까지 즉시 숨기는 방식으로 사망 상태를 표현합니다.
            _entityAnimator?.SetVisible(false);
        }
        public void PlayRespawn()
        {
            _entityAnimator?.SetTrigger(_respawnHash);
        }

        public void ResetView(bool faceRight)
        {
            if (_entityAnimator == null)
                return;

            _entityAnimator.ResetAnimator();
            _entityAnimator.SetVisible(true);
            _entityAnimator.SetMovementDirection(faceRight ? Vector2.right : Vector2.left);
        }

        private static int ToHash(string parameter)
        {
            return string.IsNullOrWhiteSpace(parameter)
                ? 0
                : Animator.StringToHash(parameter);
        }
    }
}
