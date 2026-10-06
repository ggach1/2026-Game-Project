using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using CIW.Code.System.Interface;

namespace CIW.Code.System.Stage
{
    // 테스트 씬 전용: 구간 표시, 맵 밖 낙사, 개발용 진행 초기화 입력을 담당합니다.
    public class StageFlowTestHUD : MonoBehaviour
    {
        [SerializeField] StageFlowController flow;
        [SerializeField] Player.Player player;
        [SerializeField] TMP_Text status;
        [SerializeField] GameObject backButton;
        GameObject _controls;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] Key resetProgressKey = Key.F9;
        [SerializeField, Min(0.1f)] float resetHoldSeconds = 1.5f;
        TMP_Text _resetHint;
        float _resetHeld;
        bool _resetConsumed;
#endif

        void Start()
        {
            // 지도 화면에는 테스트 조작 안내를 중복 노출하지 않습니다.
            _controls = status.transform.parent.Find("Controls")?.gameObject;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _resetHint = StageMapPresentation.Label("Progress Reset Hint", status.transform.parent,
                "", new Vector2(0, 320), new Vector2(1100, 30), 16);
#endif
        }

        private void Update()
        {
            bool selecting = flow.State == StageFlowState.StageSelect;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UpdateResetInput(selecting);
#endif
            backButton.SetActive(!selecting);
            status.gameObject.SetActive(!selecting);
            if(_controls != null) _controls.SetActive(!selecting);
            status.text = selecting ? "SELECT A STAGE" :
                $"{flow.CurrentStage?.DisplayName}  /  Section {flow.CurrentSectionIndex + 1}  /  {flow.State}";

            if (flow.State == StageFlowState.Playing && player.IsAlive && player.transform.position.y < -6f)
                player.Kill(new DeathContext(player.transform.position, Vector2.zero, DeathCause.Unknown));
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void UpdateResetInput(bool selecting)
        {
            _resetHint.gameObject.SetActive(selecting);
            var keyboard = Keyboard.current;
            if (!selecting || keyboard == null || resetProgressKey == Key.None ||
                !Application.isFocused || !keyboard[resetProgressKey].isPressed)
            {
                _resetHeld = 0f;
                _resetConsumed = false;
                _resetHint.text = $"HOLD {resetProgressKey} ({resetHoldSeconds:0.#}s) TO RESET PROGRESS";
                return;
            }

            // 길게 누른 한 번만 실행합니다. 시간 배율과 무관하며 키를 떼면 누적 시간이 초기화됩니다.
            if (_resetConsumed) return;
            _resetHeld += Time.unscaledDeltaTime;
            _resetHint.text = $"RESET PROGRESS... {Mathf.Clamp01(_resetHeld / resetHoldSeconds):P0}";
            if (_resetHeld < resetHoldSeconds) return;

            _resetConsumed = true;
            if (flow.TryResetProgressFromStageSelect())
            {
                _resetHint.text = "PROGRESS RESET";
                Debug.Log("스테이지 클리어 기록 초기화 완료: 현재 StageProgressService의 저장 키만 초기화했습니다.");
            }
        }

        void OnDisable()
        {
            _resetHeld = 0f;
            _resetConsumed = false;
            if (_resetHint != null) _resetHint.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (_resetHint != null) Destroy(_resetHint.gameObject);
        }
#endif
    }
}
