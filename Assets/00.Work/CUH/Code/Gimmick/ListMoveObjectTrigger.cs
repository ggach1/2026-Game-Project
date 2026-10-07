using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ListMoveObjectTrigger : MonoBehaviour
    {
        [SerializeField] private ListMoveObject moveObject;
        [SerializeField] private ListMoveObject previousMoveObject;
        [SerializeField] private LayerMask playerLayer;

        private void OnTriggerEnter2D(Collider2D other) => HandleCollision(other);
        private void OnTriggerStay2D(Collider2D other) => HandleCollision(other);

        private void HandleCollision(Collider2D other)
        {
            if (isActiveAndEnabled == false || moveObject == null) return;
            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0) return;
            if (previousMoveObject != null && previousMoveObject.HasInteracted == false) return;

            moveObject.Interact();
        }
    }
}
