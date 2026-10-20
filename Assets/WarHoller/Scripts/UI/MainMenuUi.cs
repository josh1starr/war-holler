using System;
using UnityEngine;
using UnityEngine.UI;
using WarHoller.Characters;
using WarHoller.Core;
using WarHoller.Maps;

namespace WarHoller.UI
{
    /// <summary>
    /// Brand-first barn menu: War Holler title, Play, settings, Quit, two mannequins.
    /// </summary>
    public class MainMenuUi : MonoBehaviour
    {
        public event Action OnPlay;
        public event Action OnOpenCharacterSelect;
        public event Action OnQuit;

        BarnBackdropBuilder _barn;
        GameObject _hatMannequin;
        GameObject _mcMannequin;
        Canvas _canvas;

        Text _resLabel;
        Text _qualityLabel;

        public void Show()
        {
            UiFactory.EnsureEventSystem();
            SettingsStore.ApplyAll();

            _barn = gameObject.AddComponent<BarnBackdropBuilder>();
            _barn.Build();

            SpawnMannequins();
            BuildUi();
        }

        void SpawnMannequins()
        {
            var hat = CharacterRoster.DefaultHatfield;
            var mc = CharacterRoster.DefaultMcCoy;

            _hatMannequin = CharacterVisualBuilder.Build(hat, transform, includeCollider: false);
            _hatMannequin.transform.position = new Vector3(-2.2f, 0f, 1.5f);
            _hatMannequin.transform.rotation = Quaternion.Euler(0f, 35f, 0f);

            _mcMannequin = CharacterVisualBuilder.Build(mc, transform, includeCollider: false);
            _mcMannequin.transform.position = new Vector3(2.2f, 0f, 1.5f);
            _mcMannequin.transform.rotation = Quaternion.Euler(0f, -35f, 0f);

            // Click proxies via invisible UI buttons mapped to screen positions of characters —
            // also add world colliders + simple screen ray for clicking mannequins.
            foreach (var col in _hatMannequin.GetComponentsInChildren<Collider>())
                Destroy(col);
            foreach (var col in _mcMannequin.GetComponentsInChildren<Collider>())
                Destroy(col);

            AddClickProxy(_hatMannequin, "Hatfield");
            AddClickProxy(_mcMannequin, "McCoy");
        }

        void AddClickProxy(GameObject mannequin, string id)
        {
            var proxy = new GameObject("ClickProxy_" + id);
            proxy.transform.SetParent(mannequin.transform, false);
            proxy.transform.localPosition = new Vector3(0f, 1f, 0f);
            var box = proxy.AddComponent<BoxCollider>();
            box.size = new Vector3(1.2f, 2.2f, 1f);
            var click = proxy.AddComponent<MannequinClick>();
            click.OnClicked = () => OnOpenCharacterSelect?.Invoke();
        }

        void BuildUi()
        {
            _canvas = UiFactory.CreateCanvas("MainMenuCanvas", 10);
            var root = _canvas.transform;

            // Brand hero title
            var title = UiFactory.Label(root, "WAR HOLLER", 96, TextAnchor.UpperCenter,
                new Color(0.95f, 0.82f, 0.35f), new Vector2(0f, -60f), new Vector2(1200f, 140f));
            title.fontStyle = FontStyle.Bold;
            var rt = title.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);

            UiFactory.Label(root, "Redneck comedy FPS", 28, TextAnchor.UpperCenter,
                new Color(0.85f, 0.75f, 0.55f), new Vector2(0f, -160f), new Vector2(800f, 40f))
                .GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            title.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);

            // Play — bottom center
            var play = UiFactory.CreateButton(root, "PLAY", new Vector2(0f, 80f), new Vector2(280f, 72f),
                () => OnPlay?.Invoke(), new Color(0.55f, 0.2f, 0.12f, 0.95f));
            var playRt = play.GetComponent<RectTransform>();
            playRt.anchorMin = new Vector2(0.5f, 0f);
            playRt.anchorMax = new Vector2(0.5f, 0f);

            // Quit — bottom left
            var quit = UiFactory.CreateButton(root, "QUIT", new Vector2(120f, 80f), new Vector2(160f, 56f),
                () => OnQuit?.Invoke(), new Color(0.2f, 0.15f, 0.1f, 0.9f));
            var quitRt = quit.GetComponent<RectTransform>();
            quitRt.anchorMin = new Vector2(0f, 0f);
            quitRt.anchorMax = new Vector2(0f, 0f);

            // Character hint
            UiFactory.Label(root, "Click Hatfield or McCoy → Character Select", 22, TextAnchor.LowerCenter,
                new Color(0.8f, 0.7f, 0.5f), new Vector2(0f, 170f), new Vector2(700f, 36f));

            BuildSettingsPanel(root);
        }

        void BuildSettingsPanel(Transform root)
        {
            var panel = UiFactory.Panel(root, "Settings", new Vector2(220f, 0f), new Vector2(400f, 520f),
                new Color(0.08f, 0.06f, 0.04f, 0.82f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));

            var p = panel.transform;
            UiFactory.Label(p, "SETTINGS", 32, TextAnchor.UpperLeft,
                new Color(0.95f, 0.85f, 0.5f), new Vector2(20f, -20f), new Vector2(360f, 40f));

            UiFactory.Label(p, "Video", 24, TextAnchor.UpperLeft,
                new Color(0.85f, 0.75f, 0.55f), new Vector2(20f, -70f), new Vector2(200f, 30f));

            _resLabel = UiFactory.Label(p, ResText(), 20, TextAnchor.MiddleLeft,
                Color.white, new Vector2(20f, -110f), new Vector2(360f, 28f));
            UiFactory.CreateButton(p, "Res -", new Vector2(-90f, -150f), new Vector2(100f, 36f), CycleResDown);
            UiFactory.CreateButton(p, "Res +", new Vector2(30f, -150f), new Vector2(100f, 36f), CycleResUp);

            _qualityLabel = UiFactory.Label(p, QualityText(), 20, TextAnchor.MiddleLeft,
                Color.white, new Vector2(20f, -200f), new Vector2(360f, 28f));
            UiFactory.CreateButton(p, "Quality", new Vector2(-40f, -240f), new Vector2(160f, 36f), CycleQuality);

            UiFactory.CreateButton(p, SettingsStore.Fullscreen ? "Fullscreen: ON" : "Fullscreen: OFF",
                new Vector2(-40f, -290f), new Vector2(200f, 36f), ToggleFullscreen);

            UiFactory.Label(p, "Audio", 24, TextAnchor.UpperLeft,
                new Color(0.85f, 0.75f, 0.55f), new Vector2(20f, -340f), new Vector2(200f, 30f));

            UiFactory.Label(p, "Master", 18, TextAnchor.MiddleLeft, Color.white, new Vector2(20f, -375f), new Vector2(100f, 24f));
            UiFactory.CreateSlider(p, "Master", new Vector2(40f, -400f), new Vector2(280f, 24f), SettingsStore.MasterVolume, v =>
            {
                SettingsStore.MasterVolume = v;
                SettingsStore.SaveAndApply();
            });

            UiFactory.Label(p, "SFX", 18, TextAnchor.MiddleLeft, Color.white, new Vector2(20f, -430f), new Vector2(100f, 24f));
            UiFactory.CreateSlider(p, "SFX", new Vector2(40f, -455f), new Vector2(280f, 24f), SettingsStore.SfxVolume, v =>
            {
                SettingsStore.SfxVolume = v;
                PlayerPrefs.Save();
            });

            UiFactory.Label(p, "Voice", 18, TextAnchor.MiddleLeft, Color.white, new Vector2(20f, -485f), new Vector2(100f, 24f));
            UiFactory.CreateSlider(p, "Voice", new Vector2(40f, -510f), new Vector2(280f, 24f), SettingsStore.VoiceVolume, v =>
            {
                SettingsStore.VoiceVolume = v;
                PlayerPrefs.Save();
            });
        }

        static readonly Vector2Int[] ResOptions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440)
        };

        int ResIndex()
        {
            for (var i = 0; i < ResOptions.Length; i++)
                if (ResOptions[i].x == SettingsStore.ResolutionWidth && ResOptions[i].y == SettingsStore.ResolutionHeight)
                    return i;
            return 2;
        }

        void CycleResUp()
        {
            var i = Mathf.Min(ResOptions.Length - 1, ResIndex() + 1);
            SettingsStore.ResolutionWidth = ResOptions[i].x;
            SettingsStore.ResolutionHeight = ResOptions[i].y;
            SettingsStore.SaveAndApply();
            if (_resLabel != null) _resLabel.text = ResText();
        }

        void CycleResDown()
        {
            var i = Mathf.Max(0, ResIndex() - 1);
            SettingsStore.ResolutionWidth = ResOptions[i].x;
            SettingsStore.ResolutionHeight = ResOptions[i].y;
            SettingsStore.SaveAndApply();
            if (_resLabel != null) _resLabel.text = ResText();
        }

        void CycleQuality()
        {
            var next = (SettingsStore.QualityLevel + 1) % QualitySettings.names.Length;
            SettingsStore.QualityLevel = next;
            SettingsStore.SaveAndApply();
            if (_qualityLabel != null) _qualityLabel.text = QualityText();
        }

        void ToggleFullscreen()
        {
            SettingsStore.Fullscreen = !SettingsStore.Fullscreen;
            SettingsStore.SaveAndApply();
            // Rebuild button label by refreshing panel is heavy; log is fine — Apply changes immediately.
            Debug.Log("Fullscreen: " + SettingsStore.Fullscreen);
            // Force refresh menu labels if canvas still alive
            ShowRefreshFullscreenButton();
        }

        void ShowRefreshFullscreenButton()
        {
            // Find button named with Fullscreen and update text
            if (_canvas == null) return;
            foreach (var t in _canvas.GetComponentsInChildren<Text>())
            {
                if (t.text.StartsWith("Fullscreen"))
                    t.text = SettingsStore.Fullscreen ? "Fullscreen: ON" : "Fullscreen: OFF";
            }
        }

        string ResText() => $"Resolution: {SettingsStore.ResolutionWidth}x{SettingsStore.ResolutionHeight}";
        string QualityText()
        {
            var names = QualitySettings.names;
            var i = Mathf.Clamp(SettingsStore.QualityLevel, 0, names.Length - 1);
            return "Quality: " + names[i];
        }

        void Update()
        {
            // Mannequin click via camera ray
            if (_barn == null || _barn.MenuCamera == null) return;
            if (!Input.GetMouseButtonDown(0)) return;
            var ray = _barn.MenuCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 50f))
            {
                var click = hit.collider.GetComponent<MannequinClick>();
                click?.OnClicked?.Invoke();
            }
        }

        public void Teardown()
        {
            if (_canvas != null) Destroy(_canvas.gameObject);
            Destroy(this);
        }
    }

    public class MannequinClick : MonoBehaviour
    {
        public Action OnClicked;
    }
}
