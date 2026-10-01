using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIW.Code.System.Stage
{
    [Serializable]
    public class StageMapEntry
    {
        [SerializeField] StageDefinition stage;
        [SerializeField] Vector2 position;

        public StageDefinition Stage => stage;
        public Vector2 Position => position;
    }

    [CreateAssetMenu(fileName = "WorldDef", menuName = "SO/World Definition")]
    public class WorldDefinition : ScriptableObject
    {
        [SerializeField] StageMapEntry[] entries;

        public IReadOnlyList<StageMapEntry> Entries => entries;

        public StageDefinition FindStage(string id)
        {
            foreach (var en in entries)
            {
                if (en.Stage != null && en.Stage.StageId == id)
                    return en.Stage;
            }

            return null;
        }
    }
}

