using System;
using System.Collections.Generic;
using UnityEngine;
using WarHoller.Audio;
using WarHoller.Bots;
using WarHoller.Characters;
using WarHoller.Core;
using WarHoller.Maps;
using WarHoller.UI;
using WarHoller.Weapons;

namespace WarHoller.Gameplay
{
    /// <summary>
    /// Free-for-all match: first to 25 kills OR 10:00 timer (most kills wins).
    /// EXTENSION: ModeIds.CaptureTheFlag / ZoneControl branches belong here.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public const int KillLimit = 25;
        public const float MatchSeconds = 600f;

        public TheHollerBuilder Map;
        public BotDirector Bots;
        public VoiceLinePlayer Voice;
        public MatchHud Hud;
        public EndScreenUi EndScreen;

        readonly Dictionary<Health, int> _kills = new Dictionary<Health, int>();
        readonly Dictionary<Health, int> _streaks = new Dictionary<Health, int>();
        readonly List<string> _killFeed = new List<string>();

        Health _player;
        float _timeLeft;
        bool _running;
        bool _ended;

        public float TimeLeft => _timeLeft;
        public IReadOnlyDictionary<Health, int> Kills => _kills;
        public IReadOnlyList<string> KillFeed => _killFeed;
        public bool IsRunning => _running;

        public void StartMatch()
        {
            // EXTENSION: switch (PlayerLoadout.Mode) for CTF / ZoneControl.
            if (PlayerLoadout.Mode != ModeIds.FreeForAll)
                Debug.LogWarning("[GameSession] Mode " + PlayerLoadout.Mode + " not implemented — falling back to FFA.");

            if (PlayerLoadout.Map != MapIds.TheHoller)
                Debug.LogWarning("[GameSession] Map " + PlayerLoadout.Map + " not built — using The Holler.");

            _timeLeft = MatchSeconds;
            _running = true;
            _ended = false;
            _kills.Clear();
            _streaks.Clear();
            _killFeed.Clear();

            if (Map == null) Map = gameObject.AddComponent<TheHollerBuilder>();
            Map.Build();

            if (Voice == null) Voice = gameObject.AddComponent<VoiceLinePlayer>();
            if (Bots == null) Bots = gameObject.AddComponent<BotDirector>();

            _player = SpawnCombatant(PlayerLoadout.SelectedCharacter, PlayerLoadout.Weapon,
                Map.SpawnPoints[0], Quaternion.identity, isPlayer: true);

            Bots.SpawnFill(Map.SpawnPoints, humanCount: 1, (c, w, p, r) =>
                SpawnCombatant(c, w, p, r, isPlayer: false));

            foreach (var h in FindObjectsByType<Health>(FindObjectsSortMode.None))
            {
                _kills[h] = 0;
                _streaks[h] = 0;
                h.OnDied += HandleDeath;
            }

            if (Hud == null)
            {
                var hudGo = new GameObject("MatchHud");
                Hud = hudGo.AddComponent<MatchHud>();
            }
            Hud.Bind(this, _player);

            var pc = _player.GetComponent<PlayerController>();
            if (pc != null) pc.LockCursor(true);
        }

        Health SpawnCombatant(CharacterDef character, WeaponIds weapon, Vector3 pos, Quaternion rot, bool isPlayer)
        {
            var go = CharacterVisualBuilder.Build(character, transform, includeCollider: true);
            go.transform.SetPositionAndRotation(pos, rot);
            go.tag = isPlayer ? "Player" : "Bot";

            var health = go.AddComponent<Health>();
            health.DisplayName = isPlayer ? character.DisplayName : character.DisplayName;
            health.IsPlayer = isPlayer;
            health.MaxHp = 100f;
            health.FullHeal();

            go.AddComponent<CombatantMotor>();

            WeaponController weaponCtrl;
            Camera cam = null;

            if (isPlayer)
            {
                var pc = go.AddComponent<PlayerController>();
                weaponCtrl = go.AddComponent<WeaponController>();
                pc.Weapon = weaponCtrl;
                // Camera created in PlayerController.Awake — configure next frame-safe:
                cam = pc.PlayerCamera;
                weaponCtrl.Configure(weapon, health, cam);
            }
            else
            {
                weaponCtrl = go.AddComponent<WeaponController>();
                // Bots aim from head height without a camera.
                var aim = new GameObject("BotAim");
                aim.transform.SetParent(go.transform, false);
                aim.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                var fakeCam = aim.AddComponent<Camera>();
                fakeCam.enabled = false;
                weaponCtrl.Configure(weapon, health, fakeCam);
            }

            return health;
        }

        void HandleDeath(Health victim, Health killer)
        {
            if (_ended) return;

            if (killer != null && killer != victim)
            {
                if (!_kills.ContainsKey(killer)) _kills[killer] = 0;
                _kills[killer]++;

                if (!_streaks.ContainsKey(killer)) _streaks[killer] = 0;
                _streaks[killer]++;

                PushFeed(killer.DisplayName + " iced " + victim.DisplayName);

                if (killer.IsPlayer && Voice != null)
                {
                    Voice.PlayKill();
                    var streak = _streaks[killer];
                    if (streak == 3) Voice.PlayTripleStreak();
                    if (streak == 5) Voice.PlayFiveStreak();
                }

                if (_kills[killer] >= KillLimit)
                {
                    EndMatch(killer);
                    return;
                }
            }
            else
            {
                PushFeed(victim.DisplayName + " bit the dust");
            }

            if (_streaks.ContainsKey(victim)) _streaks[victim] = 0;
            StartCoroutine(RespawnRoutine(victim));
        }

        System.Collections.IEnumerator RespawnRoutine(Health victim)
        {
            yield return new WaitForSeconds(2.5f);
            if (_ended || victim == null) yield break;

            var spawn = Map.SpawnPoints[UnityEngine.Random.Range(0, Map.SpawnPoints.Count)];
            var motor = victim.GetComponent<CombatantMotor>();
            var rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
            if (motor != null) motor.Warp(spawn + Vector3.up * 0.2f, rot);
            else victim.transform.SetPositionAndRotation(spawn, rot);

            victim.FullHeal();
            var weapon = victim.GetComponentInChildren<WeaponController>();
            if (weapon != null && weapon.Stats != null)
                weapon.Configure(weapon.Stats.Id, victim, weapon.AimCamera);
        }

        void Update()
        {
            if (!_running || _ended) return;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                Health leader = null;
                var best = -1;
                foreach (var kv in _kills)
                {
                    if (kv.Value > best)
                    {
                        best = kv.Value;
                        leader = kv.Key;
                    }
                }
                EndMatch(leader);
            }
        }

        void EndMatch(Health winner)
        {
            if (_ended) return;
            _ended = true;
            _running = false;

            var pc = _player != null ? _player.GetComponent<PlayerController>() : null;
            if (pc != null)
            {
                pc.InputEnabled = false;
                pc.LockCursor(false);
            }

            if (EndScreen == null)
            {
                var go = new GameObject("EndScreen");
                EndScreen = go.AddComponent<EndScreenUi>();
            }
            EndScreen.Show(winner != null ? winner.DisplayName : "Nobody", _kills, () =>
            {
                // Return to menu via bootstrap reload of menu flow.
                Bootstrap.GameBootstrap.Instance?.ReturnToMainMenu();
            });
        }

        void PushFeed(string line)
        {
            _killFeed.Insert(0, line);
            if (_killFeed.Count > 6) _killFeed.RemoveAt(_killFeed.Count - 1);
        }
    }
}
