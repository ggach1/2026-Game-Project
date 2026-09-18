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
        [SerializeField] string groundedParameter = "";
        [SerializeField] string verticalSpeedParameter = "";
        [SerializeField] string moveSpeedParameter = "";
        [SerializeField] string deathTrigger = "";
        [SerializeField] string respawnTrigger = "";

        CIW.Code.System.EntityAnimator _entityAnimator;
        PlayerMotor2D _motor;
        PlayerGroundSensor _groundSensor;

        int _groundedHash;
        int _verticalSpeedHash;
        int _moveSpeedHash;
        int _deathHash;
        int _respawnHash;

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
        }

        private void LateUpdate()
        {
            if (_motor == null || _groundSensor == null)
                return;

            SetMoveDirection(_motor.GetHorizontalSpeed());
            SetGrounded(_groundSensor.IsGrounded);
            SetVerticalSpeed(_motor.GetVerticalSpeed());
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

        public void PlayDeath(DeathContext context)
        {
            _entityAnimator?.SetTrigger(_deathHash);
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
