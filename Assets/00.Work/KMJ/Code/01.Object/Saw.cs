using System;
using System.Collections;
using System.Numerics;
using DevLib.ObjectPool.Runtime;
using CIW.Code.System;
using CIW.Code.System.Interface;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace KMJ.Code.Object
{
    public class Saw : MonoBehaviour, IInteractable, IPoolable, ISceneRetryCleanup
    {
        [Header("Pool")]
        [field: SerializeField] public PoolItemSO PoolItem { get; set; }
        [SerializeField] private PoolManagerSO poolManager;
        
        [Space(5)]
        [Header("Setting")]
        [SerializeField] private LayerMask targetMask;
        
        [Space(10) ,SerializeField] private bool isOwnDirection;
        [SerializeField] private Direction ownDirection;
        
        [Range(0,3), Space(10)]
        [SerializeField] private float minGravityScale = 0;
        
        [Range(1,10)]
        [SerializeField] private float maxGravityScale = 1;
        
        [Range(0,10)]
        [SerializeField] private float moveSpeed = 5;
        
        [Range(0,10)]
        [SerializeField] private float lifeTime = 0;
        
        public GameObject GameObject => gameObject;
        
        private Rigidbody2D _rbCompo;
        private Vector2 _moveDirection = Vector2.zero;
        private bool _borrowedFromPool;
        
        

        private void Awake()
        {
            _rbCompo = GetComponentInChildren<Rigidbody2D>();

            if (_rbCompo == null)
            {
                Debug.LogError("This Object isn't have RigidBody component!");
                return;
            }

            _rbCompo.gravityScale = minGravityScale;

            // 만약 isOwnDirection이 false면 ownDirection의 값은 None이 됨
            if (!isOwnDirection)
                ownDirection = Direction.None;
        }


        /// <summary>
        /// 떨어 뜨릴때 사용함
        /// </summary>
        public void Interact() => _rbCompo.gravityScale = maxGravityScale;

        /// <summary>
        /// 새로운 방향으로 설정함
        /// </summary>
        /// <param name="direction"></param>
        public void SetDirection(Vector2 direction) => _moveDirection = direction.normalized;

        /// <summary>
        /// 파라미터로 정해놓은 방향으로 움직임
        /// </summary>
        public void MoveOwnDirection()
        {
            if (ownDirection == Direction.None)
                return;
            
            switch (ownDirection)
            {
                case Direction.Up:
                    _moveDirection = Vector2.up;
                    break;
                case Direction.Down:
                    _moveDirection = Vector2.down;
                    break;
                case Direction.Left:
                    _moveDirection = Vector2.left;
                    break;
                case Direction.Right:
                    _moveDirection = Vector2.right;
                    break;
                default:
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_moveDirection != Vector2.zero)
            {
                _rbCompo.linearVelocity = _moveDirection * moveSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryKillTarget(other);
        private void OnTriggerStay2D(Collider2D other) => TryKillTarget(other);

        private void TryKillTarget(Collider2D other)
        {
            if (!isActiveAndEnabled || other == null) return;
            // 센서가 먼저 닿아도 죽이지 않고, 실제 몸통 접촉 시 플레이어의 사망 이벤트를 발생시킵니다.
            Vector2 origin = transform.position;
            KillContact2D.TryKill(other, targetMask, new DeathContext(other.ClosestPoint(origin),
                ((Vector2)other.bounds.center - origin).normalized, DeathCause.Saw));
        }

        public void ResetItem()
        {
            _borrowedFromPool = true;
            StopAllCoroutines();
            _moveDirection = Vector2.zero;
            _rbCompo.gravityScale = minGravityScale;
            _rbCompo.linearVelocity = Vector2.zero; 
            _rbCompo.angularVelocity = 0f;
            StartCoroutine(WaitPushObject());
        }

        public void CleanupBeforeSceneRetry() => ReturnToPool();

        private void ReturnToPool()
        {
            // 씬에 직접 배치된 톱은 풀에 넣지 않습니다. Pop -> ResetItem을 거친 톱만 반환합니다.
            if (!_borrowedFromPool || poolManager == null) return;
            _borrowedFromPool = false;
            StopAllCoroutines();
            _moveDirection = Vector2.zero;
            _rbCompo.linearVelocity = Vector2.zero;
            _rbCompo.angularVelocity = 0f;
            poolManager.Push(this);
        }
        
        private IEnumerator WaitPushObject()
        {
            yield return new WaitForSeconds(lifeTime);
            ReturnToPool();
        }
    }
}
