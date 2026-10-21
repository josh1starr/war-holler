using System.Collections.Generic;
using UnityEngine;

namespace WarHoller.Core
{
    /// <summary>
    /// Local quick-join playlist: Play advances through map/mode pairs. Bots fill; no online.
    /// </summary>
    public static class LobbyRotation
    {
        public struct Entry
        {
            public MapIds Map;
            public ModeIds Mode;
            public string DisplayName;
        }

        public static readonly IReadOnlyList<Entry> QuickJoinPlaylist = new List<Entry>
        {
            new Entry { Map = MapIds.TheHoller, Mode = ModeIds.FreeForAll, DisplayName = "The Holler — Free for All" },
            new Entry { Map = MapIds.TheHoller, Mode = ModeIds.CaptureTheFlag, DisplayName = "The Holler — Pig Flag" },
            new Entry { Map = MapIds.WowMart, Mode = ModeIds.FreeForAll, DisplayName = "WowMart — Free for All" },
            new Entry { Map = MapIds.WowMart, Mode = ModeIds.ZoneControl, DisplayName = "WowMart — Zone Control" },
            new Entry { Map = MapIds.MethLab, Mode = ModeIds.FreeForAll, DisplayName = "Meth Lab — Free for All" },
            new Entry { Map = MapIds.MethLab, Mode = ModeIds.CaptureTheFlag, DisplayName = "Meth Lab — Pig Flag" },
            new Entry { Map = MapIds.TheHoller, Mode = ModeIds.ZoneControl, DisplayName = "The Holler — Zone Control" },
            new Entry { Map = MapIds.WowMart, Mode = ModeIds.CaptureTheFlag, DisplayName = "WowMart — Pig Flag" },
            new Entry { Map = MapIds.MethLab, Mode = ModeIds.ZoneControl, DisplayName = "Meth Lab — Zone Control" }
        };

        const string PrefKey = "WarHoller.LobbyRotationIndex";

        public static int CurrentIndex
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 0), 0, QuickJoinPlaylist.Count - 1);
            set => PlayerPrefs.SetInt(PrefKey, Mathf.Clamp(value, 0, QuickJoinPlaylist.Count - 1));
        }

        public static Entry Current => QuickJoinPlaylist[CurrentIndex];

        public static Entry PeekNext()
        {
            var i = (CurrentIndex + 1) % QuickJoinPlaylist.Count;
            return QuickJoinPlaylist[i];
        }

        /// <summary>Apply current playlist entry to PlayerLoadout without advancing.</summary>
        public static void ApplyCurrentToLoadout()
        {
            var e = Current;
            PlayerLoadout.Map = e.Map;
            PlayerLoadout.Mode = e.Mode;
        }

        /// <summary>
        /// Play / confirm: apply the shown rotation to loadout, then advance index for the next menu visit.
        /// </summary>
        public static Entry ConsumeForPlay()
        {
            var played = Current;
            ApplyCurrentToLoadout();
            CurrentIndex = (CurrentIndex + 1) % QuickJoinPlaylist.Count;
            PlayerPrefs.Save();
            return played;
        }

        public static string FormatCurrentLabel()
        {
            var e = Current;
            return $"Now playing: {e.DisplayName}";
        }

        public static string ModeLabel(ModeIds mode)
        {
            switch (mode)
            {
                case ModeIds.CaptureTheFlag: return "Pig Flag CTF";
                case ModeIds.ZoneControl: return "Zone Control";
                default: return "Free for All";
            }
        }

        public static string MapLabel(MapIds map)
        {
            switch (map)
            {
                case MapIds.WowMart: return "WowMart";
                case MapIds.MethLab: return "Meth Lab";
                default: return "The Holler";
            }
        }
    }
}
