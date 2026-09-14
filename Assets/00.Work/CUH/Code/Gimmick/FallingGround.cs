using UnityEngine;
using UnityEngine.Tilemaps;

namespace _00.Work.CUH.Code.Gimmick
{
    public class FallingGround : MonoBehaviour
    {
        [SerializeField] private float fallSpeed = 12f;
        [SerializeField] private float fallDistance = 20f;

        public bool IsFalling { get; private set; }
        
        public void Fall()
        {
            if (IsFalling) return;
            
            IsFalling = true;
            
        }

        private void Update()
        {
            if (IsFalling == false) return;
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        }
    }
}
