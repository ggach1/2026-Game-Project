using System;
using UnityEngine;
using UnityEngine.Events;

namespace KMJ.Code.Object
{
    public class SmashObject : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private Direction direction;
        [SerializeField] private float forcePower;
        [SerializeField] private bool isOnceDetect;

        [Header("Event")] public UnityEvent smashEvent;

        private int _detectCnt = 0;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            if (isOnceDetect && _detectCnt > 0)
                return;
            
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            if (rb == null)
                return;

            rb.linearVelocity = Vector2.zero;

            Vector2 forceDirection = direction switch
            {
                Direction.Up => Vector2.up,
                Direction.Down => Vector2.down,
                Direction.Left => Vector2.left,
                Direction.Right => Vector2.right,
                _ => Vector2.zero
            };
            
            rb.AddForce(forceDirection * forcePower, ForceMode2D.Impulse);
            smashEvent?.Invoke();
            
            if (isOnceDetect)
                _detectCnt += 1;
        }
    }
}