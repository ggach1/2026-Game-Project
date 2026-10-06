using System;
using UnityEngine;

namespace CIW.Code.System.Stage
{
    public class SectionLoader : MonoBehaviour
    {
        [SerializeField] Transform sectionRoot;

        public SectionContext Current { get; private set; }

        public SectionContext Load(SectionDefinition def)
        {
            Unload();

            if (def == null || def.Prefab == null)
            {
                Debug.LogError("구간 프리팹이 지정되지 않았어요");
                return null;
            }

            Current = Instantiate(def.Prefab, sectionRoot);
            return Current;
        }

        public void Unload()
        {
            if (Current == null)
                return;

            Current.gameObject.SetActive(false);
            Destroy(Current.gameObject);
            Current = null;
        }
    }
}

