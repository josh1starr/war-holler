using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WarHoller.Core;

namespace WarHoller.Characters
{
    /// <summary>
    /// Six playable characters: 3 Hatfields + 3 McCoys (one female each).
    /// Early-1900s earth-tone cloth; Asmon Hatfield is a labeled parody.
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

            // Earth-tone early-1900s country palette (no neon / modern gamer colors)
            var hatHomespun = new Color(0.42f, 0.32f, 0.2f);
            var hatTrouser = new Color(0.28f, 0.3f, 0.28f);
            var mcShirt = new Color(0.48f, 0.22f, 0.16f);
            var mcTrouser = new Color(0.32f, 0.24f, 0.16f);

            _all = new List<CharacterDef>
            {
                new CharacterDef(
                    "hatfield_jed", "Jed Hatfield", FactionIds.Hatfield, false,
                    hatHomespun, hatTrouser, 0.12f),
                new CharacterDef(
                    "hatfield_asmongold", "Asmon Hatfield", FactionIds.Hatfield, false,
                    new Color(0.38f, 0.34f, 0.28f), new Color(0.3f, 0.28f, 0.22f),
                    hairLength: 0.85f, hasHeadset: true, isParody: true,
                    parodyNote: "Parody / inspired look — not affiliated with any streamer."),
                new CharacterDef(
                    "hatfield_belle", "Belle Hatfield", FactionIds.Hatfield, true,
                    new Color(0.45f, 0.35f, 0.28f), new Color(0.5f, 0.4f, 0.32f), 0.5f),

                new CharacterDef(
                    "mccoy_clem", "Clem McCoy", FactionIds.McCoy, false,
                    mcShirt, mcTrouser, 0.1f),
                new CharacterDef(
                    "mccoy_buck", "Buck McCoy", FactionIds.McCoy, false,
                    new Color(0.4f, 0.2f, 0.14f), new Color(0.35f, 0.28f, 0.18f), 0.18f),
                new CharacterDef(
                    "mccoy_loretta", "Loretta McCoy", FactionIds.McCoy, true,
                    new Color(0.46f, 0.28f, 0.22f), new Color(0.55f, 0.42f, 0.3f), 0.55f)
            };
        }
    }
}
