using UnityEngine;

namespace CIW.Code.Player
{
    /// <summary>충돌체 없는 사각 파편을 재사용하는 임시 픽셀 폭발 효과.</summary>
    public class PlayerDeathBurst : MonoBehaviour
    {
        [SerializeField, Range(4, 64)] int fragmentCount = 24;
        [SerializeField, Min(0.05f)] float lifetime = 0.45f;
        [SerializeField, Min(0.01f)] float fragmentSize = 0.09f;
        [SerializeField, Min(0f)] float burstSpeed = 2.8f;
        [SerializeField, Min(0f)] float gravity = 8f;
        [SerializeField, Min(0f)] float spawnRadius = 0.25f;
        [SerializeField] Color fragmentColor = new Color(0.15f, 0.12f, 0.2f, 1f);

        GameObject _root;
        Sprite _sprite;
        SpriteRenderer[] _fragments;
        Vector2[] _velocities;
        float[] _spins;
        Vector2 _gravityDirection;
        float _elapsed;
        bool _playing;

        private void EnsureFragments(SpriteRenderer source)
        {
            if (_root != null) return;
            // 월드 공간에 두어 플레이어가 리스폰 위치로 옮겨져도 파편이 따라가지 않습니다.
            _root = new GameObject("Player death fragments");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_root, gameObject.scene);
            _sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
            int count = Mathf.Clamp(fragmentCount, 4, 64);
            _fragments = new SpriteRenderer[count];
            _velocities = new Vector2[count];
            _spins = new float[count];
            for (int i = 0; i < count; i++)
            {
                var piece = new GameObject("Fragment");
                piece.transform.SetParent(_root.transform, false);
                var renderer = piece.AddComponent<SpriteRenderer>();
                renderer.sprite = _sprite;
                renderer.sharedMaterial = source.sharedMaterial;
                renderer.sortingLayerID = source.sortingLayerID;
                renderer.sortingOrder = source.sortingOrder + 1;
                _fragments[i] = renderer;
            }
        }

        public void Play(SpriteRenderer source, Vector2 gravityDirection)
        {
            if (source == null) return;
            EnsureFragments(source);
            _root.SetActive(true);
            _gravityDirection = gravityDirection.sqrMagnitude > 0f ? gravityDirection.normalized : Vector2.down;
            Bounds bounds = source.bounds;
            for (int i = 0; i < _fragments.Length; i++)
            {
                var piece = _fragments[i];
                // 스프라이트의 투명 여백 크기에 영향받지 않고 중심의 좁은 범위에서 생성합니다.
                piece.transform.position = bounds.center + (Vector3)(Random.insideUnitCircle * spawnRadius);
                piece.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                piece.transform.localScale = Vector3.one * Mathf.Max(0.01f, fragmentSize);
                piece.color = fragmentColor;
                // 방사 방향에 위쪽 힘을 더해 파편이 짧은 포물선을 그리며 퍼지게 합니다.
                _velocities[i] = Random.insideUnitCircle.normalized * Random.Range(0.4f, 1f) * burstSpeed
                    - _gravityDirection * burstSpeed * 0.35f;
                _spins[i] = Random.Range(-540f, 540f);
            }
            _elapsed = 0f;
            _playing = true;
        }

        private void Update()
        {
            if (!_playing) return;
            float dt = Time.deltaTime;
            _elapsed += dt;
            float progress = Mathf.Clamp01(_elapsed / Mathf.Max(0.05f, lifetime));
            for (int i = 0; i < _fragments.Length; i++)
            {
                _velocities[i] += _gravityDirection * gravity * dt;
                _fragments[i].transform.position += (Vector3)(_velocities[i] * dt);
                _fragments[i].transform.Rotate(0f, 0f, _spins[i] * dt);
                Color color = fragmentColor;
                color.a *= 1f - progress;
                _fragments[i].color = color;
            }
            if (progress >= 1f) Clear();
        }

        public void Clear()
        {
            _playing = false;
            if (_root != null) _root.SetActive(false);
        }

        private void OnDisable() => Clear();

        private void OnDestroy()
        {
            if (_root != null) Destroy(_root);
            if (_sprite != null) Destroy(_sprite);
        }
    }
}
