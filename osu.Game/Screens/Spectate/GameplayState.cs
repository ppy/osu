// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Spectate
{
    /// <summary>
    /// The gameplay state of a spectated user. This class is immutable.
    /// </summary>
    public class GameplayState
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

        public GameplayState(Score score, Ruleset ruleset, WorkingBeatmap beatmap)
        {
            Score = score;
            Ruleset = ruleset;
            Beatmap = beatmap;
        }
    }
}
