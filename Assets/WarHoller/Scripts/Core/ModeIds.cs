namespace WarHoller.Core
{
    /// <summary>
    /// Match modes. FreeForAll is playable in this vertical slice.
    /// CaptureTheFlag (pig flag) and ZoneControl are scaffolded for future work.
    /// </summary>
    public enum ModeIds
    {
        FreeForAll = 0,

        // EXTENSION: pig-flag CTF — see LobbyRotation / GameSession stubs.
        CaptureTheFlag = 1,

        // EXTENSION: zone/hill control loop — see LobbyRotation / GameSession stubs.
        ZoneControl = 2
    }
}
