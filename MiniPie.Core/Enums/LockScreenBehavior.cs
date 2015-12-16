namespace MiniPie.Core.Enums
{
    public enum LockScreenBehavior
    {
        /// <summary>
        /// No action on screen locked
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// Pause when screen is locked
        /// </summary>
        Pause = 1,
        /// <summary>
        /// Pause when screen is locked. Unpause when screen is locked automatically, but not if previously paused manually
        /// </summary>
        PauseUnpause = 2,
        /// <summary>
        /// Pause when screen is locked. Always unpause when screen unlocked
        /// </summary>
        PauseUnpauseAlways = 3
    }
}