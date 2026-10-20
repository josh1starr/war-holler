using System.Collections.Generic;
using UnityEngine;
using WarHoller.Characters;
using WarHoller.Core;
using WarHoller.Gameplay;
using WarHoller.Weapons;

namespace WarHoller.Bots
{
    /// <summary>
    /// Fills empty FFA slots with bots up to PlayerLoadout.TargetFfaParticipants (~12).
    /// </summary>
    public class BotDirector : MonoBehaviour
    {
        public Transform BotRoot;
        public readonly List<Health> Bots = new List<Health>();

        public void SpawnFill(List<Vector3> spawns, int humanCount, System.Func<CharacterDef, WeaponIds, Vector3, Quaternion, Health> spawnCombatant)
        {
            if (BotRoot == null)
            {
                var go = new GameObject("Bots");
                BotRoot = go.transform;
            }

            var needed = Mathf.Max(0, PlayerLoadout.TargetFfaParticipants - humanCount);
            var roster = CharacterRoster.All;
            var weapons = new[] { WeaponIds.LeverActionRifle, WeaponIds.Revolver, WeaponIds.Shotgun };

            for (var i = 0; i < needed; i++)
            {
                var spawn = spawns[i % Mathf.Max(1, spawns.Count)];
                // Offset so bots don't stack on the same point.
                spawn += new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                var character = roster[i % roster.Count];
                var weapon = weapons[i % weapons.Length];
                var rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                var health = spawnCombatant(character, weapon, spawn, rot);
                if (health == null) continue;
                health.IsPlayer = false;
                health.DisplayName = character.DisplayName + " Bot";
                health.TeamOrSlot = 100 + i;

                var bot = health.gameObject.AddComponent<BotController>();
                bot.Weapon = health.GetComponentInChildren<WeaponController>();
                health.transform.SetParent(BotRoot, true);
                Bots.Add(health);
            }
        }
    }
}
