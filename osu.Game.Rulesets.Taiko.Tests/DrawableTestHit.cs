// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    internal class DrawableTestHit : DrawableTaikoHitObject
    {
        private readonly HitResult type;

        public DrawableTestHit(Hit hit, HitResult type = HitResult.Great)
            : base(hit)
        {
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Result.Type = type;
        }

        public override bool OnPressed(TaikoAction action) => false;
    }
}
