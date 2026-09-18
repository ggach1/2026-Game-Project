using DevLib.AnimatorSystem;
using DevLib.ModuleSystem;
using System;
using UnityEngine;

namespace CIW.Code.System
{
    /// <summary>
    /// 모든 Entity가 공유하는 Animator/SpriteRenderer 어댑터입니다.
    /// 게임 규칙을 해석하지 않고 재생, 파라미터 변경, 방향 표현만 담당합니다.
    /// </summary>
    public class EntityAnimator : Module, IAnimatorRenderer
    {
        [SerializeField] Animator animator;
        [SerializeField] SpriteRenderer spriteRenderer;

        Entity _entity;
        Vector2 _facingDirection = Vector2.right;

        public Animator Animator => animator;
        public SpriteRenderer Renderer => spriteRenderer;

        public event Action OnAnimationEnd;
        public event Action OnDamageCast;
        public event Action<Vector2> OnFootstep;

        public Vector2 GetFacingDirection()
        {
            return _facingDirection;
        }

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _entity = owner as Entity;
            animator ??= GetComponentInChildren<Animator>();
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        }

        public void RenderClip(int clipHash)
        {
            if (animator == null || clipHash == 0)
                return;

            animator.Play(clipHash);
        }

        public void RenderClipIfNotPlaying(int clipHash)
        {
            if (animator == null || clipHash == 0)
                return;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.shortNameHash != clipHash && state.fullPathHash != clipHash)
                animator.Play(clipHash);
        }

        public void SetMovementDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude <= Mathf.Epsilon)
                return;

            _facingDirection = dir.normalized;

            // 현재 캐릭터 표현은 좌우 반전만 사용하므로 수평 입력이 있을 때만 뒤집습니다.
            if (spriteRenderer != null && Mathf.Abs(dir.x) > Mathf.Epsilon)
                spriteRenderer.flipX = dir.x < 0f;
        }

        public void SetBool(int parameterHash, bool value)
        {
            if (animator != null && parameterHash != 0)
                animator.SetBool(parameterHash, value);
        }

        public void SetFloat(int parameterHash, float value)
        {
            if (animator != null && parameterHash != 0)
                animator.SetFloat(parameterHash, value);
        }

        public void SetTrigger(int parameterHash)
        {
            if (animator != null && parameterHash != 0)
                animator.SetTrigger(parameterHash);
        }

        public void ResetAnimator()
        {
            if (animator != null)
                animator.Rebind();
        }

        public void SetVisible(bool visible)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;
        }

        // Animation Event가 게임 코드에 알림을 전달할 때 사용하는 진입점입니다.
        public void RaiseAnimationEnd() => OnAnimationEnd?.Invoke();
        public void RaiseDamageCast() => OnDamageCast?.Invoke();
        public void RaiseFootstep() => OnFootstep?.Invoke(_facingDirection);

        private void OnValidate()
        {
            animator ??= GetComponentInChildren<Animator>();
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        }
    }
}
