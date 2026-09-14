using DevLib.AnimatorSystem;
using DevLib.ModuleSystem;
using System;
using System.Collections;
using UnityEngine;

namespace CIW.Code.System
{
    public class EntityAnimator : MonoBehaviour, IModule, IAnimatorRenderer
    {
        [SerializeField] Animator animator;

        SpriteRenderer _renderer;
        Entity _entity;

        public Animator Animator => animator;
        public SpriteRenderer Renderer => _renderer;

        public event Action OnAnimationEnd;
        public event Action OnDamageCast;
        public event Action<Vector2> OnFootstep;

        public Vector2 GetFacingDirection()
        {
            throw new NotImplementedException();
        }

        public void Initialize(ModuleOwner owner)
        {
            _entity = owner as Entity;
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void RenderClip(int clipHash)
        {
            throw new NotImplementedException();
        }

        public void RenderClipIfNotPlaying(int clipHash)
        {
            throw new NotImplementedException();
        }

        public void SetMovementDirection(Vector2 dir)
        {
            throw new NotImplementedException();
        }
    }
}