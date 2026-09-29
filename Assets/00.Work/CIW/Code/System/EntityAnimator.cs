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
        bool _exitPresentation;
        bool _animatorWasEnabled;
        Vector3 _exitLocalPosition, _exitScale, _exitStart, _exitTarget;
        Color _exitColor;

        public void BeginExitPresentation(Vector3 target)
        {
            RestoreExitPresentation();
            if (spriteRenderer == null) return;
            _exitPresentation = true;
            _exitLocalPosition = spriteRenderer.transform.localPosition;
            _exitScale = spriteRenderer.transform.localScale;
            _exitColor = spriteRenderer.color;
            _exitStart = spriteRenderer.transform.position;
            _exitTarget = target;
            _exitTarget.z = _exitStart.z;
            _animatorWasEnabled = animator != null && animator.enabled;
            // 입장 중에는 클립이 색상/Transform을 덮어쓰지 않도록 일시 정지합니다.
            if (animator != null) animator.enabled = false;
        }

        public void SetExitProgress(float progress)
        {
            if (!_exitPresentation || spriteRenderer == null) return;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
            spriteRenderer.transform.position = Vector3.Lerp(_exitStart, _exitTarget, t);
            spriteRenderer.transform.localScale = _exitScale * Mathf.Lerp(1f, 0.2f, t);
            Color color = _exitColor;
            color.a *= 1f - t;
            spriteRenderer.color = color;
        }

        public void RestoreExitPresentation()
        {
            if (!_exitPresentation) return;
            _exitPresentation = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = _exitLocalPosition;
                spriteRenderer.transform.localScale = _exitScale;
                spriteRenderer.color = _exitColor;
            }
            if (animator != null) animator.enabled = _animatorWasEnabled;
        }

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

        public void RestartState(int stateHash)
        {
            if (animator == null || stateHash == 0 || !animator.HasState(0, stateHash))
                return;

            // 같은 상태가 재생 중이거나 전환 중이어도 새 동작은 반드시 0초부터 시작합니다.
            animator.Play(stateHash, 0, 0f);
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
            {
                animator.Rebind();
                // 기본 상태의 첫 프레임을 즉시 적용해 다시 표시할 때 사망 직전 스프라이트가 보이지 않게 합니다.
                animator.Update(0f);
            }
        }

        public void ResetTrigger(int parameterHash)
        {
            if (animator != null && parameterHash != 0)
                animator.ResetTrigger(parameterHash);
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
