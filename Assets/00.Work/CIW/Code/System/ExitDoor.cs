using CIW.Code.Player;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace CIW.Code.System
{
    /// <summary>접촉형 출구. 다음 맵 로드는 레벨 담당자가 onEscaped에 연결합니다.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ExitDoor : MonoBehaviour
    {
        [SerializeField] bool unlocked = true;
        [SerializeField] Material doorMaterial;
        [SerializeField, Min(0.01f)] float entryDuration = 0.25f;
        [SerializeField, Min(0f)] float centerTolerance = 0.2f;
        [Tooltip("문틀을 제외한 실제 입구의 로컬 너비/높이. 접근 감지 BoxCollider 크기와 별개입니다.")]
        [SerializeField] Vector2 openingSize = new Vector2(0.94f, 1.82f);
        [SerializeField] UnityEvent<CIW.Code.Player.Player> onEscaped = new();
        public bool IsCompleted { get; private set; }
        public bool IsUnlocked => unlocked;
        public UnityEvent<Player.Player> OnEscaped => onEscaped;

        Sprite _square;
        SpriteRenderer _frame;
        SpriteRenderer _opening;
        SpriteRenderer _handle;
        Vector2 _displayedOpeningSize;
        static readonly Vector2 OpeningOffset = new Vector2(0f, -0.05f);
        Player.Player _enteringPlayer;
        Coroutine _entryRoutine;
        public bool IsEntering => _enteringPlayer != null;

        private void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            // 임시 문 그래픽은 사각형으로 구성하며 외부 이미지 자산을 요구하지 않습니다.
            _square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
            _frame = CreatePart("Frame", Vector2.zero, new Vector2(1.2f, 2f), Color.white, -2);
            _opening = CreatePart("Opening", OpeningOffset, openingSize, new Color(0.06f, 0.07f, 0.1f), -1);
            _handle = CreatePart("Handle", new Vector2(0.3f, 0), Vector2.one * 0.1f, new Color(1f, 0.83f, 0.3f), 0);
            RefreshOpeningVisuals();
            RefreshColor();
        }

        public void SetOpeningSize(Vector2 size)
        {
            openingSize = new Vector2(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y));
            RefreshOpeningVisuals();
        }

        private void LateUpdate()
        {
            // Play 중 Inspector/애니메이션으로 크기를 바꿔도 표시와 판정을 일치시킵니다.
            if (_displayedOpeningSize != openingSize) RefreshOpeningVisuals();
        }

        private void OnValidate()
        {
            openingSize = new Vector2(Mathf.Max(0.01f, openingSize.x), Mathf.Max(0.01f, openingSize.y));
        }

        private void RefreshOpeningVisuals()
        {
            if (_opening == null) return;
            _displayedOpeningSize = openingSize;
            _opening.transform.localScale = new Vector3(openingSize.x, openingSize.y, 1f);
            _frame.transform.localScale = new Vector3(openingSize.x + 0.26f, openingSize.y + 0.18f, 1f);
            _handle.transform.localPosition = new Vector3(openingSize.x * 0.32f, 0f, 0f);
        }

        /// <summary>위치/접지와 별개인 크기 검사. 입장 연출 시작 전에 호출합니다.</summary>
        public bool CanFit(CIW.Code.Player.Player player)
        {
            Collider2D body = player != null ? player.BodyCollider : null;
            if (body == null || !body.isActiveAndEnabled || body.isTrigger) return false;
            Vector3 scale = transform.lossyScale;
            if (Mathf.Abs(scale.x) < 0.0001f || Mathf.Abs(scale.y) < 0.0001f) return false;

            // 월드 물리 Bounds를 문 좌표로 투영하여 부모의 확대/축소 및 음수 Scale도 반영합니다.
            // 회전한 Collider는 보수적인 외접 사각형으로 검사합니다(일반적인 직립 2D 기준).
            Vector3 size = body.bounds.size;
            Vector3 x = transform.InverseTransformVector(new Vector3(size.x, 0f, 0f));
            Vector3 y = transform.InverseTransformVector(new Vector3(0f, size.y, 0f));
            float width = Mathf.Abs(x.x) + Mathf.Abs(y.x);
            float height = Mathf.Abs(x.y) + Mathf.Abs(y.y);
            // 같은 크기를 허용하기 위한 미세 오차만 둡니다. 연출용 시각 축소는 검사에 쓰지 않습니다.
            const float epsilon = 0.0001f;
            return width <= openingSize.x + epsilon && height <= openingSize.y + epsilon;
        }

        private SpriteRenderer CreatePart(string label, Vector2 position, Vector2 size, Color color, int order)
        {
            var part = new GameObject(label);
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = _square;
            if (doorMaterial != null) renderer.sharedMaterial = doorMaterial;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void OnTriggerEnter2D(Collider2D other) => TryEnter(other);
        private void OnTriggerStay2D(Collider2D other) => TryEnter(other);

        private void TryEnter(Collider2D other)
        {
            if (!isActiveAndEnabled || !unlocked || IsCompleted || IsEntering) return;
            var player = other.GetComponentInParent<CIW.Code.Player.Player>();
            if (player == null || player.Life == null || player.Life.State != PlayerLifeState.Alive) return;
            // 들어갈 수 없으면 상태/입력/물리를 바꾸지 않습니다. Stay에서 매번 재검사합니다.
            if (!CanFit(player)) return;
            // 가장자리 스침이나 공중 통과는 제외하고 문 중심에 서 있을 때 입장합니다.
            Vector3 local = transform.InverseTransformPoint(player.transform.position);
            if (Mathf.Abs(local.x) > centerTolerance) return;
            if (!player.Life.TryBeginExit(transform.position)) return;
            _enteringPlayer = player;
            _entryRoutine = StartCoroutine(Enter(player));
        }

        private IEnumerator Enter(CIW.Code.Player.Player player)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, entryDuration);
            while (elapsed < duration)
            {
                yield return null;
                if (player == null || !player.isActiveAndEnabled || player.Life.State != PlayerLifeState.EnteringExit)
                {
                    _entryRoutine = null;
                    CancelEntry();
                    yield break;
                }
                elapsed += Time.deltaTime;
                player.View.SetExitProgress(elapsed / duration);
            }
            _entryRoutine = null;
            _enteringPlayer = null;
            IsCompleted = true;
            // 두 완료 이벤트 모두 연출 종료 이후에만 보냅니다.
            if (!player.TryEscape()) { IsCompleted = false; yield break; }
            RefreshColor();
            Debug.Log("탈출 완료: " + player.name, this);
            onEscaped.Invoke(player);
        }

        public void SetUnlocked(bool value)
        {
            unlocked = value;
            RefreshColor();
        }

        public void ResetExit()
        {
            CancelEntry();
            // 같은 씬을 재시작할 때 플레이어 Respawn과 함께 레벨 시스템에서 호출합니다.
            IsCompleted = false;
            RefreshColor();
        }

        private void CancelEntry()
        {
            if (_entryRoutine != null) StopCoroutine(_entryRoutine);
            _entryRoutine = null;
            if (_enteringPlayer != null) _enteringPlayer.Life.CancelExit();
            _enteringPlayer = null;
        }

        private void OnDisable() => CancelEntry();

        private void RefreshColor()
        {
            if (_frame != null)
                _frame.color = IsCompleted ? Color.green : unlocked ? new Color(0.35f, 0.9f, 1f) : Color.gray;
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = unlocked ? Color.cyan : Color.gray;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(openingSize.x + 0.26f, openingSize.y + 0.18f, 0.05f));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(OpeningOffset, new Vector3(openingSize.x, openingSize.y, 0.05f));
        }

        private void OnDestroy()
        {
            if (_square != null) Destroy(_square);
        }
    }
}
