using UnityEngine;
using UnityEngine.UI;

namespace CIW.Code.System.Stage
{
    // MenuScene의 표현만 구성합니다. 실제 스테이지 흐름/저장/테스트 키는 연결하지 않습니다.
    [RequireComponent(typeof(Canvas))]
    public sealed class MenuStageUI : MonoBehaviour
    {
        [SerializeField] WorldDefinition previewWorld;
        [SerializeField] StageNodeView nodePrefab;

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
            // null 진행 서비스는 UI 전용 모드입니다. 클릭해도 씬 전환/클리어 저장이 발생하지 않습니다.
            select.Show(previewWorld, null);
        }
    }
}
