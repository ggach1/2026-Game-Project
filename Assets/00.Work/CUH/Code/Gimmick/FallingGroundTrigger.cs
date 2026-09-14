using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    public class FallingGroundTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask playerLayer;
        
        private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision);
        private void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision);
        
        private void HandleCollision(Collision2D collision)
        {
            
        }
    }
}
