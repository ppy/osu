// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Game.Beatmaps;
using osu.Game.Replays;
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
        public readonly ScoreInfo ScoreInfo;

        /// <summary>
        /// The replay of the user play.
        /// </summary>
        public readonly StreamingReplay Replay;

        public Score Score => new Score
        {
            ScoreInfo = ScoreInfo,
            Replay = Replay
        };

        /// <summary>
        /// The ruleset which the user is playing.
        /// </summary>
        public readonly Ruleset Ruleset;

        /// <summary>
        /// The beatmap which the user is playing.
        /// </summary>
        public readonly WorkingBeatmap Beatmap;

        public GameplayState(ScoreInfo scoreInfo, Ruleset ruleset, WorkingBeatmap beatmap)
        {
            ScoreInfo = scoreInfo;
            Replay = new StreamingReplay();
            Ruleset = ruleset;
            Beatmap = beatmap;
        }
    }
}
