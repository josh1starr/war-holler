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
        public const int TargetFfaParticipants = 12;

        public static CharacterDef SelectedCharacter => CharacterRoster.Get(CharacterId);

        public static void ResetDefaults()
        {
            CharacterId = CharacterRoster.DefaultHatfield.Id;
            Weapon = WeaponIds.LeverActionRifle;
            Map = MapIds.TheHoller;
            Mode = ModeIds.FreeForAll;
        }
    }
}
