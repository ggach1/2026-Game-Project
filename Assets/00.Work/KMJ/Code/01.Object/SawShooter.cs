using System;
using System.Collections;
using DevLib.ObjectPool.Editor;
using DevLib.ObjectPool.Runtime;
using UnityEngine;

namespace KMJ.Code.Object
{
    public enum SawDirection
    {
        Right,
        Left,
        Up,
        Down,
        None
    }
    public class SawShooter : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private SawDirection sawDirection;
        [SerializeField] private bool isLoop = false;

        [Range(0f, 10f)]
        [SerializeField] private float crackCnt;
        
        [Range(0,10)]
        [SerializeField] private float shootDelay;
        
        [Range(0,100)]
        [SerializeField] private int maxShootCnt;

        [SerializeField] private Transform shootTrm;
        [SerializeField] private Vector3 shootRotation;
        
        [Space(10)]
        [Header("Pool")]
        [SerializeField] private PoolItemSO sawPrefab;
        [SerializeField] private PoolManagerSO poolManager;

        
        private void Awake()
        {
            StartCoroutine(ShootSaw());
        }

        private void Shoot(SawDirection sawDirection1)
        {
            Saw sawObj = poolManager.Pop<Saw>(sawPrefab);
            sawObj.transform.position = shootTrm.position;
            sawObj.transform.rotation = Quaternion.Euler(shootRotation);
         
            switch (sawDirection)
            {
                case SawDirection.Right:
                    sawObj.SetDirection(Vector3.right);
                    break;
                
                case SawDirection.Left:
                    sawObj.SetDirection(Vector3.left);
                    break;
                
                case  SawDirection.Up:
                    sawObj.SetDirection(Vector3.up);
                    break;
                
                case  SawDirection.Down:
                    sawObj.SetDirection(Vector3.down);
                    break;
                
                case SawDirection.None:
                    break;
            }
        }
        
        private IEnumerator ShootSaw()
        {
            if (isLoop)
            {
                int cnt = 0;
                
                while(true)
                {
                    cnt += 1;
                    
                    Debug.Log(cnt);
                    yield return new WaitForSeconds(shootDelay);
                    if (cnt >= crackCnt)
                    {
                        Shoot(sawDirection);
                        yield return new WaitForSeconds(0.05f);
                        Shoot(sawDirection);
                        cnt = 0;
                    }
                    else
                        Shoot(sawDirection);
                }
            }
            else
            {
                for (int i = 0; i < maxShootCnt; i++)
                {
                    yield return new WaitForSeconds(shootDelay);
                    Shoot(sawDirection);
                }
            }
        }
    }
}