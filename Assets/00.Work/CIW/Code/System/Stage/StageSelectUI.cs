using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIW.Code.System.Stage
{
    public class StageSelectUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] RectTransform nodeRoot;
        [SerializeField] StageNodeView nodePrefab;
        [SerializeField] string mapTitle = "STAGE SELECT";

        readonly List<StageNodeView> _nodes = new();
        StageMapPresentation _presentation;

        public event Action<StageDefinition> StageSelected;

        public void Show(WorldDefinition world, StageProgressService progress)
        {
            ClearNodes();
            panel.SetActive(true);
            _presentation ??= new StageMapPresentation(panel, nodeRoot, mapTitle);
            _presentation.Refresh(world, progress);

            foreach (var entry in world.Entries)
            {
                var stage = entry.Stage;

                if (stage == null) continue;

                var node = Instantiate(nodePrefab, nodeRoot);

                var rect = (RectTransform)node.transform;
                rect.anchoredPosition = entry.Position;

                node.Bind(stage, progress.IsUnlocked(stage, world), progress.IsCompleted(stage.StageId), HandleSelected);
                node.UseMapStyle(HandleFocused);

                _nodes.Add(node);
            }
            // 다음 미완료 문을 우선 선택하고, 전부 완료했으면 재도전 가능한 첫 문을 선택합니다.
            var next = _nodes.Find(node => node.Unlocked && !node.Completed) ?? _nodes.Find(node => node.Unlocked);
            if(next != null) next.Focus();
        }

        void LateUpdate()
        {
            if(panel.activeInHierarchy) _presentation?.Fit();
        }

        void HandleFocused(StageNodeView selected)
        {
            foreach(var node in _nodes) node.SetFocused(node == selected);
            _presentation.Describe(selected);
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        private void HandleSelected(StageDefinition stage)
        {
            StageSelected?.Invoke(stage);
        }

        private void ClearNodes()
        {
            foreach (var node in _nodes)
            {
                if (node == null) continue;

                node.gameObject.SetActive(false);
                Destroy(node.gameObject);
            }

            _nodes.Clear();
        }
    }
}
