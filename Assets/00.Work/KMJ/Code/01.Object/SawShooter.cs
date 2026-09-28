using System;
using System.Collections;
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
        [SerializeField] private SawDirection sawDirection;
        
        [SerializeField] private bool isLoop = false;
        [SerializeField] private float shootDelay;
        [SerializeField] private int maxShootCnt;
        [SerializeField] private Saw sawPrefab;

        private void Awake()
        {
            StartCoroutine(ShootSaw());
        }

        private void Shoot(SawDirection sawDirection1)
        {
            switch (sawDirection)
            {
                case SawDirection.Right:
                    Saw sawRight = Instantiate(sawPrefab, transform.position, Quaternion.identity);
                    sawRight.SetDirection(Vector3.right);
                    break;
                
                case SawDirection.Left:
                    Saw sawLeft = Instantiate(sawPrefab, transform.position, Quaternion.identity);
                    sawLeft.SetDirection(Vector3.left);
                    break;
                
                case  SawDirection.Up:
                    Saw sawUp = Instantiate(sawPrefab, transform.position, Quaternion.identity);
                    sawUp.SetDirection(Vector3.up);
                    break;
                
                case  SawDirection.Down:
                    Saw sawDown = Instantiate(sawPrefab, transform.position, Quaternion.identity);
                    sawDown.SetDirection(Vector3.down);
                    break;
                
                case SawDirection.None:
                    break;
            }
        }
        
        private IEnumerator ShootSaw()
        {
            if(isLoop)
                while(true)
                {
                    yield return new WaitForSeconds(shootDelay);
                    Shoot(sawDirection);
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