using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _00.Work.CUH.Code.Title
{
    public class TitleSceneController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string menuScenePath = "Assets/00.Work/GameScene/MenuScene.unity";

        [Header("UI")]
        [SerializeField] private CanvasGroup mainMenu;
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TitleSettingsPanel settingsPanel;

        private bool _isLoading;

        private void Awake()
        {
            DisplaySettings.Load();
            startButton.onClick.AddListener(StartGame);
            settingsButton.onClick.AddListener(OpenSettings);
            exitButton.onClick.AddListener(ExitGame);
            settingsPanel.OnClosed += CloseSettings;
        }

        private void Start() => startButton.Select();

        private void Update()
        {
            if (settingsPanel.gameObject.activeSelf && Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
                settingsPanel.Cancel();
        }

        private void StartGame()
        {
            if (_isLoading) return;
            if (Application.CanStreamedLevelBeLoaded(menuScenePath) == false)
            {
                Debug.LogError($"메뉴 씬을 빌드 목록에 등록해야 합니다: {menuScenePath}", this);
                return;
            }
            StartCoroutine(LoadMenu());
        }

        private IEnumerator LoadMenu()
        {
            _isLoading = true;
            SetMenuInteractable(false);
            yield return SceneManager.LoadSceneAsync(menuScenePath);
        }

        private void OpenSettings()
        {
            if (_isLoading) return;
            SetMenuInteractable(false);
            settingsPanel.Show();
        }

        private void CloseSettings()
        {
            SetMenuInteractable(true);
            settingsButton.Select();
        }

        private void SetMenuInteractable(bool interactable)
        {
            mainMenu.interactable = interactable;
            mainMenu.blocksRaycasts = interactable;
        }

        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            startButton.onClick.RemoveListener(StartGame);
            settingsButton.onClick.RemoveListener(OpenSettings);
            exitButton.onClick.RemoveListener(ExitGame);
            settingsPanel.OnClosed -= CloseSettings;
        }
    }
}
