using UnityEngine;
using UnityEngine.Events;

namespace KMJ.Code.Object
{
    public class TeleportationMachine : MonoBehaviour   
    {
        [Header("Setting")] 
        [SerializeField] private Transform targetTrm;
        [SerializeField] private LayerMask whatIsTarget;

        public UnityEvent OnTeleportSuccess;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((whatIsTarget.value & (1 << other.gameObject.layer)) == 0)
                return;
            
            OnTeleportSuccess?.Invoke();
            other.gameObject.transform.position = targetTrm.position;
        }
    }
}