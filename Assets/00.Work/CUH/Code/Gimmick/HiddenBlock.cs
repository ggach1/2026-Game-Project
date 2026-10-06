using System;
using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    public class HiddenBlock : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private SpriteRenderer blockRenderer;
        [SerializeField] private Collider2D solidCollider;
        [SerializeField] private Collider2D hitCollider;

        public bool IsRevealed { get; private set; }

        public event Action OnRevealed;

        private void Awake()
        {
            blockRenderer.enabled = false;
            solidCollider.enabled = false;
            hitCollider.enabled = true;
        }

        public void Reveal()
        {
            if (isActiveAndEnabled == false || IsRevealed) return;

            IsRevealed = true;
            blockRenderer.enabled = true;
            solidCollider.enabled = true;
            hitCollider.enabled = false;
            OnRevealed?.Invoke();
        }
    }
}
