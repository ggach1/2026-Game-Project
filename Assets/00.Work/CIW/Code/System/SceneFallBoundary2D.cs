using CIW.Code.System.Interface;
using UnityEngine;

namespace CIW.Code.System
{
    /// <summary>현재 씬의 시작 지형 아래에 낙사 영역을 구성합니다. 리스폰/씬 재시작은 담당하지 않습니다.</summary>
    [DisallowMultipleComponent]
    public sealed class SceneFallBoundary2D : MonoBehaviour
    {
        [SerializeField] LayerMask targetMask = 1 << 3;
        [SerializeField, Min(1f)] float bottomMargin = 5f;
        [SerializeField, Min(1f)] float horizontalMargin = 30f;
        [SerializeField, Min(1f)] float depth = 20f;
        BoxCollider2D _boundary;

        void Start()
        {
            // 런타임에 생성된 지형과 Transform 기반 Collider의 월드 좌표를 반영합니다.
            Physics2D.SyncTransforms();
            bool found = false;
            Bounds bounds = default;
            foreach (var root in gameObject.scene.GetRootGameObjects())
            foreach (var collider in root.GetComponentsInChildren<Collider2D>())
            {
                // 플레이어/톱 센서/출구 영역은 지형 경계 계산에서 제외합니다.
                if (!collider.enabled || collider.isTrigger ||
                    collider.GetComponentInParent<IKillable>() != null ||
                    collider.GetComponentInParent<KillZone2D>() != null)
                    continue;
                if (!found) { bounds = collider.bounds; found = true; }
                else bounds.Encapsulate(collider.bounds);
            }

            if (!found)
            {
                Debug.LogWarning("낙사 영역을 만들 지형 Collider가 없습니다. 수동 KillZone을 배치하세요.", this);
                return;
            }

            // 시작 경계를 고정합니다. 떨어지는 발판을 매 프레임 따라가면 낙사 영역도 계속 내려갑니다.
            var zone = new GameObject("Fall KillZone (Generated)");
            zone.transform.SetParent(transform, false);
            zone.transform.position = new Vector3(bounds.center.x,
                bounds.min.y - bottomMargin - depth * .5f, 0f);
            _boundary = zone.AddComponent<BoxCollider2D>();
            _boundary.isTrigger = true;
            _boundary.size = new Vector2(bounds.size.x + horizontalMargin * 2f, depth);
            zone.AddComponent<KillZone2D>().Configure(DeathCause.Fall, targetMask);
        }

        void OnEnable()
        {
            if (_boundary != null) _boundary.gameObject.SetActive(true);
        }

        void OnDisable()
        {
            if (_boundary != null) _boundary.gameObject.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            if (_boundary == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_boundary.bounds.center, _boundary.bounds.size);
        }
    }
}
