// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Spectate
{
    /// <summary>
    /// An immutable spectator gameplay state.
    /// </summary>
    public class SpectatorGameplayState
    {
        /// <summary>
        /// The score which the user is playing.
        /// </summary>
        public readonly Score Score;

        /// <summary>
        /// The ruleset which the user is playing.
        /// </summary>
        public readonly Ruleset Ruleset;

        /// <summary>
        /// The beatmap which the user is playing.
        /// </summary>
        public readonly WorkingBeatmap Beatmap;

        public SpectatorGameplayState(Score score, Ruleset ruleset, WorkingBeatmap beatmap)
        {
            Score = score;
            Ruleset = ruleset;
            Beatmap = beatmap;
        }
    }
}
