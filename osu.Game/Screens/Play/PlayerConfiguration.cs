// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Play
{
    public class PlayerConfiguration
    {
        /// <summary>
        /// Whether pausing should be allowed. If not allowed, attempting to pause will quit.
        /// </summary>
        public bool AllowPause { get; set; } = true;

        /// <summary>
        /// Whether results screen should be pushed on completion.
        /// </summary>
        public bool ShowResults { get; set; } = true;

        /// <summary>
        /// Whether the player should be allowed to trigger a restart.
        /// </summary>
        public bool AllowRestart { get; set; } = true;

        /// <summary>
        /// Whether the player should be allowed to skip the intro, advancing to the start of gameplay.
        /// </summary>
        public bool AllowSkippingIntro { get; set; } = true;
    }
}
