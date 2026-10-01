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

        [Tooltip("이 스테이지를 완료했을 때 열릴 스테이지의 ID")]
        [SerializeField] string[] unlockStage;

        public string StageId => stageId;
        public string DisplayName => displayName;
        public bool InitiallyUnlocked => initiallyUnlocked;

        public IReadOnlyList<SectionDefinition> Sections => sections;
        public IReadOnlyList<string> UnlockStage => unlockStage;
    }
}

