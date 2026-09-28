using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingGround : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float fallSpeed = 12f;
        [SerializeField, Min(0f)] private float fallDistance = 20f;

        private Rigidbody2D _rigidbody;
        private Vector2 _targetPosition;
        private bool _hasStartedFalling;

        public bool IsFalling { get; private set; }
        
        private void Awake() => InitializeBody();

        private void InitializeBody()
        {
            if (_rigidbody != null) return;

            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }

        public void Fall()
        {
            if (isActiveAndEnabled == false || _hasStartedFalling) return;

            if (fallSpeed <= 0f || fallDistance < 0f)
            {
                Debug.LogError($"{gameObject.name}의 낙하 속도는 0보다 크고, 낙하 거리는 0 이상이어야 합니다.", this);
                return;
            }

            InitializeBody();
            _targetPosition = _rigidbody.position + Vector2.down * fallDistance;
            _hasStartedFalling = true;
            IsFalling = true;
        }

        private void FixedUpdate()
        {
            if (IsFalling == false) return;

            if (_rigidbody.position == _targetPosition)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                IsFalling = false;
                return;
            }

            Vector2 nextPosition = Vector2.MoveTowards(
                _rigidbody.position, _targetPosition, fallSpeed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(nextPosition);
        }

        private void OnDisable()
        {
            if (_rigidbody == null) return;

            _rigidbody.linearVelocity = Vector2.zero;
        }
    }
}
