using UnityEngine;

namespace WarHoller.Core
{
    /// <summary>
    /// Video + audio preferences via PlayerPrefs.
    /// </summary>
    public static class SettingsStore
    {
        const string PrefResW = "wh_res_w";
        const string PrefResH = "wh_res_h";
        const string PrefFullscreen = "wh_fullscreen";
        const string PrefQuality = "wh_quality";
        const string PrefMaster = "wh_master";
        const string PrefSfx = "wh_sfx";
        const string PrefVoice = "wh_voice";

        public static int ResolutionWidth
        {
            get => PlayerPrefs.GetInt(PrefResW, Screen.currentResolution.width);
            set => PlayerPrefs.SetInt(PrefResW, value);
        }

        public static int ResolutionHeight
        {
            get => PlayerPrefs.GetInt(PrefResH, Screen.currentResolution.height);
            set => PlayerPrefs.SetInt(PrefResH, value);
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(PrefFullscreen, 1) == 1;
            set => PlayerPrefs.SetInt(PrefFullscreen, value ? 1 : 0);
        }

        public static int QualityLevel
        {
            get => PlayerPrefs.GetInt(PrefQuality, QualitySettings.GetQualityLevel());
            set => PlayerPrefs.SetInt(PrefQuality, Mathf.Clamp(value, 0, QualitySettings.names.Length - 1));
        }

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(PrefMaster, 0.85f);
            set => PlayerPrefs.SetFloat(PrefMaster, Mathf.Clamp01(value));
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(PrefSfx, 0.9f);
            set => PlayerPrefs.SetFloat(PrefSfx, Mathf.Clamp01(value));
        }

        public static float VoiceVolume
        {
            get => PlayerPrefs.GetFloat(PrefVoice, 1f);
            set => PlayerPrefs.SetFloat(PrefVoice, Mathf.Clamp01(value));
        }

        public static void ApplyAll()
        {
            QualitySettings.SetQualityLevel(QualityLevel, true);
            Screen.SetResolution(ResolutionWidth, ResolutionHeight,
                Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            AudioListener.volume = MasterVolume;
            PlayerPrefs.Save();
        }

        public static void SaveAndApply()
        {
            PlayerPrefs.Save();
            ApplyAll();
        }
    }
}
