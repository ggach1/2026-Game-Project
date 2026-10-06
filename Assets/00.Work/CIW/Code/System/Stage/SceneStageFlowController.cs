using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIW.Code.System.Stage
{
    /// <summary>메뉴에서 시작한 씬 기반 스테이지의 구간 번호만 씬 전환 사이에 유지합니다.</summary>
    public sealed class SceneStageFlowController : MonoBehaviour
    {
        static SceneStageFlowController _instance;
        readonly List<ExitDoor> _doors = new();
        StageDefinition _stage;
        StageProgressService _progress;
        string _menuPath;
        int _sectionIndex;
        bool _transitioning;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic() => _instance = null;

        public static bool TryStart(StageDefinition stage, string menuPath)
        {
            if (_instance != null || SceneRetryController.IsReloading || stage == null ||
                stage.ScenePaths == null || stage.ScenePaths.Count == 0) return false;
            // 중간 맵이 누락된 채 시작해 출구에서 막히는 일을 방지합니다.
            if (!SceneRetryController.CanLoadScene(menuPath)) return false;
            foreach (string path in stage.ScenePaths)
            {
                if (SceneRetryController.CanLoadScene(path)) continue;
                Debug.LogError($"스테이지 구간 씬을 불러올 수 없습니다: {path}");
                return false;
            }

            var root = new GameObject("Scene Stage Flow");
            var flow = root.AddComponent<SceneStageFlowController>();
            _instance = flow;
            flow._stage = stage;
            flow._menuPath = menuPath;
            flow._progress = root.AddComponent<StageProgressService>();
            DontDestroyOnLoad(root);
            SceneManager.sceneLoaded += flow.HandleSceneLoaded;
            flow._transitioning = true;
            if (SceneRetryController.TryLoadScene(stage.ScenePaths[0])) return true;
            Destroy(root);
            return false;
        }

        void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DetachDoors();
            if (scene.path != _stage.ScenePaths[_sectionIndex])
            {
                // 메뉴 복귀 또는 외부 씬 이동 시 세션을 종료합니다. 재도전은 항상 Map 1부터입니다.
                Destroy(gameObject);
                return;
            }
            _transitioning = false;
            // 사망 재로드도 여기로 들어오지만 인덱스는 바꾸지 않고 새 출구에만 다시 연결합니다.
            foreach (var root in scene.GetRootGameObjects())
            foreach (var door in root.GetComponentsInChildren<ExitDoor>(true))
            {
                _doors.Add(door);
                door.OnEscaped.AddListener(HandleEscaped);
            }
            if (_doors.Count == 0) Debug.LogError($"구간에 ExitDoor가 없습니다: {scene.path}", this);
        }

        void HandleEscaped(Player.Player player)
        {
            if (_transitioning || SceneRetryController.IsReloading || player == null ||
                player.gameObject.scene.path != _stage.ScenePaths[_sectionIndex]) return;
            _transitioning = true;
            int previousIndex = _sectionIndex;
            bool finalSection = _sectionIndex + 1 == _stage.ScenePaths.Count;
            if (!finalSection) _sectionIndex++;
            string destination = finalSection ? _menuPath : _stage.ScenePaths[_sectionIndex];
            if (SceneRetryController.TryLoadScene(destination))
            {
                // 중간 출구는 저장하지 않습니다. 마지막 구간 완료만 스테이지 완료로 기록합니다.
                if (finalSection) _progress.Complete(_stage);
                return;
            }
            _sectionIndex = previousIndex;
            _transitioning = false;
            // 로딩 요청 실패 시 사라진 플레이어 상태로 고립되지 않도록 현재 맵에서 재도전합니다.
            SceneRetryController.TryRestart(player.gameObject.scene);
        }

        void DetachDoors()
        {
            foreach (var door in _doors)
                if (door != null) door.OnEscaped.RemoveListener(HandleEscaped);
            _doors.Clear();
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            DetachDoors();
            if (_instance == this) _instance = null;
        }
    }
}
