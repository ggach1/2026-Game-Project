using System.Collections;
using UnityEngine;

namespace CIW.Code.Player
{
    public class PlayerGroundSensor : DevLib.ModuleSystem.Module
    {
        [SerializeField] Collider2D bodyCollider;
        [SerializeField] LayerMask groundLayerMask;
        [SerializeField, Min(0.001f)] float castDistance = 0.08f;

        public bool IsGrounded { get; private set; }
        public bool IsTouchingWall { get; private set; }
        public Vector2 GroundNormal { get; private set; } = Vector2.up;

        readonly RaycastHit2D[] _castHits = new RaycastHit2D[8];
        ContactFilter2D _groundFilter;

        public override void Initialize(DevLib.ModuleSystem.ModuleOwner owner)
        {
            base.Initialize(owner);
            bodyCollider ??= GetComponentInParent<Collider2D>();

            _groundFilter = new ContactFilter2D { useTriggers = false };
            _groundFilter.SetLayerMask(groundLayerMask);
        }

        public void CheckGround(Vector2 gravityDir)
        {
            if (bodyCollider == null)
            {
                IsGrounded = false;
                IsTouchingWall = false;
                return;
            }

            Vector2 gravity = gravityDir.sqrMagnitude > Mathf.Epsilon
                ? gravityDir.normalized
                : Vector2.down;
            // 자신의 Collider2D를 캐스팅하면 자기 자신은 검사 대상에서 제외됩니다.
            int groundHitCount = bodyCollider.Cast(
                gravity, _groundFilter, _castHits, castDistance);

            IsGrounded = groundHitCount > 0;
            GroundNormal = IsGrounded ? _castHits[0].normal : -gravity;

            Vector2 side = new Vector2(-gravity.y, gravity.x);
            int leftHitCount = bodyCollider.Cast(
                -side, _groundFilter, _castHits, castDistance);
            int rightHitCount = bodyCollider.Cast(
                side, _groundFilter, _castHits, castDistance);

            IsTouchingWall = leftHitCount > 0 || rightHitCount > 0;
        }
    }
}
