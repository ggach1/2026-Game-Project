using UnityEngine;
using UnityEngine.UI;

namespace CIW.Code.System.Stage
{
    // 메뉴 표현과 씬 기반 스테이지 선택을 연결합니다. 구간 프리팹용 테스트 흐름과는 독립적입니다.
    [RequireComponent(typeof(Canvas))]
    public sealed class MenuStageUI : MonoBehaviour
    {
        [SerializeField] WorldDefinition previewWorld;
        [SerializeField] StageNodeView nodePrefab;
        StageSelectUI _select;
        StageProgressService _progress;

        void Start()
        {
            if (previewWorld == null || nodePrefab == null)
            {
                Debug.LogError("MenuStageUI: 메뉴 월드/문 프리팹을 연결하세요.", this);
                return;
            }

            var panel = StageMapPresentation.Rect("Stage Select Panel", transform,
                Vector2.zero, new Vector2(1280, 720));
            panel.gameObject.AddComponent<Image>();
            var nodes = StageMapPresentation.Rect("Stage Nodes", panel,
                Vector2.zero, new Vector2(1000, 350));
            var select = gameObject.AddComponent<StageSelectUI>();
            select.Initialize(panel.gameObject, nodes, nodePrefab);
            _select = select;
            _progress = GetComponent<StageProgressService>();
            if (_progress == null) _progress = gameObject.AddComponent<StageProgressService>();
            select.StageSelected += HandleSelected;
            select.Show(previewWorld, _progress);
        }

        void HandleSelected(StageDefinition stage)
        {
            if (!_progress.IsUnlocked(stage, previewWorld)) return;
            if (SceneStageFlowController.TryStart(stage, gameObject.scene.path)) _select.Hide();
        }

        void OnDestroy()
        {
            if (_select != null) _select.StageSelected -= HandleSelected;
        }
    }
}
