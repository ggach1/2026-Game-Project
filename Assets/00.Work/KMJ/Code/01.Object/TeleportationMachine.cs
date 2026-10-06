using System;
using UnityEngine;

namespace KMJ.Code.Object
{
    public class TeleportationMachine : MonoBehaviour   
    {
        [Header("Setting")] 
        [SerializeField] private Transform targetTrm;
        [SerializeField] private LayerMask whatIsTarget;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((whatIsTarget.value & (1 << other.gameObject.layer)) == 0)
                return;

            other.gameObject.transform.position = targetTrm.position;
        }
    }
}