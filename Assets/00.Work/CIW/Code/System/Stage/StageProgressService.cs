using System;
using System.Collections.Generic;
using UnityEngine;

namespace CIW.Code.System.Stage
{
    public class StageProgressService : MonoBehaviour
    {
        [SerializeField] string saveKey = "CIW.StageProgress.v1";

        [Serializable]
        public class SaveData
        {
            public List<string> completedStageIds = new();
        }

        readonly HashSet<string> _completed = new();
        bool _loaded;

        private void Awake()
        {
            EnsureLoaded();
        }

        public bool IsCompleted(string id)
        {
            EnsureLoaded();
            return _completed.Contains(id);
        }

        public bool IsUnlocked(StageDefinition target, WorldDefinition world)
        {
            EnsureLoaded();

            if (target == null || world == null)
                return false;

            if (target.InitiallyUnlocked || _completed.Contains(target.StageId))
                return true;

            foreach (var entry in world.Entries)
            {
                var source = entry?.Stage;

                // 정상적인 완료 스테이지만 해금 목록을 검사합니다.
                if (source == null || !_completed.Contains(source.StageId) || source.UnlockStage == null)
                    continue;

                foreach (string unlockedId in source.UnlockStage)
                {
                    if (unlockedId == target.StageId)
                        return true;
                }
            }

            return false;
        }

        public void Complete(StageDefinition stage)
        {
            EnsureLoaded();

            if (stage == null || string.IsNullOrWhiteSpace(stage.StageId))
                return;

            // 이미 완료한 스테이지는 중복 저장되지 않는다.
            if (_completed.Add(stage.StageId))
                Save();
        }

        public void ResetProgress()
        {
            // 현재 서비스의 저장 키만 초기화합니다. 다른 씬의 진행 기록/환경 설정은 유지합니다.
            _completed.Clear();
            _loaded = true;
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();
        }

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;

            string json = PlayerPrefs.GetString(saveKey, "");

            if (string.IsNullOrEmpty(json))
                return;

            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);

                if (data?.completedStageIds == null)
                    return;

                foreach (string id in data.completedStageIds)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                        _completed.Add(id);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"진행 데이터를 읽지 못함 : {ex.Message}");
            }
        }

        private void Save()
        {
            var data = new SaveData
            {
                completedStageIds = new List<string>(_completed)
            };

            PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(data));

            PlayerPrefs.Save();
        }
    }
}

