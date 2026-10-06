using System.Collections;
using DevLib.ObjectPool.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace KMJ.Code.Object
{
    public class Missile : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private LayerMask whatIsGround;
        
        [Range(0,10)]
        [SerializeField] private float eventTime;
        
        [Range(0,5)]
        [SerializeField] private float waitTime;

        [Space(10)]
        [Header("Pool")]
        
        [SerializeField] private PoolItemSO sawPoolItem;
        [SerializeField] private PoolManagerSO poolManager;
        
        [Space(10)]
        [Header("Event")]
        [SerializeField] private UnityEvent onMissileTimeEvent;
        
        
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


            Saw saw1 = poolManager.Pop<Saw>(sawPoolItem);
            Saw saw2 = poolManager.Pop<Saw>(sawPoolItem);

            saw1.transform.position = transform.position;
            saw2.transform.position = transform.position;
            
            saw1.transform.rotation = Quaternion.identity;  
            saw2.transform.rotation = Quaternion.identity;
                
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