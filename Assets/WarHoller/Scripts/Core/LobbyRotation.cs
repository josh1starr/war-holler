using System.Collections.Generic;

namespace WarHoller.Core
{
    /// <summary>
    /// Scaffold for future online quick-join map/mode rotation.
    /// Not driven in this vertical slice — FFA on The Holler is hard-wired from Play.
    /// </summary>
    public static class LobbyRotation
    {
        public struct Entry
        {
            public MapIds Map;
            public ModeIds Mode;
            public string DisplayName;
        }

        /// <summary>Future quick-join playlist. Extension point for matchmaking.</summary>
        public static readonly IReadOnlyList<Entry> QuickJoinPlaylist = new List<Entry>
        {
            new Entry { Map = MapIds.TheHoller, Mode = ModeIds.FreeForAll, DisplayName = "The Holler — FFA" },
            // EXTENSION: CTF pig flag on The Holler / WowMart
            new Entry { Map = MapIds.TheHoller, Mode = ModeIds.CaptureTheFlag, DisplayName = "The Holler — Pig Flag (soon)" },
            new Entry { Map = MapIds.WowMart, Mode = ModeIds.FreeForAll, DisplayName = "WowMart — FFA (soon)" },
            new Entry { Map = MapIds.WowMart, Mode = ModeIds.ZoneControl, DisplayName = "WowMart — Zones (soon)" },
            new Entry { Map = MapIds.MethLab, Mode = ModeIds.FreeForAll, DisplayName = "Meth Lab — FFA (soon)" },
            new Entry { Map = MapIds.MethLab, Mode = ModeIds.CaptureTheFlag, DisplayName = "Meth Lab — Pig Flag (soon)" }
        };
    }
}
