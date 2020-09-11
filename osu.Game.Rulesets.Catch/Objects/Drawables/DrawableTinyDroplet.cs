// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableTinyDroplet : DrawableDroplet
    {
        protected override HitResult HitResult => HitResult.SmallBonusHit;

        protected override HitResult MissResult => HitResult.SmallBonusMiss;

        public DrawableTinyDroplet(TinyDroplet h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScaleContainer.Scale /= 2;
        }
    }
}
