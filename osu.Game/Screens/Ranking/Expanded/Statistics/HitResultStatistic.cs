// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public partial class HitResultStatistic : CounterStatistic
    {
        public readonly HitResult Result;

        public HitResultStatistic(HitResultDisplayStatistic result)
            : base(result.DisplayName, result.Count, result.MaxCount)
        {
            Result = result.Result;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HeaderText.Colour = colours.ForHitResult(Result);
        }
    }
}
