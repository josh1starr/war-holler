using System.Collections.Generic;
using UnityEngine;
using WarHoller.Characters;
using WarHoller.Core;
using WarHoller.Gameplay;
using WarHoller.Maps;
using WarHoller.Weapons;

namespace WarHoller.Bots
{
    /// <summary>
    /// Fills FFA slots (~12) or team modes to 6v6 with bots.
    /// </summary>
    public class BotDirector : MonoBehaviour
    {
        public Transform BotRoot;
        public readonly List<Health> Bots = new List<Health>();

        public void SpawnFill(List<Vector3> spawns, int humanCount,
            System.Func<CharacterDef, WeaponIds, Vector3, Quaternion, Health> spawnCombatant)
        {
            EnsureRoot();
            var needed = Mathf.Max(0, PlayerLoadout.TargetFfaParticipants - humanCount);
            var roster = CharacterRoster.All;
            var weapons = new[] { WeaponIds.LeverActionRifle, WeaponIds.Revolver, WeaponIds.Shotgun };

            for (var i = 0; i < needed; i++)
            {
                var spawn = spawns[i % Mathf.Max(1, spawns.Count)];
                spawn += new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                var character = roster[i % roster.Count];
                var weapon = weapons[i % weapons.Length];
                var rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                var health = spawnCombatant(character, weapon, spawn, rot);
                if (health == null) continue;
                FinishBot(health, character.DisplayName + " Bot", 100 + i);
            }
        }

        public void SpawnTeamFill(MapLayout map, FactionIds playerTeam, int humanCountOnPlayerTeam,
            System.Func<CharacterDef, WeaponIds, Vector3, Quaternion, FactionIds, Health> spawnCombatant)
        {
            EnsureRoot();
            var weapons = new[] { WeaponIds.LeverActionRifle, WeaponIds.Revolver, WeaponIds.Shotgun };
            var botIndex = 0;

            foreach (FactionIds team in new[] { FactionIds.Hatfield, FactionIds.McCoy })
            {
                var already = team == playerTeam ? humanCountOnPlayerTeam : 0;
                var needed = Mathf.Max(0, PlayerLoadout.TeamSize - already);
                var roster = new List<CharacterDef>(CharacterRoster.ByFaction(team));
                if (roster.Count == 0) continue;

                for (var i = 0; i < needed; i++)
                {
                    var spawn = map.PickSpawn(team, preferTeamPad: true);
                    spawn += new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                    var character = roster[i % roster.Count];
                    var weapon = weapons[(botIndex + i) % weapons.Length];
                    var face = team == FactionIds.Hatfield ? 90f : -90f;
                    var rot = Quaternion.Euler(0f, face + Random.Range(-30f, 30f), 0f);
                    var health = spawnCombatant(character, weapon, spawn, rot, team);
                    if (health == null) continue;
                    FinishBot(health, character.DisplayName + " Bot", (int)team * 10 + i);
                }

                botIndex += needed;
            }
        }

        void FinishBot(Health health, string name, int slot)
        {
            health.IsPlayer = false;
            health.DisplayName = name;
            health.TeamOrSlot = slot;
            var bot = health.gameObject.AddComponent<BotController>();
            bot.Weapon = health.GetComponentInChildren<WeaponController>();
            health.transform.SetParent(BotRoot, true);
            Bots.Add(health);
        }

        void EnsureRoot()
        {
            if (BotRoot != null) return;
            var go = new GameObject("Bots");
            BotRoot = go.transform;
        }
    }
}
