using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CIW.Code.System.Stage.Editor
{
    public sealed class SceneStageFlowTests
    {
        [TestCase(0, "cuh_stage_01", "Map ", 4)]
        [TestCase(1, "cuh_stage_02", "Map_2 ", 4)]
        [TestCase(2, "cuh_stage_03", "Map_3 ", 2)]
        public void MenuHasOrderedLoadableScenes(int index, string id, string prefix, int count)
        {
            var world = AssetDatabase.LoadAssetAtPath<WorldDefinition>(
                "Assets/00.Work/CIW/08.SO/Menu/MenuWorld.asset");
            Assert.That(world, Is.Not.Null);
            Assert.That(world.Entries.Count, Is.EqualTo(3));
            var stage = world.Entries[index].Stage;
            Assert.That(stage.StageId, Is.EqualTo(id));
            Assert.That(stage.InitiallyUnlocked, Is.EqualTo(index == 0));
            Assert.That(stage.SectionCount, Is.EqualTo(count));
            if (index < 2)
                CollectionAssert.AreEqual(new[] { world.Entries[index + 1].Stage.StageId }, stage.UnlockStage);
            else Assert.That(stage.UnlockStage, Is.Empty);
            for (int i = 0; i < count; i++)
            {
                string path = $"Assets/00.Work/CUH/00.Scene/{prefix}{i + 1}.unity";
                Assert.That(stage.ScenePaths[i], Is.EqualTo(path));
                Assert.That(SceneRetryController.CanLoadScene(path), Is.True);
                Assert.That(global::System.Array.Exists(EditorBuildSettings.scenes,
                    scene => scene.enabled && scene.path == path), Is.True);
            }
        }

        [Test]
        public void PrefabStagesRetainTheirSectionCount()
        {
            var stage = ScriptableObject.CreateInstance<StageDefinition>();
            try
            {
                var serialized = new SerializedObject(stage);
                serialized.FindProperty("sections").arraySize = 2;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(stage.SectionCount, Is.EqualTo(2));
            }
            finally { Object.DestroyImmediate(stage); }
        }
    }
}
