// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

#nullable enable

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

        /// <summary>
        /// A bindable tracking the last judgement result applied to any hit object.
        /// </summary>
        public IBindable<JudgementResult> LastJudgementResult => lastJudgementResult;

        private readonly Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

        public GameplayState(IBeatmap beatmap, Ruleset ruleset, IReadOnlyList<Mod>? mods = null, Score? score = null)
        {
            Beatmap = beatmap;
            Ruleset = ruleset;
            Score = score ?? new Score
            {
                ScoreInfo =
                {
                    Ruleset = ruleset.RulesetInfo
                }
            };
            Mods = mods ?? ArraySegment<Mod>.Empty;
        }

        /// <summary>
        /// Applies the score change of a <see cref="JudgementResult"/> to this <see cref="GameplayState"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to apply.</param>
        public void ApplyResult(JudgementResult result) => lastJudgementResult.Value = result;
    }
}
