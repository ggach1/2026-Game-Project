using System.Collections.Generic;
using UnityEngine;

namespace _00.Work.CUH.Code.Title
{
    public static class DisplaySettings
    {
        private const string WidthKey = "Display.Width";
        private const string HeightKey = "Display.Height";
        private const string FullscreenKey = "Display.Fullscreen";

        public static List<Vector2Int> GetResolutions()
        {
            List<Vector2Int> resolutions = new();
            Resolution display = Screen.currentResolution;
            foreach (Resolution resolution in Screen.resolutions)
                AddResolution(resolutions, new Vector2Int(resolution.width, resolution.height));

            Vector2Int[] commonResolutions =
            {
                new(1280, 720), new(1600, 900), new(1920, 1080),
                new(1920, 1200), new(2560, 1440), new(3840, 2160)
            };
            foreach (Vector2Int resolution in commonResolutions)
            {
                if (resolution.x > display.width || resolution.y > display.height) continue;
                AddResolution(resolutions, resolution);
            }

            AddResolution(resolutions, new Vector2Int(Screen.width, Screen.height));
            resolutions.Sort((left, right) => left.x != right.x
                ? left.x.CompareTo(right.x) : left.y.CompareTo(right.y));
            return resolutions;
        }

        public static Vector2Int GetSavedResolution()
        {
            return new Vector2Int(PlayerPrefs.GetInt(WidthKey, Screen.width),
                PlayerPrefs.GetInt(HeightKey, Screen.height));
        }

        public static bool GetSavedFullscreen() => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        public static void Load()
        {
            if (PlayerPrefs.HasKey(WidthKey) == false) return;

            Vector2Int resolution = GetSavedResolution();
            // 모니터를 바꿨을 때 지원하지 않는 저장 해상도를 다시 적용하지 않습니다.
            if (GetResolutions().Contains(resolution) == false)
                resolution = new Vector2Int(Screen.width, Screen.height);
            SetResolution(resolution, GetSavedFullscreen());
        }

        public static void Apply(Vector2Int resolution, bool fullscreen)
        {
            SetResolution(resolution, fullscreen);
            PlayerPrefs.SetInt(WidthKey, resolution.x);
            PlayerPrefs.SetInt(HeightKey, resolution.y);
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static void SetResolution(Vector2Int resolution, bool fullscreen)
        {
            Screen.SetResolution(resolution.x, resolution.y,
                fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        private static void AddResolution(List<Vector2Int> resolutions, Vector2Int resolution)
        {
            if (resolution.x <= 0 || resolution.y <= 0 || resolutions.Contains(resolution)) return;
            resolutions.Add(resolution);
        }
    }
}
