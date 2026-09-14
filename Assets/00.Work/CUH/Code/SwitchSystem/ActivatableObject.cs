using UnityEngine;
using UnityEngine.Events;

namespace _00.Work.CUH.Code.SwitchSystem
{
    public class ActivatableObject : MonoBehaviour, IActivatable
    {
        [Header("State")]
        [SerializeField] private UnityEvent<bool> onStateApplied = new();

        [Header("Feedback")]
        [SerializeField] private UnityEvent onActivated = new();
        [SerializeField] private UnityEvent onDeactivated = new();

        private bool _hasAppliedState;

        public bool IsActive { get; private set; }

        protected virtual void Start()
        {
            SetActive(IsActive, false);
        }

        public void Activate() => SetActive(true);
        public void Deactivate() => SetActive(false);

        public void SetActive(bool isActive, bool playFeedback = true)
        {
            if (_hasAppliedState && IsActive == isActive) return;

            bool stateChanged = IsActive != isActive;
            IsActive = isActive;
            _hasAppliedState = true;

            ApplyState(IsActive);

            if (IsActive != isActive || stateChanged == false || playFeedback == false) return;

            if (IsActive)
                onActivated.Invoke();
            else
                onDeactivated.Invoke();
        }

        protected virtual void ApplyState(bool isActive)
        {
            onStateApplied.Invoke(isActive);
        }
    }
}
