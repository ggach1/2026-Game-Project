using CIW.Code.System.Interface;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CIW.Code.Player
{
    /// <summary>레벨 재시작 시스템이 연결되기 전 사용하는 임시 사망/리스폰 드라이버.</summary>
    [RequireComponent(typeof(Player))]
    public class PlayerRespawnTest : MonoBehaviour
    {
        [SerializeField] Key deathKey = Key.F8;
        [SerializeField, Min(0.05f)] float respawnDelay = 0.65f;
        [SerializeField] Transform spawnPoint;

        Player _player;
        PlayerSpawnData _spawn;
        Coroutine _pendingRespawn;

        private void Start()
        {
            // 모든 모듈의 Awake 초기화가 끝난 뒤 시작 위치와 방향을 보관합니다.
            _player = GetComponent<Player>();
            var renderer = _player.GetModule<CIW.Code.System.EntityAnimator>().Renderer;
            _spawn = new PlayerSpawnData(transform.position, renderer == null || !renderer.flipX);
            Subscribe();
        }

        private void OnEnable()
        {
            if (_player != null) Subscribe();
        }

        private void Subscribe()
        {
            _player.Life.Died += HandleDeath;
            _player.Life.Respawned += CancelPendingRespawn;
            if (!_player.IsAlive) HandleDeath(default);
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // 공유 Controls 에셋을 변경하지 않는 개발용 키입니다. 일반 릴리스 빌드에서는 제외합니다.
            if (_player != null && _player.IsAlive && Time.timeScale > 0f &&
                Keyboard.current != null && deathKey != Key.None && Keyboard.current[deathKey].wasPressedThisFrame)
                _player.Kill(new DeathContext(transform.position, Vector2.zero, DeathCause.Unknown));
#endif
        }

        private void HandleDeath(DeathContext context)
        {
            if (_pendingRespawn == null && !_player.IsAlive)
                _pendingRespawn = StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(Mathf.Max(0.05f, respawnDelay));
            _pendingRespawn = null;
            if (!_player.IsAlive)
                _player.Respawn(new PlayerSpawnData(
                    spawnPoint != null ? (Vector2)spawnPoint.position : _spawn.Position, _spawn.FaceRight));
        }

        private void CancelPendingRespawn()
        {
            // 외부에서 먼저 리스폰하면 예약된 리스폰이 나중에 다시 순간이동시키지 않도록 취소합니다.
            if (_pendingRespawn != null) StopCoroutine(_pendingRespawn);
            _pendingRespawn = null;
        }

        private void OnDisable()
        {
            CancelPendingRespawn();
            if (_player == null) return;
            _player.Life.Died -= HandleDeath;
            _player.Life.Respawned -= CancelPendingRespawn;
        }
    }
}
