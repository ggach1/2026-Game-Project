using UnityEngine;

namespace KMJ.Code.Object
{
    public class DropSaw : MonoBehaviour, IInteractable
    {
        [SerializeField] private LayerMask playerMask;

        private Rigidbody2D _rbCompo;

        private void Awake()
        {
            _rbCompo = GetComponentInChildren<Rigidbody2D>();

            if (_rbCompo == null)
            {
                Debug.LogError("This Object isn't have RigidBody component!");
                return;
            }

            _rbCompo.gravityScale = 0;
        }

        public void Interact()
        {
            _rbCompo.gravityScale = 1;
        }
    }
}