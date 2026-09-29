using System;
using UnityEngine;
using UnityEngine.Events;

namespace KMJ.Code.Object
{
    public class InteractSignObject : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private LayerMask targetLayer;
        
        [Header("Events")]
        [SerializeField] private UnityEvent OnInteractEvent;

        private bool _isActive = true;

        public void IsActiveSign(bool isActive)
        {
            _isActive = isActive;
        }
        
        private void Sign()
        {
            if (!_isActive) return;
            
            OnInteractEvent?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                Sign();
            } 
        } 
    }
}