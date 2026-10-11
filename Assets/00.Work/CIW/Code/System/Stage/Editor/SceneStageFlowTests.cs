using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CIW.Code.System.Stage.Editor
{
    public sealed class SceneStageFlowTests
    {
        [Test]
        public void MenuHasOneStageWithFourOrderedLoadableScenes()
        {
            var world = AssetDatabase.LoadAssetAtPath<WorldDefinition>(
                "Assets/00.Work/CIW/08.SO/Menu/MenuWorld.asset");
            Assert.That(world, Is.Not.Null);
            Assert.That(world.Entries.Count, Is.EqualTo(1));
            var stage = world.Entries[0].Stage;
            Assert.That(stage.StageId, Is.EqualTo("cuh_stage_01"));
            Assert.That(stage.InitiallyUnlocked, Is.True);
            Assert.That(stage.SectionCount, Is.EqualTo(4));
            for (int i = 0; i < 4; i++)
            {
                string path = $"Assets/00.Work/CUH/00.Scene/Map {i + 1}.unity";
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
