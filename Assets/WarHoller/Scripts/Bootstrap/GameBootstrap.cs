using UnityEngine;
using WarHoller.Core;
using WarHoller.Gameplay;
using WarHoller.UI;

namespace WarHoller.Bootstrap
{
    /// <summary>
    /// Runtime entry: constructs menu → character select → FFA match with almost no scene setup.
    /// Drop this on an empty GameObject in Bootstrap.unity (or it auto-creates via RuntimeInitializeOnLoad).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        MainMenuUi _menu;
        CharacterSelectUi _select;
        GameObject _matchRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (Instance != null) return;
            var existing = FindFirstObjectByType<GameBootstrap>();
            if (existing != null) return;

            var go = new GameObject("GameBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            SettingsStore.ApplyAll();
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            TearDownMatch();
            TearDownSelect();
            TearDownMenu();

            ClearWorldExceptBootstrap();

            var menuGo = new GameObject("MainMenuFlow");
            menuGo.transform.SetParent(transform, false);
            _menu = menuGo.AddComponent<MainMenuUi>();
            _menu.OnPlay += HandlePlayQuickStart;
            _menu.OnOpenCharacterSelect += HandleOpenSelect;
            _menu.OnQuit += HandleQuit;
            _menu.Show();
        }

        public void ReturnToMainMenu() => ShowMainMenu();

        void HandlePlayQuickStart()
        {
            // Quick-start FFA on The Holler with current/default loadout; bots fill to ~12.
            PlayerLoadout.Map = MapIds.TheHoller;
            PlayerLoadout.Mode = ModeIds.FreeForAll;
            if (string.IsNullOrEmpty(PlayerLoadout.CharacterId))
                PlayerLoadout.ResetDefaults();
            StartMatch();
        }

        void HandleOpenSelect()
        {
            TearDownMenu();
            var go = new GameObject("CharacterSelectFlow");
            go.transform.SetParent(transform, false);
            _select = go.AddComponent<CharacterSelectUi>();
            _select.OnConfirm += () =>
            {
                TearDownSelect();
                StartMatch();
            };
            _select.OnBack += () =>
            {
                TearDownSelect();
                ShowMainMenu();
            };
            _select.Show();
        }

        void StartMatch()
        {
            TearDownMenu();
            TearDownSelect();
            ClearWorldExceptBootstrap();

            _matchRoot = new GameObject("MatchRoot");
            DontDestroyOnLoad(_matchRoot);
            var session = _matchRoot.AddComponent<GameSession>();
            session.StartMatch();
        }

        void HandleQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void TearDownMenu()
        {
            if (_menu != null)
            {
                var host = _menu.gameObject;
                _menu.Teardown();
                if (host != null) Destroy(host);
                _menu = null;
            }
        }

        void TearDownSelect()
        {
            if (_select != null)
            {
                var host = _select.gameObject;
                _select.Teardown();
                if (host != null) Destroy(host);
                _select = null;
            }
        }

        void TearDownMatch()
        {
            if (_matchRoot != null)
            {
                Destroy(_matchRoot);
                _matchRoot = null;
            }
        }

        void ClearWorldExceptBootstrap()
        {
            // Destroy leftover cameras / lights / canvas from previous flow (except DDOL bootstrap).
            foreach (var cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (cam.transform.root == transform) continue;
                if (_matchRoot != null && cam.transform.IsChildOf(_matchRoot.transform)) continue;
                Destroy(cam.gameObject);
            }

            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas.transform.root == transform) continue;
                Destroy(canvas.gameObject);
            }
        }
    }
}
