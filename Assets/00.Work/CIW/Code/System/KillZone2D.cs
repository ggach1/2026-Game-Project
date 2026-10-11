using CIW.Code.System.Interface;
using UnityEngine;

namespace CIW.Code.System
{
    [DisallowMultipleComponent]
    public sealed class KillZone2D : MonoBehaviour
    {
        [SerializeField] LayerMask targetMask = 1 << 3;
        [SerializeField] DeathCause cause = DeathCause.Spike;

        void Awake()
        {
            if (GetComponent<Collider2D>() == null)
            {
                Debug.LogError("KillZone2D와 같은 오브젝트에 Collider2D를 추가하세요.", this);
                enabled = false;
            }
        }

        public void Configure(DeathCause deathCause, LayerMask layers)
        {
            cause = deathCause;
            targetMask = layers;
        }

        void OnTriggerEnter2D(Collider2D other) => TryKill(other);
        void OnTriggerStay2D(Collider2D other) => TryKill(other);
        void OnCollisionEnter2D(Collision2D collision) => TryKill(collision.collider);
        void OnCollisionStay2D(Collision2D collision) => TryKill(collision.collider);

        void TryKill(Collider2D other)
        {
            // Unity는 비활성 Behaviour에도 충돌 메시지를 보낼 수 있으므로 명시적으로 검사합니다.
            if (!isActiveAndEnabled || other == null) return;
            Vector2 origin = transform.position;
            Vector2 direction = ((Vector2)other.bounds.center - origin).normalized;
            KillContact2D.TryKill(other, targetMask,
                new DeathContext(other.ClosestPoint(origin), direction, cause));
        }
    }
}
