using System;
using System.IO;
using CIW.Code.System.Stage;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CIW.Code.System.Stage.Editor
{
    public static class StageFlowTestSetup
    {
        const string Root = "Assets/00.Work/CIW/StageFlowTest";
        public const string ScenePath = Root + "/StageFlowTest.unity";

        // 기존 씬을 교체하거나 저장하지 않고, 최초 한 번 새 에셋만 생성합니다.
        [InitializeOnLoadMethod]
        static void Schedule()
        {
            EditorApplication.delayCall += AutoCreate;
        }

        static void AutoCreate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;
            if (!File.Exists(ScenePath)) Create();
        }

        [MenuItem("CIW/Stage Flow/Create Test Assets (Only If Missing)")]
        public static void Create()
        {
            if (File.Exists(ScenePath))
            {
                Debug.Log("StageFlowTest already exists; existing assets were not overwritten.");
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                Directory.CreateDirectory(Root);
                AssetDatabase.Refresh();
                var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/00.Work/CIW/02.Prefabs/Player.prefab");
                var doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/00.Work/CIW/02.Prefabs/ExitDoor.prefab");
                if (playerPrefab == null || doorPrefab == null || Resources.Load<TMP_Settings>("TMP Settings") == null)
                    throw new InvalidOperationException("Player, ExitDoor or TMP default font is missing.");

                var sprite = MakeSprite();
                var section1 = MakeSection("A1", false, new Color(0.35f, 0.75f, 0.8f), sprite, doorPrefab);
                var section2 = MakeSection("A2", true, new Color(0.65f, 0.55f, 0.85f), sprite, doorPrefab);
                var sectionB = MakeSection("B1", false, new Color(0.85f, 0.65f, 0.3f), sprite, doorPrefab);
                var a = MakeStage("A", true, new[] { section1, section2 }, new[] { "ciw_test_b" });
                var b = MakeStage("B", false, new[] { sectionB }, Array.Empty<string>());
                var world = Asset<WorldDefinition>("World");
                var ws = new SerializedObject(world);
                var entries = ws.FindProperty("entries"); entries.arraySize = 2;
                entries.GetArrayElementAtIndex(0).FindPropertyRelative("stage").objectReferenceValue = a;
                entries.GetArrayElementAtIndex(0).FindPropertyRelative("position").vector2Value = new Vector2(-200, 0);
                entries.GetArrayElementAtIndex(1).FindPropertyRelative("stage").objectReferenceValue = b;
                entries.GetArrayElementAtIndex(1).FindPropertyRelative("position").vector2Value = new Vector2(200, 0);
                ws.ApplyModifiedPropertiesWithoutUndo();

                var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
                camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10);
                camera.orthographic = true; camera.orthographicSize = 5.5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.08f, 0.12f, 0.2f);
                var player = ((GameObject)PrefabUtility.InstantiatePrefab(playerPrefab)).GetComponent<Player.Player>();
                player.transform.position = new Vector3(-6, -1.4f, 0);
                var systems = new GameObject("Stage Systems (Always Active)");
                var loader = systems.AddComponent<SectionLoader>();
                var progress = systems.AddComponent<StageProgressService>();
                var flow = systems.AddComponent<StageFlowController>();
                var ui = systems.AddComponent<StageSelectUI>();
                var sectionRoot = new GameObject("Section Root").transform;
                Set(loader, "sectionRoot", sectionRoot);
                SetString(progress, "saveKey", "CIW.StageFlowTest.v1");

                var canvas = new GameObject("Stage Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.matchWidthOrHeight = 0.5f;
                var panel = Rect("Stage Select Panel", canvas.transform, Vector2.zero, new Vector2(1280, 720));
                panel.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.09f, 0.15f, 0.98f);
                Text("Title", panel, "STAGE FLOW TEST", new Vector2(0, 230), new Vector2(800, 65), 42);
                Text("Subtitle", panel, "Clear A's two sections to unlock B", new Vector2(0, 170), new Vector2(800, 50), 24);
                var nodes = Rect("Stage Nodes", panel, Vector2.zero, new Vector2(1000, 300));
                var nodePrefab = MakeNode();
                Set(ui, "panel", panel.gameObject); Set(ui, "nodeRoot", nodes); Set(ui, "nodePrefab", nodePrefab);

                var status = Text("Status", canvas.transform, "SELECT A STAGE", new Vector2(0, 320), new Vector2(1000, 40), 22);
                Text("Controls", canvas.transform, "A / D: Move    Space: Jump    F8: Death test", new Vector2(0, -320), new Vector2(1000, 40), 22);
                var back = Rect("Back to Select", canvas.transform, new Vector2(490, 260), new Vector2(200, 48));
                back.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.35f, 0.5f);
                var backButton = back.gameObject.AddComponent<Button>();
                Text("Label", back, "STAGE SELECT", Vector2.zero, new Vector2(190, 45), 19);
                UnityEventTools.AddPersistentListener(backButton.onClick, flow.ReturnToStageSelect);
                var hud = systems.AddComponent<StageFlowTestHUD>();
                Set(hud, "flow", flow); Set(hud, "player", player); Set(hud, "status", status); Set(hud, "backButton", back.gameObject);
                Set(flow, "world", world); Set(flow, "loader", loader); Set(flow, "progress", progress); Set(flow, "selectUI", ui); Set(flow, "player", player);
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log("CIW_STAGE_SETUP_OK: " + ScenePath);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        static Sprite MakeSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/GroundSprite.asset");
            if (existing != null) return existing;
            var texture = new Texture2D(4, 4); texture.name = "GroundTexture";
            var pixels = new Color[16]; for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels); texture.Apply();
            // 텍스처도 서브 에셋으로 저장해 에디터 재시작 후 참조를 유지합니다.
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4);
            sprite.name = "GroundSprite";
            AssetDatabase.CreateAsset(sprite, Root + "/GroundSprite.asset");
            AssetDatabase.AddObjectToAsset(texture, sprite);
            return sprite;
        }

        static SectionDefinition MakeSection(string id, bool gap, Color color, Sprite sprite, GameObject doorPrefab)
        {
            string path = Root + "/Section_" + id + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<SectionContext>(path);
            if (prefab == null)
            {
                var root = new GameObject("Section_" + id);
                var context = root.AddComponent<SectionContext>();
                var spawn = new GameObject("Spawn Point").transform; spawn.SetParent(root.transform);
                spawn.position = new Vector3(-6, -1.4f, 0);
                void Ground(string name, float x, float width)
                {
                    var ground = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
                    ground.transform.SetParent(root.transform); ground.layer = LayerMask.NameToLayer("Ground");
                    ground.transform.position = new Vector3(x, -2.5f, 0); ground.transform.localScale = new Vector3(width, 1, 1);
                    var renderer = ground.GetComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color;
                    // 빈 SpriteRenderer에 Collider를 먼저 붙이면 자동 크기가 거의 0이 되므로 명시합니다.
                    ground.GetComponent<BoxCollider2D>().size = Vector2.one;
                    renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("a97c105638bdf8b4a8650670310a4cd3"));
                }
                if (gap) { Ground("Left Ground", -4.5f, 7); Ground("Right Ground", 4.5f, 7); }
                else Ground("Ground", 0, 16);
                var door = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
                door.transform.SetParent(root.transform); door.transform.position = new Vector3(6, -1, 0);
                Set(context, "spawnPoint", spawn);
                var so = new SerializedObject(context); var exits = so.FindProperty("exitDoors"); exits.arraySize = 1;
                exits.GetArrayElementAtIndex(0).objectReferenceValue = door.GetComponent<ExitDoor>(); so.ApplyModifiedPropertiesWithoutUndo();
                prefab = PrefabUtility.SaveAsPrefabAsset(root, path).GetComponent<SectionContext>();
                Object.DestroyImmediate(root);
            }
            var definition = Asset<SectionDefinition>("Section_" + id);
            SetString(definition, "sectionId", "ciw_test_" + id.ToLowerInvariant()); Set(definition, "prefab", prefab);
            return definition;
        }

        static StageDefinition MakeStage(string id, bool unlocked, SectionDefinition[] sections, string[] unlocks)
        {
            var stage = Asset<StageDefinition>("Stage_" + id); var so = new SerializedObject(stage);
            so.FindProperty("stageId").stringValue = "ciw_test_" + id.ToLowerInvariant();
            so.FindProperty("displayName").stringValue = "STAGE " + id;
            so.FindProperty("initiallyUnlocked").boolValue = unlocked;
            var list = so.FindProperty("sections"); list.arraySize = sections.Length;
            for (int i = 0; i < sections.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sections[i];
            list = so.FindProperty("unlockStage"); list.arraySize = unlocks.Length;
            for (int i = 0; i < unlocks.Length; i++) list.GetArrayElementAtIndex(i).stringValue = unlocks[i];
            so.ApplyModifiedPropertiesWithoutUndo(); return stage;
        }

        static StageNodeView MakeNode()
        {
            string path = Root + "/StageNode.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<StageNodeView>(path); if (existing != null) return existing;
            var rect = Rect("Stage Door", null, Vector2.zero, new Vector2(240, 260));
            rect.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.3f, 0.4f);
            var button = rect.gameObject.AddComponent<Button>();
            var title = Text("Title", rect, "STAGE", new Vector2(0, 70), new Vector2(220, 55), 30);
            var locked = Text("Locked", rect, "LOCKED", Vector2.zero, new Vector2(220, 45), 24);
            var completed = Text("Completed", rect, "CLEARED", new Vector2(0, -65), new Vector2(220, 45), 24);
            completed.color = Color.green;
            var view = rect.gameObject.AddComponent<StageNodeView>();
            Set(view, "button", button); Set(view, "title", title); Set(view, "lockMark", locked.gameObject); Set(view, "completedMark", completed.gameObject);
            var prefab = PrefabUtility.SaveAsPrefabAsset(rect.gameObject, path).GetComponent<StageNodeView>();
            Object.DestroyImmediate(rect.gameObject); return prefab;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }

        static TextMeshProUGUI Text(string name, Transform parent, string value, Vector2 position, Vector2 size, float fontSize)
        {
            var text = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset; text.text = value; text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; return text;
        }

        static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = Root + "/" + name + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }

        static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target); so.FindProperty(field).stringValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
