namespace _00.Work.CUH.Code.SwitchSystem
{
    public interface IActivatable
    {
        public bool IsActive { get; }

        public void SetActive(bool isActive, bool playFeedback = true);
        public void Activate();
        public void Deactivate();
    }
}
