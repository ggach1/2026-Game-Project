using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    public class FallingGroundTrigger : MonoBehaviour
    {
        [SerializeField] private FallingGround fallingGround;
        [SerializeField] private LayerMask playerLayer;
        
        private void OnTriggerEnter2D(Collider2D other) => HandleCollision(other);
        private void OnTriggerStay2D(Collider2D collision) => HandleCollision(collision);
        
        private void HandleCollision(Collider2D other)
        {
            if (isActiveAndEnabled == false || fallingGround == null) return;

            GameObject target = other.gameObject;
            if ((playerLayer.value & (1 << target.layer)) == 0) return;

            fallingGround.Fall();
        }
    }
}
