using System;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace KMJ.Code.Object
{
    public class Saw : MonoBehaviour, IInteractable
    {
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private float minGravityScale = 0;
        [SerializeField] private float maxGravityScale = 1;
        [SerializeField] private float moveSpeed = 5;
        
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
        private void KillEnemy() => Debug.Log("적을 처치함");

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
                KillEnemy();
            }
        }
    }
}