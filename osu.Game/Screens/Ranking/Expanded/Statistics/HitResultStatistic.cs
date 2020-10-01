// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class HitResultStatistic : CounterStatistic
    {
        private readonly HitResult result;

        public HitResultStatistic(HitResult result, int count, int? maxCount = null)
            : base(result.GetDescription(), count, maxCount)
        {
            this.result = result;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HeaderText.Colour = colours.ForHitResult(result);
        }
    }
}
