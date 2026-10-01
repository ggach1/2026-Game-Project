using UnityEngine;

namespace CIW.Code.System.Stage
{
    [CreateAssetMenu(fileName = "SectionDef", menuName = "SO/Section Definition")]
    public class SectionDefinition : ScriptableObject
    {
        [SerializeField] string sectionId;
        [SerializeField] SectionContext prefab;

        public string SectionId => sectionId;
        public SectionContext Prefab => prefab;
    }
}

