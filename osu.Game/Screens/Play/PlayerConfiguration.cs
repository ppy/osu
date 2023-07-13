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
        /// Whether the player should be able to interact with this player instance.
        /// </summary>
        public bool AllowUserInteraction { get; set; } = true;

        /// <summary>
        /// Whether the player should be allowed to skip intros/outros, advancing to the start of gameplay or the end of a storyboard.
        /// </summary>
        public bool AllowSkipping { get; set; } = true;

        /// <summary>
        /// Whether the intro should be skipped by default.
        /// </summary>
        public bool AutomaticallySkipIntro { get; set; }

        /// <summary>
        /// Whether the gameplay leaderboard should always be shown (usually in a contracted state).
        /// </summary>
        public bool AlwaysShowLeaderboard { get; set; }
    }
}
