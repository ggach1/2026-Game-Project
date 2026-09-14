using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.CUH.Code.SwitchSystem
{
    public class SwitchGroup : MonoBehaviour
    {
        [Header("Switches")]
        [SerializeField] private Switch[] switches;

        [Header("Target")]
        [SerializeField] private MonoBehaviour operateObject;
        
        private readonly HashSet<Switch> _subscribedSwitches = new();
        private IActivatable _activatable;
        private bool _hasStarted;

        public bool IsActive { get; private set; }

        private void OnEnable()
        {
            if (ValidateReferences() == false)
            {
                enabled = false;
                return;
            }

            _activatable = (IActivatable)operateObject;

            if (switches != null)
            {
                foreach (Switch linkedSwitch in switches)
                {
                    if (_subscribedSwitches.Add(linkedSwitch))
                        linkedSwitch.OnStateChanged += HandleStateChanged;
                }
            }

            if (_hasStarted)
                RefreshState(false);
        }

        private void Start()
        {
            _hasStarted = true;
            RefreshState(false);
        }

        private void OnDisable()
        {
            foreach (Switch linkedSwitch in _subscribedSwitches)
            {
                if (linkedSwitch != null)
                    linkedSwitch.OnStateChanged -= HandleStateChanged;
            }

            _subscribedSwitches.Clear();
            _activatable = null;
        }

        private void HandleStateChanged(bool isActive)
        {
            if (_hasStarted)
                RefreshState(true);
        }

        private void RefreshState(bool playFeedback)
        {
            if (operateObject == null || _activatable == null) return;

            IsActive = EvaluateState();
            _activatable.SetActive(IsActive, playFeedback);
        }

        private bool EvaluateState()
        {
            if (switches == null || switches.Length == 0) return false;

            foreach (Switch linkedSwitch in switches)
            {
                if (linkedSwitch == null || linkedSwitch.IsActive == false) return false;
            }

            return true;
        }

        private bool ValidateReferences()
        {
            if (operateObject == null || operateObject is not IActivatable)
            {
                Debug.LogError($"{gameObject.name} 스위치 그룹에 IActivatable 대상을 설정해야 합니다.", this);
                return false;
            }

            if (switches == null) return true;

            foreach (Switch linkedSwitch in switches)
            {
                if (linkedSwitch != null) continue;

                Debug.LogError($"{gameObject.name} 스위치 그룹에 비어 있는 스위치 참조가 있습니다.", this);
                return false;
            }

            return true;
        }
    }
}
