using System.Collections.Generic;
using UnityEngine;

namespace CIW.Code.System.Stage
{
    [CreateAssetMenu(fileName = "StageDef", menuName = "SO/Stage Definition")]
    public class StageDefinition : ScriptableObject
    {
        [SerializeField] string stageId;
        [SerializeField] string displayName;

        [SerializeField] bool initiallyUnlocked;
        [SerializeField] SectionDefinition[] sections;
        [Tooltip("씬 기반 스테이지의 구간 순서. 비어 있으면 기존 프리팹 구간을 사용합니다.")]
        [SerializeField] string[] scenePaths;

        [Tooltip("이 스테이지를 완료했을 때 열릴 스테이지의 ID")]
        [SerializeField] string[] unlockStage;

        public string StageId => stageId;
        public string DisplayName => displayName;
        public bool InitiallyUnlocked => initiallyUnlocked;

        public IReadOnlyList<SectionDefinition> Sections => sections;
        public IReadOnlyList<string> ScenePaths => scenePaths;
        public int SectionCount => scenePaths != null && scenePaths.Length > 0
            ? scenePaths.Length : sections?.Length ?? 0;
        public IReadOnlyList<string> UnlockStage => unlockStage;
    }
}

