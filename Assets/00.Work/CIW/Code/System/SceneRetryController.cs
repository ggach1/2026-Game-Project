using CIW.Code.System.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CIW.Code.System
{
    /// <summary>단독 맵의 공통 재시작 진입점. 별도 GameObject나 Inspector 연결 없이 사용합니다.</summary>
    public static class SceneRetryController
    {
        public static bool IsReloading { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => IsReloading = false;

        public static bool TryRestart(Scene scene)
        {
            if (IsReloading) return false;
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
            {
                Debug.LogError("씬 재시작에는 저장된 로드 상태의 씬이 필요합니다. 씬을 저장한 뒤 다시 Play하세요.");
                return false;
            }
#if !UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(scene.path))
            {
                Debug.LogError($"Build Settings에 재시작할 씬을 등록하세요: {scene.path}");
                return false;
            }
#endif
            // 정리 콜백에서 다시 요청하더라도 로딩이 중복되지 않도록 먼저 잠급니다.
            IsReloading = true;
            try
            {
                CleanupBeforeRetry();
                Time.timeScale = 1f;
#if UNITY_EDITOR
                var operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
                var operation = SceneManager.LoadSceneAsync(scene.path, LoadSceneMode.Single);
#endif
                if (operation == null)
                {
                    IsReloading = false;
                    Debug.LogError($"씬 재시작 요청에 실패했습니다: {scene.path}");
                    return false;
                }
                operation.completed += _ => IsReloading = false;
                return true;
            }
            catch (global::System.Exception exception)
            {
                IsReloading = false;
                Debug.LogException(exception);
                return false;
            }
        }

        public static void CleanupBeforeRetry()
        {
            // Single 모드 전체 씬 교체용입니다. DontDestroyOnLoad와 비활성 대여 객체도 포함합니다.
            // 에셋은 검색하지 않으며, 인터페이스를 구현한 런타임 객체만 자신의 상태를 정리합니다.
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null || behaviour is not ISceneRetryCleanup cleanup) continue;
                try
                {
                    cleanup.CleanupBeforeSceneRetry();
                }
                catch (global::System.Exception exception)
                {
                    // 한 기믹의 정리 오류가 다른 기믹의 반환과 씬 재시작까지 막지 않게 합니다.
                    Debug.LogException(exception, behaviour);
                }
            }
        }
    }
}
