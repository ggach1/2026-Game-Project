using CIW.Code.System.Interface;
using System.Collections;
using UnityEngine;

namespace CIW.Code.Player
{
    public class PlayerView : DevLib.ModuleSystem.Module
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Animator animator;
        [SerializeField] string groundedParameter = "";
        [SerializeField] string verticalSpeedParameter = "";
        [SerializeField] string moveSpeedParameter = "";
        [SerializeField] string deathTrigger = "";
        [SerializeField] string respawnTrigger = "";

        bool _faceRight = true;
        PlayerMotor2D _motor;
        PlayerGroundSensor _groundSensor;

        public override void Initialize(DevLib.ModuleSystem.ModuleOwner owner)
        {
            base.Initialize(owner);
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
            animator ??= GetComponentInChildren<Animator>();
            _motor = owner.GetModule<PlayerMotor2D>();
            _groundSensor = owner.GetModule<PlayerGroundSensor>();
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
            SetAnimatorFloat(moveSpeedParameter, Mathf.Abs(dir));

            if (Mathf.Abs(dir) <= 0.01f)
                return;

            _faceRight = dir > 0f;
            if (spriteRenderer != null)
                spriteRenderer.flipX = !_faceRight;

        }

        public void SetGrounded(bool grounded)
        {
            SetAnimatorBool(groundedParameter, grounded);
        }

        public void SetVerticalSpeed(float spd)
        {
            SetAnimatorFloat(verticalSpeedParameter, spd);
        }

        public void PlayDeath(DeathContext context)
        {
            SetAnimatorTrigger(deathTrigger);
        }
        public void PlayRespawn()
        {
            SetAnimatorTrigger(respawnTrigger);
        }

        public void ResetView(bool faceRight)
        {
            _faceRight = faceRight;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.flipX = !_faceRight;
            }

            if (animator != null)
                animator.Rebind();
        }

        private void SetAnimatorBool(string parameter, bool value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(parameter))
                animator.SetBool(parameter, value);
        }

        private void SetAnimatorFloat(string parameter, float value)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(parameter))
                animator.SetFloat(parameter, value);
        }

        private void SetAnimatorTrigger(string parameter)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(parameter))
                animator.SetTrigger(parameter);
        }
    }
}
