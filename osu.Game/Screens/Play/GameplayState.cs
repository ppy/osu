// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// The state of an active gameplay session, generally constructed and exposed by <see cref="Player"/>.
    /// </summary>
    public class GameplayState
    {
        /// <summary>
        /// The final post-convert post-mod-application beatmap.
        /// </summary>
        public readonly IBeatmap Beatmap;

        /// <summary>
        /// The ruleset used in gameplay.
        /// </summary>
        public readonly Ruleset Ruleset;

        /// <summary>
        /// The mods applied to the gameplay.
        /// </summary>
        public readonly IReadOnlyList<Mod> Mods;

        /// <summary>
        /// The gameplay score.
        /// </summary>
        public readonly Score Score;

        public readonly ScoreProcessor ScoreProcessor;

        /// <summary>
        /// Whether gameplay completed without the user failing.
        /// </summary>
        public bool HasPassed { get; set; }

        /// <summary>
        /// Whether the user failed during gameplay and the fail animation has been displayed.
        /// </summary>
        /// <remarks>
        /// In multiplayer, this is never set to <c>true</c> even if the player reached zero health, due to <see cref="PlayerConfiguration.AllowFailAnimation"/> being turned off.
        /// </remarks>
        public bool ShownFailAnimation { get; set; }

        /// <summary>
        /// Whether the user quit gameplay without having either passed or failed.
        /// </summary>
        public bool HasQuit { get; set; }

        /// <summary>
        /// A bindable tracking the last judgement result applied to any hit object.
        /// </summary>
        public IBindable<JudgementResult> LastJudgementResult => lastJudgementResult;

        private readonly Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

        public GameplayState(IBeatmap beatmap, Ruleset ruleset, IReadOnlyList<Mod>? mods = null, Score? score = null, ScoreProcessor? scoreProcessor = null)
        {
            Beatmap = beatmap;
            Ruleset = ruleset;
            Score = score ?? new Score
            {
                ScoreInfo =
                {
                    BeatmapInfo = beatmap.BeatmapInfo,
                    Ruleset = ruleset.RulesetInfo
                }
            };
            Mods = mods ?? Array.Empty<Mod>();
            ScoreProcessor = scoreProcessor ?? ruleset.CreateScoreProcessor();
        }

        /// <summary>
        /// Applies the score change of a <see cref="JudgementResult"/> to this <see cref="GameplayState"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to apply.</param>
        public void ApplyResult(JudgementResult result) => lastJudgementResult.Value = result;
    }
}
