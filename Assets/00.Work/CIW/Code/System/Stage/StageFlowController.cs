using CIW.Code.System.Interface;
using System.Collections;
using UnityEngine;

namespace CIW.Code.System.Stage
{
    public enum StageFlowState
    {
        StageSelect,
        LoadingSection,
        Playing,
        Respawning,
        Transitioning,
        CompletingStage
    }

    public class StageFlowController : MonoBehaviour
    {
        [SerializeField] WorldDefinition world;
        [SerializeField] SectionLoader loader;
        [SerializeField] StageProgressService progress;
        [SerializeField] StageSelectUI selectUI;
        [SerializeField] Player.Player player;

        [SerializeField, Min(0f)] float deathDelay = 0.65f;

        public StageFlowState State { get; private set; }
        public StageDefinition CurrentStage { get; private set; }
        public int CurrentSectionIndex { get; private set; }

        SectionContext _curSection;
        Coroutine _transition; // 이거는 DoTween으로 구현하기
        bool _subscribed;
        Player.PlayerRespawnTest _respawnTest;
        bool _previousAutoRespawn;

        private void Start()
        {
            // 같은 플레이어의 임시 자동 리스폰만 끕니다. 단독 테스트 씬의 기본 동작은 유지합니다.
            _respawnTest = player.GetComponent<Player.PlayerRespawnTest>();
            if (_respawnTest != null)
            {
                _previousAutoRespawn = _respawnTest.AutoRespawn;
                _respawnTest.SetAutoRespawn(false);
            }

            selectUI.StageSelected += StartStage;
            player.Life.Died += HandlePlayerDied;
            _subscribed = true;

            ShowStageSelect();
        }

        private void OnDestroy()
        {
            DetachSection();

            if (_respawnTest != null)
                _respawnTest.SetAutoRespawn(_previousAutoRespawn);

            if (!_subscribed)
                return;

            if (selectUI != null)
                selectUI.StageSelected -= StartStage;

            if (player != null && player.Life != null)
                player.Life.Died -= HandlePlayerDied;
        }

        public void StartStage(StageDefinition stage)
        {
            if (State != StageFlowState.StageSelect)
                return;

            // UI 외부에서 호출되더라도 해금 조건을 다시 검사합니다.
            if (stage == null || world.FindStage(stage.StageId) != stage || !progress.IsUnlocked(stage, world))
                return;

            if (!ValidateStage(stage))
                return;

            CurrentStage = stage;
            CurrentSectionIndex = 0;

            selectUI.Hide();

            State = StageFlowState.LoadingSection;
            _transition = StartCoroutine(LoadCurrentSection(0f));
        }

        private bool ValidateStage(StageDefinition stage)
        {
            if (stage.Sections == null || stage.Sections.Count == 0)
            {
                Debug.LogError("스테이지에 구간이 없습니다.");
                return false;
            }

            foreach (var section in stage.Sections)
            {
                if (section == null ||
                    section.Prefab == null ||
                    section.Prefab.SpawnPoint == null)
                {
                    Debug.LogError(
                        $"구간 데이터 또는 시작 위치 누락: {stage.name}");

                    return false;
                }
            }

            return true;
        }

        private void HandleSectionEscaped(SectionContext source, Player.Player escapedPlayer)
        {
            // 이전 맵의 늦은 이벤트와 중복 완료를 차단합니다.
            if (State != StageFlowState.Playing || source != _curSection || escapedPlayer != player)
                return;

            bool hasNext = CurrentSectionIndex + 1 < CurrentStage.Sections.Count;

            if (hasNext)
            {
                // 코루틴 실행 전에 상태부터 잠급니다.
                State = StageFlowState.Transitioning;
                CurrentSectionIndex++;

                _transition = StartCoroutine(LoadCurrentSection(0f));
            }
            else
            {
                State = StageFlowState.CompletingStage;

                progress.Complete(CurrentStage);
                ShowStageSelect();
            }
        }

        private void HandlePlayerDied(DeathContext context)
        {
            if (State != StageFlowState.Playing)
                return;

            State = StageFlowState.Respawning;

            // 인덱스를 바꾸지 않으므로 현재 구간에서 다시 시작합니다.
            _transition = StartCoroutine(LoadCurrentSection(deathDelay));
        }

        private IEnumerator LoadCurrentSection(float delay)
        {
            // 사망 파편을 보여준 다음 맵을 초기화합니다.
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            player.gameObject.SetActive(false);

            DetachSection();

            _curSection = loader.Load(CurrentStage.Sections[CurrentSectionIndex]);

            if (_curSection == null)
            {
                _transition = null;
                ShowStageSelect();
                yield break;
            }

            _curSection.Escaped += HandleSectionEscaped;

            // 새 맵의 Start 초기화가 실행될 시간을 줍니다.
            // 비동기 준비가 필요한 맵이라면 명시적인 Ready 신호로 대체합니다.
            yield return null;

            Transform spawn = _curSection.SpawnPoint;

            // 활성화 전에 위치를 옮겨 이전 구간 위치가 잠깐 보이지 않게 합니다.
            player.transform.position = spawn.position;
            player.gameObject.SetActive(true);

            player.Respawn(new PlayerSpawnData(spawn.position, _curSection.FaceRight));

            State = StageFlowState.Playing;
            _transition = null;
        }

        public bool TryResetProgressFromStageSelect()
        {
            // 플레이/출구 연출/리스폰 도중에는 초기화하지 않아 늦은 클리어 이벤트의 재저장을 막습니다.
            if (State != StageFlowState.StageSelect)
                return false;

            progress.ResetProgress();
            selectUI.Show(world, progress);
            return true;
        }

        public void ReturnToStageSelect()
        {
            // 메뉴로 나가면 진행 중인 로딩/리스폰 예약도 취소합니다.
            if (_transition != null)
            {
                StopCoroutine(_transition);
                _transition = null;
            }

            ShowStageSelect();
        }

        private void ShowStageSelect()
        {
            State = StageFlowState.StageSelect;

            player.gameObject.SetActive(false);

            DetachSection();
            loader.Unload();

            CurrentStage = null;
            CurrentSectionIndex = 0;

            selectUI.Show(world, progress);
        }

        private void DetachSection()
        {
            if (_curSection != null)
                _curSection.Escaped -= HandleSectionEscaped;

            _curSection = null;
        }
    }
}

