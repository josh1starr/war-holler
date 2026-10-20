using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WarHoller.Core;

namespace WarHoller.Characters
{
    /// <summary>
    /// Six playable characters: 3 Hatfields + 3 McCoys (one female each).
    /// Includes an Asmongold-inspired Hatfield parody (labeled, not official).
    /// </summary>
    public static class CharacterRoster
    {
        static List<CharacterDef> _all;

        public static IReadOnlyList<CharacterDef> All
        {
            get
            {
                Ensure();
                return _all;
            }
        }

        public static CharacterDef DefaultHatfield => Get("hatfield_jed");
        public static CharacterDef DefaultMcCoy => Get("mccoy_clem");

        public static CharacterDef Get(string id)
        {
            Ensure();
            return _all.FirstOrDefault(c => c.Id == id) ?? _all[0];
        }

        public static IEnumerable<CharacterDef> ByFaction(FactionIds faction)
        {
            Ensure();
            return _all.Where(c => c.Faction == faction);
        }

        static void Ensure()
        {
            if (_all != null) return;

            var hatWood = new Color(0.45f, 0.28f, 0.12f);
            var hatDenim = new Color(0.25f, 0.35f, 0.55f);
            var mcRed = new Color(0.55f, 0.15f, 0.12f);
            var mcFlannel = new Color(0.55f, 0.22f, 0.18f);

            _all = new List<CharacterDef>
            {
                new CharacterDef(
                    "hatfield_jed", "Jed Hatfield", FactionIds.Hatfield, false,
                    hatWood, hatDenim, 0.15f),
                new CharacterDef(
                    "hatfield_asmongold", "Asmon Hatfield", FactionIds.Hatfield, false,
                    new Color(0.35f, 0.32f, 0.28f), new Color(0.2f, 0.55f, 0.35f),
                    hairLength: 0.85f, hasHeadset: true, isParody: true,
                    parodyNote: "Parody / inspired look — not affiliated with any streamer."),
                new CharacterDef(
                    "hatfield_belle", "Belle Hatfield", FactionIds.Hatfield, true,
                    new Color(0.5f, 0.3f, 0.2f), new Color(0.7f, 0.45f, 0.35f), 0.55f),

                new CharacterDef(
                    "mccoy_clem", "Clem McCoy", FactionIds.McCoy, false,
                    mcRed, mcFlannel, 0.12f),
                new CharacterDef(
                    "mccoy_buck", "Buck McCoy", FactionIds.McCoy, false,
                    new Color(0.4f, 0.18f, 0.12f), new Color(0.65f, 0.5f, 0.25f), 0.2f),
                new CharacterDef(
                    "mccoy_loretta", "Loretta McCoy", FactionIds.McCoy, true,
                    new Color(0.5f, 0.2f, 0.15f), new Color(0.85f, 0.7f, 0.45f), 0.6f)
            };
        }
    }
}
