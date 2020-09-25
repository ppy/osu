// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class DrawableTestStrongHit : DrawableHit
    {
        private readonly HitResult type;
        private readonly bool hitBoth;

        public DrawableTestStrongHit(double startTime, HitResult type = HitResult.Great, bool hitBoth = true)
            : base(new Hit
            {
                IsStrong = true,
                StartTime = startTime,
            })
        {
            // in order to create nested strong hit
            HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            this.type = type;
            this.hitBoth = hitBoth;
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();

            Result.Type = type;

            var nestedStrongHit = (DrawableStrongNestedHit)NestedHitObjects.Single();
            nestedStrongHit.Result.Type = hitBoth ? type : HitResult.Miss;
        }

        public override bool OnPressed(TaikoAction action) => false;
    }
}
