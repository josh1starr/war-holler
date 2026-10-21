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
    /// Match host: FFA (25 kills), CTF (3 pig captures), Zone Control (500 pts). 10:00 timer; most score wins.
    /// Team modes fill 6v6 with bots. Local only — no netcode.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public const int KillLimit = 25;
        public const float MatchSeconds = 600f;

        public MapLayout Map;
        public BotDirector Bots;
        public VoiceLinePlayer Voice;
        public MatchHud Hud;
        public EndScreenUi EndScreen;
        public CaptureTheFlagMode Ctf;
        public ZoneControlMode Zones;

        readonly Dictionary<Health, int> _kills = new Dictionary<Health, int>();
        readonly Dictionary<Health, int> _streaks = new Dictionary<Health, int>();
        readonly Dictionary<Health, FactionIds> _teams = new Dictionary<Health, FactionIds>();
        readonly List<Health> _combatants = new List<Health>();
        readonly List<string> _killFeed = new List<string>();

        Health _player;
        FactionIds _playerTeam;
        float _timeLeft;
        bool _running;
        bool _ended;
        ModeIds _mode;

        public float TimeLeft => _timeLeft;
        public IReadOnlyDictionary<Health, int> Kills => _kills;
        public IReadOnlyList<string> KillFeed => _killFeed;
        public bool IsRunning => _running;
        public ModeIds Mode => _mode;
        public MapLayout ActiveMap => Map;
        public FactionIds PlayerTeam => _playerTeam;
        public IReadOnlyList<Health> Combatants => _combatants;

        public FactionIds TeamOf(Health h) =>
            h != null && _teams.TryGetValue(h, out var t) ? t : FactionIds.Hatfield;

        public void StartMatch()
        {
            _mode = PlayerLoadout.Mode;
            _timeLeft = MatchSeconds;
            _running = true;
            _ended = false;
            _kills.Clear();
            _streaks.Clear();
            _teams.Clear();
            _combatants.Clear();
            _killFeed.Clear();

            Map = BuildMap(PlayerLoadout.Map);
            if (Voice == null) Voice = gameObject.AddComponent<VoiceLinePlayer>();
            if (Bots == null) Bots = gameObject.AddComponent<BotDirector>();

            _playerTeam = PlayerLoadout.SelectedCharacter.Faction;
            var playerSpawn = Map.PickSpawn(_playerTeam, preferTeamPad: PlayerLoadout.IsTeamMode);
            _player = SpawnCombatant(PlayerLoadout.SelectedCharacter, PlayerLoadout.Weapon,
                playerSpawn, Quaternion.identity, isPlayer: true, _playerTeam);

            if (PlayerLoadout.IsTeamMode)
            {
                Bots.SpawnTeamFill(Map, _playerTeam, humanCountOnPlayerTeam: 1,
                    (c, w, p, r, team) => SpawnCombatant(c, w, p, r, isPlayer: false, team));
            }
            else
            {
                Bots.SpawnFill(Map.SpawnPoints, humanCount: 1, (c, w, p, r) =>
                    SpawnCombatant(c, w, p, r, isPlayer: false, c.Faction));
            }

            foreach (var h in _combatants)
            {
                _kills[h] = 0;
                _streaks[h] = 0;
                h.OnDied += HandleDeath;
            }

            if (_mode == ModeIds.CaptureTheFlag)
            {
                Ctf = gameObject.AddComponent<CaptureTheFlagMode>();
                Ctf.Begin(Map);
                Ctf.OnCapture += (team, name) =>
                    PushFeed($"{name} scored a pig for {team}! ({Ctf.Captures(team)}/{CaptureTheFlagMode.CaptureLimit})");
                Ctf.OnReachedLimit += team => EndMatchTeam(team, $"{team} wins on pig captures!");
            }
            else if (_mode == ModeIds.ZoneControl)
            {
                Zones = gameObject.AddComponent<ZoneControlMode>();
                Zones.Begin(Map);
                Zones.OnReachedLimit += team => EndMatchTeam(team, $"{team} wins Zone Control!");
            }

            if (Hud == null)
            {
                var hudGo = new GameObject("MatchHud");
                Hud = hudGo.AddComponent<MatchHud>();
            }
            Hud.Bind(this, _player);

            var pc = _player.GetComponent<PlayerController>();
            if (pc != null) pc.LockCursor(true);

            // Wire bot objective hooks after modes exist
            foreach (var bot in FindObjectsByType<BotController>(FindObjectsSortMode.None))
                bot.BindSession(this);
        }

        MapLayout BuildMap(MapIds id)
        {
            switch (id)
            {
                case MapIds.WowMart:
                {
                    var b = gameObject.AddComponent<WowMartBuilder>();
                    b.Build();
                    return b;
                }
                case MapIds.MethLab:
                {
                    var b = gameObject.AddComponent<MethLabBuilder>();
                    b.Build();
                    return b;
                }
                default:
                {
                    var b = gameObject.AddComponent<TheHollerBuilder>();
                    b.Build();
                    return b;
                }
            }
        }

        Health SpawnCombatant(CharacterDef character, WeaponIds weapon, Vector3 pos, Quaternion rot,
            bool isPlayer, FactionIds team)
        {
            var go = CharacterVisualBuilder.Build(character, transform, includeCollider: true);
            go.transform.SetPositionAndRotation(pos, rot);
            go.tag = isPlayer ? "Player" : "Untagged";

            var health = go.AddComponent<Health>();
            health.DisplayName = character.DisplayName;
            health.IsPlayer = isPlayer;
            health.MaxHp = 100f;
            health.TeamOrSlot = (int)team;
            health.FullHeal();

            go.AddComponent<CombatantMotor>();

            WeaponController weaponCtrl;
            if (isPlayer)
            {
                var pc = go.AddComponent<PlayerController>();
                weaponCtrl = go.AddComponent<WeaponController>();
                pc.Weapon = weaponCtrl;
                weaponCtrl.Configure(weapon, health, pc.PlayerCamera);
            }
            else
            {
                weaponCtrl = go.AddComponent<WeaponController>();
                var aim = new GameObject("BotAim");
                aim.transform.SetParent(go.transform, false);
                aim.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                var fakeCam = aim.AddComponent<Camera>();
                fakeCam.enabled = false;
                weaponCtrl.Configure(weapon, health, fakeCam);
            }

            _teams[health] = team;
            _combatants.Add(health);
            return health;
        }

        void HandleDeath(Health victim, Health killer)
        {
            if (_ended) return;

            // Friendly fire off in team modes
            if (PlayerLoadout.IsTeamMode && killer != null && killer != victim &&
                TeamOf(killer) == TeamOf(victim))
            {
                // Undo would be complex; prevent by zeroing in WeaponController ideally.
                // Here: ignore kill credit but still respawn.
                PushFeed(victim.DisplayName + " got cluttered");
                if (_streaks.ContainsKey(victim)) _streaks[victim] = 0;
                StartCoroutine(RespawnRoutine(victim));
                return;
            }

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

                if (_mode == ModeIds.FreeForAll && _kills[killer] >= KillLimit)
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

            var team = TeamOf(victim);
            var spawn = Map.PickSpawn(team, preferTeamPad: PlayerLoadout.IsTeamMode);
            spawn += new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            var motor = victim.GetComponent<CombatantMotor>();
            var rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
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

            if (_mode == ModeIds.CaptureTheFlag && Ctf != null)
                Ctf.Tick(_combatants, TeamOf);
            else if (_mode == ModeIds.ZoneControl && Zones != null)
                Zones.Tick(_combatants, TeamOf);

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                EndByTimer();
            }
        }

        void EndByTimer()
        {
            if (_mode == ModeIds.CaptureTheFlag && Ctf != null)
            {
                if (Ctf.HatfieldCaptures == Ctf.McCoyCaptures)
                    EndMatchTeam(null, "Time! Draw on pig captures.");
                else if (Ctf.HatfieldCaptures > Ctf.McCoyCaptures)
                    EndMatchTeam(FactionIds.Hatfield, "Time! Hatfields lead on pigs.");
                else
                    EndMatchTeam(FactionIds.McCoy, "Time! McCoys lead on pigs.");
                return;
            }

            if (_mode == ModeIds.ZoneControl && Zones != null)
            {
                if (Mathf.Approximately(Zones.HatfieldScore, Zones.McCoyScore))
                    EndMatchTeam(null, "Time! Zone Control draw.");
                else if (Zones.HatfieldScore > Zones.McCoyScore)
                    EndMatchTeam(FactionIds.Hatfield, "Time! Hatfields lead zones.");
                else
                    EndMatchTeam(FactionIds.McCoy, "Time! McCoys lead zones.");
                return;
            }

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

        void EndMatchTeam(FactionIds? team, string subtitle)
        {
            if (_ended) return;
            var winnerName = team.HasValue ? team.Value.ToString() + "s" : "Nobody";
            Finish(winnerName, subtitle);
        }

        void EndMatch(Health winner)
        {
            if (_ended) return;
            Finish(winner != null ? winner.DisplayName : "Nobody", null);
        }

        void Finish(string winnerName, string subtitle)
        {
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

            EndScreen.Show(winnerName, _kills, _mode, Ctf, Zones, subtitle, () =>
            {
                Bootstrap.GameBootstrap.Instance?.ReturnToMainMenu();
            });
        }

        void PushFeed(string line)
        {
            _killFeed.Insert(0, line);
            if (_killFeed.Count > 6) _killFeed.RemoveAt(_killFeed.Count - 1);
        }

        public bool AreEnemies(Health a, Health b)
        {
            if (a == null || b == null) return false;
            if (!PlayerLoadout.IsTeamMode) return a != b;
            return TeamOf(a) != TeamOf(b);
        }
    }
}
