using WarHoller.Characters;

namespace WarHoller.Core
{
    /// <summary>
    /// Persists menu selections into the match bootstrap.
    /// </summary>
    public static class PlayerLoadout
    {
        public static string CharacterId = "hatfield_jed";
        public static WeaponIds Weapon = WeaponIds.LeverActionRifle;
        public static MapIds Map = MapIds.TheHoller;
        public static ModeIds Mode = ModeIds.FreeForAll;

        /// <summary>FFA lobby size (player + bots).</summary>
        public const int TargetFfaParticipants = 12;

        /// <summary>Team modes: 6 per side including the human.</summary>
        public const int TeamSize = 6;

        public static CharacterDef SelectedCharacter => CharacterRoster.Get(CharacterId);

        public static bool IsTeamMode =>
            Mode == ModeIds.CaptureTheFlag || Mode == ModeIds.ZoneControl;

        public static void ResetDefaults()
        {
            CharacterId = CharacterRoster.DefaultHatfield.Id;
            Weapon = WeaponIds.LeverActionRifle;
            Map = MapIds.TheHoller;
            Mode = ModeIds.FreeForAll;
        }
    }
}
