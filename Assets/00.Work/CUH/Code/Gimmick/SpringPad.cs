using CIW.Code.Player;
using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpringBounce2D))]
    public class SpringPad : MonoBehaviour
    {
        [Header("Jump")]
        [SerializeField, Min(0.01f)] private float bouncePower = 16f;
        [SerializeField, Min(0.01f)] private float boostPower = 24f;
        [Tooltip("자동 반동이 시작된 뒤 점프 입력을 추가로 받는 시간입니다.")]
        [SerializeField, Min(0f)] private float boostWindow = 0.16f;
        [SerializeField] private LayerMask playerLayer = 1 << 3;

        [Header("Visual")]
        [Tooltip("눌림 연출만 적용할 자식입니다. 충돌체는 이 자식 밖에 둡니다.")]
        [SerializeField] private Transform springVisual;
        [SerializeField, Min(0.01f)] private float recoveryTime = 0.3f;

        private BoxCollider2D _trigger;
        private SpringBounce2D _bounce;
        private Vector3 _visualScale;
        private float _animationTime;

        private void Awake()
        {
            _trigger = GetComponent<BoxCollider2D>();
            _bounce = GetComponent<SpringBounce2D>();
            _trigger.isTrigger = true;
            if (springVisual != null) _visualScale = springVisual.localScale;
        }

        private void OnTriggerEnter2D(Collider2D other) => HandleLanding(other);
        private void OnTriggerStay2D(Collider2D other) => HandleLanding(other);

        private void HandleLanding(Collider2D other)
        {
            if (isActiveAndEnabled == false || other.isTrigger) return;

            Player player = other.GetComponentInParent<Player>();
            if (player == null || player.IsAlive == false || player.BodyCollider != other) return;
            if ((playerLayer.value & (1 << player.gameObject.layer)) == 0) return;
            if (player.Motor == null || player.Rules == null) return;
            // 위쪽에서 내려오는 몸통만 받습니다. 옆면/아랫면과 발 센서는 반동을 만들지 않습니다.
            if (Vector2.Dot(player.Rules.GravityDirection, Vector2.down) < 0.99f ||
                player.Motor.GetVerticalSpeed() > 0f ||
                other.bounds.min.y < _trigger.bounds.min.y - 0.08f)
                return;

            if (_bounce.TryBounce(player, bouncePower, boostPower, boostWindow))
                _animationTime = recoveryTime;
        }

        private void Update()
        {
            if (springVisual == null || _animationTime <= 0f) return;

            _animationTime = Mathf.Max(0f, _animationTime - Time.deltaTime);
            float progress = 1f - _animationTime / recoveryTime;
            float height = progress < 0.25f
                ? Mathf.Lerp(0.45f, 1.2f, progress / 0.25f)
                : Mathf.Lerp(1.2f, 1f, (progress - 0.25f) / 0.75f);
            springVisual.localScale = Vector3.Scale(_visualScale, new Vector3(1f, height, 1f));
        }

        private void OnDisable()
        {
            _animationTime = 0f;
            if (springVisual != null) springVisual.localScale = _visualScale;
        }

        private void OnValidate()
        {
            bouncePower = Mathf.Max(0.01f, bouncePower);
            boostPower = Mathf.Max(bouncePower, boostPower);
            boostWindow = Mathf.Max(0f, boostWindow);
            recoveryTime = Mathf.Max(0.01f, recoveryTime);
        }
    }
}
