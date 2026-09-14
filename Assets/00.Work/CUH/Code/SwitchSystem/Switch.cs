using System;
using UnityEngine;

namespace _00.Work.CUH.Code.SwitchSystem
{
    public class Switch : MonoBehaviour, ISwitch
    {
        [field: SerializeField] public bool IsActive { get; private set; }

        public event Action<bool> OnStateChanged;

        public void SetActive(bool isActive)
        {
            if (IsActive == isActive) return;

            IsActive = isActive;
            OnStateChanged?.Invoke(IsActive);
        }

        public void ToggleSwitch() => SetActive(!IsActive);

        protected virtual void OnDestroy()
        {
            SetActive(false);
        }
    }
}
