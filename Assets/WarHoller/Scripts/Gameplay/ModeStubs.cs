using WarHoller.Core;

namespace WarHoller.Gameplay
{
    /// <summary>
    /// Empty stubs for future game modes. Hook from GameSession when ModeIds != FreeForAll.
    /// </summary>
    public static class CaptureTheFlagMode
    {
        // EXTENSION: pig flag pickup, return-to-base, score events.
        public static void BeginStub(GameSession session)
        {
            _ = session;
            _ = ModeIds.CaptureTheFlag;
        }
    }

    public static class ZoneControlMode
    {
        // EXTENSION: rotating zones, tick scoring, contested state.
        public static void BeginStub(GameSession session)
        {
            _ = session;
            _ = ModeIds.ZoneControl;
        }
    }
}
