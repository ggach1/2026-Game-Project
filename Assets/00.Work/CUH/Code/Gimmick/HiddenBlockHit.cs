using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    public class HiddenBlockHit : MonoBehaviour
    {
        [SerializeField] private HiddenBlock hiddenBlock;
        [SerializeField] private LayerMask playerLayer;

        private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision);
        private void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision);

        private void HandleCollision(Collision2D collision)
        {
            if (isActiveAndEnabled == false || hiddenBlock == null || hiddenBlock.IsRevealed) return;

            Rigidbody2D body = collision.rigidbody;
            if (body == null || (playerLayer.value & (1 << body.gameObject.layer)) == 0) return;
            if (Vector2.Dot(collision.relativeVelocity, transform.up) <= 0f) return;

            // Effector로 통과하는 중에도 콜백이 오므로 상승 방향과 아래쪽 접촉을 함께 확인합니다.
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (contact.enabled == false || Vector2.Dot(contact.normal, transform.up) < 0.5f) continue;

                hiddenBlock.Reveal();
                return;
            }
        }
    }
}
