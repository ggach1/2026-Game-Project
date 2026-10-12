using CIW.Code.System;
using CIW.Code.System.Interface;
using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(Collider2D))]
    public class DescendingCrusher2D : MonoBehaviour
    {
        [SerializeField] private LayerMask playerLayer = 1 << 3;
        [SerializeField, Min(0f)] private float edgeInset = 0.04f;

        private Collider2D _body;
        private Bounds _previousBounds;
        private readonly Collider2D[] _hits = new Collider2D[16];

        private void OnEnable()
        {
            _body = GetComponent<Collider2D>();
            Physics2D.SyncTransforms();
            _previousBounds = _body.bounds;
        }

        private void FixedUpdate()
        {
            Physics2D.SyncTransforms();
            Bounds currentBounds = _body.bounds;
            float previousBottom = _previousBounds.min.y;
            float currentBottom = currentBounds.min.y;
            _previousBounds = currentBounds;

            if (_body.enabled == false || previousBottom <= currentBottom) return;

            // 내려온 아랫면이 지난 영역만 검사해 옆면 접촉과 정지/상승 중 접촉은 제외합니다.
            float width = Mathf.Max(0f, currentBounds.size.x - edgeInset * 2f);
            if (width <= 0f) return;

            Vector2 center = new(currentBounds.center.x, (previousBottom + currentBottom) * 0.5f);
            Vector2 size = new(width, previousBottom - currentBottom);
            ContactFilter2D filter = new() { useTriggers = false };
            filter.SetLayerMask(playerLayer);
            int count = Physics2D.OverlapBox(center, size, 0f, filter, _hits);

            for (int i = 0; i < count; i++)
            {
                Collider2D other = _hits[i];
                if (other == _body || other.bounds.min.y >= previousBottom) continue;

                Vector2 point = other.ClosestPoint(new Vector2(center.x, currentBottom));
                KillContact2D.TryKill(other, playerLayer,
                    new DeathContext(point, Vector2.down, DeathCause.Crusher));
            }
        }
    }
}
