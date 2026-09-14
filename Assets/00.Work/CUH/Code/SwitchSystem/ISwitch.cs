using System;

namespace _00.Work.CUH.Code.SwitchSystem
{
    public interface ISwitch
    {
        public bool IsActive { get; }
        public event Action<bool> OnStateChanged;

        public void SetActive(bool isActive);
        public void ToggleSwitch();
    }
}
