// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class DrawableTestHit : DrawableHit
    {
        public readonly HitResult Type;

        public DrawableTestHit(Hit hit, HitResult type = HitResult.Great)
            : base(hit)
        {
            Type = type;

            HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        }

        protected override void UpdateInitialTransforms()
        {
            // base implementation in DrawableHitObject forces alpha to 1.
            // suppress locally to allow hiding the visuals wherever necessary.
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Result.Type = Type;
        }

        public override bool OnPressed(TaikoAction action) => false;
    }
}
