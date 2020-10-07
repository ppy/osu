// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class HitResultStatistic : CounterStatistic
    {
        private readonly HitResult result;

        public HitResultStatistic(HitResultDisplayStatistic result)
            : base(result.DisplayName, result.Count, result.MaxCount)
        {
            this.result = result.Result;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HeaderText.Colour = colours.ForHitResult(result);
        }
    }
}
