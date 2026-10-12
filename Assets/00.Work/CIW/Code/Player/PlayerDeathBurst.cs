using UnityEngine;
using UnityEngine.Rendering;

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

        [Header("Fall Death")]
        [SerializeField, Range(1f, 2f)] float fallCountMultiplier = 1.5f;
        [SerializeField, Min(0f)] float fallUpwardSpeed = 6f;
        [SerializeField, Min(0f)] float fallSideSpeed = 1.8f;
        [SerializeField] bool keepFallBurstVisible = true;
        [SerializeField, Min(0f)] float fallShakeDuration = 0.22f;
        [SerializeField, Min(0f)] float fallShakeStrength = 0.12f;

        GameObject _root;
        Sprite _sprite;
        SpriteRenderer[] _fragments;
        Vector2[] _velocities;
        float[] _spins;
        Vector2 _gravityDirection;
        float _elapsed;
        bool _playing;
        int _activeCount;
        Camera _shakeCamera;
        Vector3 _renderOffset;
        bool _offsetApplied;
        float _shakeStarted;
        float _shakeUntil;

        private void EnsureFragments(SpriteRenderer source)
        {
            if (_root != null) return;
            // 월드 공간에 두어 플레이어가 리스폰 위치로 옮겨져도 파편이 따라가지 않습니다.
            _root = new GameObject("Player death fragments");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_root, gameObject.scene);
            _sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
            int count = Mathf.Clamp(Mathf.CeilToInt(fragmentCount * fallCountMultiplier), 4, 128);
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

        public void Play(SpriteRenderer source, Vector2 gravityDirection, bool isFall = false)
        {
            if (source == null) return;
            Clear();
            EnsureFragments(source);
            _root.SetActive(true);
            _gravityDirection = gravityDirection.sqrMagnitude > 0f ? gravityDirection.normalized : Vector2.down;
            Bounds bounds = source.bounds;
            Vector3 origin = bounds.center;
            Camera camera = Camera.main;
            if (isFall && camera != null)
            {
                _shakeCamera = camera;
                _shakeStarted = Time.unscaledTime;
                _shakeUntil = _shakeStarted + fallShakeDuration;
                // 낙사 영역은 화면 아래에 있을 수 있습니다. 물리 위치가 아닌 파편 원점만 보정합니다.
                if (keepFallBurstVisible)
                {
                    Vector3 viewport = camera.WorldToViewportPoint(origin);
                    if (viewport.z > 0f)
                    {
                        viewport.x = Mathf.Clamp(viewport.x, 0.06f, 0.94f);
                        viewport.y = Mathf.Clamp(viewport.y, 0.06f, 0.94f);
                        origin = camera.ViewportToWorldPoint(viewport);
                    }
                }
            }
            _activeCount = Mathf.Min(_fragments.Length, isFall
                ? Mathf.CeilToInt(fragmentCount * fallCountMultiplier) : Mathf.Clamp(fragmentCount, 4, 64));
            for (int i = 0; i < _fragments.Length; i++)
            {
                var piece = _fragments[i];
                piece.gameObject.SetActive(i < _activeCount);
                if (i >= _activeCount) continue;
                // 스프라이트의 투명 여백 크기에 영향받지 않고 중심의 좁은 범위에서 생성합니다.
                piece.transform.position = origin + (Vector3)(Random.insideUnitCircle * spawnRadius);
                piece.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                piece.transform.localScale = Vector3.one * Mathf.Max(0.01f, fragmentSize);
                piece.color = fragmentColor;
                // 방사 방향에 위쪽 힘을 더해 파편이 짧은 포물선을 그리며 퍼지게 합니다.
                _velocities[i] = Random.insideUnitCircle.normalized * Random.Range(0.4f, 1f) * burstSpeed
                    - _gravityDirection * burstSpeed * 0.35f;
                // 낙사만 좁은 좌우 폭과 강한 반중력 방향 속도를 사용합니다. 일반 사망은 기존 값 유지.
                if (isFall)
                {
                    Vector2 side = new Vector2(-_gravityDirection.y, _gravityDirection.x);
                    _velocities[i] = side * Random.Range(-fallSideSpeed, fallSideSpeed)
                        - _gravityDirection * Random.Range(0.75f, 1.15f) * fallUpwardSpeed;
                }
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
            for (int i = 0; i < _activeCount; i++)
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
            RestoreCamera();
            _shakeCamera = null;
            _shakeUntil = 0f;
            _playing = false;
            if (_root != null) _root.SetActive(false);
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
            Camera.onPreCull += BeginBuiltInCamera;
            Camera.onPostRender += EndBuiltInCamera;
        }

        private void BeginCameraRendering(ScriptableRenderContext context, Camera camera) => ApplyCameraShake(camera);
        private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _shakeCamera) RestoreCamera();
        }
        private void BeginBuiltInCamera(Camera camera)
        {
            if (GraphicsSettings.currentRenderPipeline == null) ApplyCameraShake(camera);
        }
        private void EndBuiltInCamera(Camera camera)
        {
            if (GraphicsSettings.currentRenderPipeline == null && camera == _shakeCamera) RestoreCamera();
        }

        private void ApplyCameraShake(Camera camera)
        {
            if (camera != _shakeCamera || Time.unscaledTime >= _shakeUntil) return;
            RestoreCamera();
            float elapsed = Time.unscaledTime - _shakeStarted;
            float fade = 1f - Mathf.Clamp01(elapsed / Mathf.Max(0.001f, fallShakeDuration));
            Vector2 noise = new Vector2(Mathf.Sin(elapsed * 137f + 1f), Mathf.Sin(elapsed * 173f));
            _renderOffset = (camera.transform.right * noise.x + camera.transform.up * noise.y)
                * (fallShakeStrength * fade * fade);
            // 렌더링 동안만 오프셋을 더하고 즉시 복원해 카메라 추적 코드와 리스폰 위치를 건드리지 않습니다.
            camera.transform.position += _renderOffset;
            _offsetApplied = true;
        }

        private void RestoreCamera()
        {
            if (_offsetApplied && _shakeCamera != null) _shakeCamera.transform.position -= _renderOffset;
            _offsetApplied = false;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;
            Camera.onPreCull -= BeginBuiltInCamera;
            Camera.onPostRender -= EndBuiltInCamera;
            Clear();
        }

        private void OnDestroy()
        {
            if (_root != null) Destroy(_root);
            if (_sprite != null) Destroy(_sprite);
        }
    }
}
