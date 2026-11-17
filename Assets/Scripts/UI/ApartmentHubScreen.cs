using System;

namespace SundayLastBreath.UI
{
    /// <summary>
    /// Backward-compatibility wrapper for GameStageScreen.
    /// ApartmentHubScreen has been renamed to GameStageScreen to reflect its role as the universal gameplay stage.
    /// </summary>
    [Obsolete("ApartmentHubScreen has been renamed to GameStageScreen. Use GameStageScreen instead.")]
    public class ApartmentHubScreen : GameStageScreen
    {
        public static new GameStageScreen Instance => GameStageScreen.Instance;
    }
}
