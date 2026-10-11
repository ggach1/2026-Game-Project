namespace CIW.Code.System.Interface
{
    /// <summary>씬 재로드만으로 제거되지 않는 실행 상태를 정리합니다. DevLib의 풀 계약과는 독립적입니다.</summary>
    public interface ISceneRetryCleanup
    {
        // 여러 번 호출되어도 안전해야 하며, 씬 로딩이나 플레이어 리스폰은 수행하지 않습니다.
        void CleanupBeforeSceneRetry();
    }
}
