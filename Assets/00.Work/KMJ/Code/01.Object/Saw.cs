using System;
using System.Collections;
using System.Numerics;
using DevLib.ObjectPool.Runtime;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace KMJ.Code.Object
{
    public class Saw : MonoBehaviour, IInteractable, IPoolable
    {
        [Header("Pool")]
        [field: SerializeField] public PoolItemSO PoolItem { get; set; }
        [SerializeField] private PoolManagerSO poolManager;
        
        [Space(5)]
        [Header("Setting")]
        [SerializeField] private LayerMask targetMask;
        
        [Range(0,3)]
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
        
        

        private void Awake()
        {
            _rbCompo = GetComponentInChildren<Rigidbody2D>();

            if (_rbCompo == null)
            {
                Debug.LogError("This Object isn't have RigidBody component!");
                return;
            }

            _rbCompo.gravityScale = minGravityScale;
        }



        public void Interact() => _rbCompo.gravityScale = maxGravityScale;
        

        public void SetDirection(Vector2 direction) => _moveDirection = direction.normalized;

        private void KillEnemy(Collider2D collider2D)
        {
            collider2D.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (_moveDirection != Vector2.zero)
            {
                _rbCompo.linearVelocity = _moveDirection * moveSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetMask.value & (1 << other.gameObject.layer)) != 0)
            {
                KillEnemy(other);
            }
        }

        public void ResetItem()
        {
            _rbCompo.gravityScale = minGravityScale;
            _rbCompo.linearVelocity = Vector2.zero; 
            StartCoroutine(WaitPushObject());
        }
        
        private IEnumerator WaitPushObject()
        {
            yield return new WaitForSeconds(lifeTime);
            poolManager.Push(this);
            gameObject.SetActive(false);
        }
    }
}
