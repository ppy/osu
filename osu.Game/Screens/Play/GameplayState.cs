// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;

#nullable enable

namespace osu.Game.Screens.Play
{
    public class GameplayState
    {
        /// <summary>
        /// The final post-convert post-mod-application beatmap.
        /// </summary>
        public readonly IBeatmap Beatmap;

        public readonly Ruleset Ruleset;

        public IReadOnlyList<Mod> Mods;

        public GameplayState(IBeatmap beatmap, Ruleset ruleset, IReadOnlyList<Mod> mods)
        {
            Beatmap = beatmap;
            Ruleset = ruleset;
            Mods = mods;
        }

        private readonly Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

        public IBindable<JudgementResult> LastJudgementResult => lastJudgementResult;

        public void ApplyResult(JudgementResult result) => lastJudgementResult.Value = result;
    }
}
