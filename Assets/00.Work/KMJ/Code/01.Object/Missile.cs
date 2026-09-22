using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace KMJ.Code.Object
{
    public class Missile : MonoBehaviour
    {
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private UnityEvent onMissileTimeEvent;
        [SerializeField] private float eventTime;
        [SerializeField] private Saw sawPrefab;

        [SerializeField] private float waitTime;
        
        private void Awake()
        {
            StartCoroutine(EventStartTimer());
        }

        private IEnumerator EventStartTimer()
        {
            yield return new WaitForSeconds(eventTime);
            
            onMissileTimeEvent.Invoke();
        }

        private IEnumerator MissileDetectedGround()
        {
            yield return new WaitForSeconds(waitTime);
            
            Saw saw1 = Instantiate(sawPrefab, transform.position, Quaternion.identity);
            Saw saw2 = Instantiate(sawPrefab, transform.position, Quaternion.identity);
                
            saw1.SetDirection(Vector2.right);
            saw2.SetDirection(Vector2.left);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((whatIsGround.value & (1 << collision.gameObject.layer)) != 0)
            {
                StartCoroutine(MissileDetectedGround());    
            }
        }
    }
}