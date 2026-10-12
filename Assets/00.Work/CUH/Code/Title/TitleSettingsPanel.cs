using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _00.Work.CUH.Code.Title
{
    public class TitleSettingsPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text statusText;

        private List<Vector2Int> _resolutions;

        public event Action OnClosed;

        private void Awake()
        {
            applyButton.onClick.AddListener(Apply);
            backButton.onClick.AddListener(Close);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _resolutions = DisplaySettings.GetResolutions();
            List<string> options = new();
            foreach (Vector2Int resolution in _resolutions)
                options.Add($"{resolution.x} x {resolution.y}");

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            int index = _resolutions.IndexOf(DisplaySettings.GetSavedResolution());
            if (index < 0) index = _resolutions.IndexOf(new Vector2Int(Screen.width, Screen.height));
            resolutionDropdown.SetValueWithoutNotify(Mathf.Max(0, index));
            resolutionDropdown.RefreshShownValue();
            fullscreenToggle.SetIsOnWithoutNotify(DisplaySettings.GetSavedFullscreen());
            statusText.text = "Select Apply to save your display settings.";
            fullscreenToggle.Select();
        }

        public void Cancel()
        {
            if (resolutionDropdown.IsExpanded)
            {
                resolutionDropdown.Hide();
                resolutionDropdown.Select();
                return;
            }
            Close();
        }

        private void Apply()
        {
            if (_resolutions == null || resolutionDropdown.value >= _resolutions.Count) return;
            DisplaySettings.Apply(_resolutions[resolutionDropdown.value], fullscreenToggle.isOn);
            statusText.text = "Display settings saved.";
        }

        private void Close()
        {
            resolutionDropdown.Hide();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }

        private void OnDestroy()
        {
            applyButton.onClick.RemoveListener(Apply);
            backButton.onClick.RemoveListener(Close);
        }
    }
}
