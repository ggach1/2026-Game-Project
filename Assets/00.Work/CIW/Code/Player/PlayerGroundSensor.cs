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
        public Collider2D GroundCollider { get; private set; }

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
                ResetContactState();
                return;
            }

            Vector2 gravity = gravityDir.sqrMagnitude > Mathf.Epsilon
                ? gravityDir.normalized
                : Vector2.down;
            // 자신의 Collider2D를 캐스팅하면 자기 자신은 검사 대상에서 제외됩니다.
            int groundHitCount = bodyCollider.Cast(
                gravity, _groundFilter, _castHits, castDistance);

            GroundCollider = null;
            GroundNormal = -gravity;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < groundHitCount; i++)
            {
                var hit = _castHits[i];
                // 옆벽과 가파른 면을 발판으로 잡지 않고, 중력 반대쪽을 받치는 가장 가까운 면을 선택합니다.
                if (Vector2.Dot(hit.normal, -gravity) < 0.65f || hit.distance >= nearest)
                    continue;
                nearest = hit.distance;
                GroundCollider = hit.collider;
                GroundNormal = hit.normal;
            }
            IsGrounded = GroundCollider != null;

            Vector2 side = new Vector2(-gravity.y, gravity.x);
            int leftHitCount = bodyCollider.Cast(
                -side, _groundFilter, _castHits, castDistance);
            int rightHitCount = bodyCollider.Cast(
                side, _groundFilter, _castHits, castDistance);

            IsTouchingWall = leftHitCount > 0 || rightHitCount > 0;
        }

        public void ResetContactState()
        {
            IsGrounded = false;
            IsTouchingWall = false;
            GroundNormal = Vector2.up;
            GroundCollider = null;
        }
    }
}
