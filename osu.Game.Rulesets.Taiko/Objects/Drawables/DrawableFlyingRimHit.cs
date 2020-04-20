// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableFlyingRimHit : DrawableRimHit
    {
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            ApplyResult(r => r.Type = HitResult.Good);
        }

        public DrawableFlyingRimHit(double time, bool isStrong = false)
            : base(new IgnoreHit { StartTime = time, IsStrong = isStrong })
        {
            HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        }
    }
}
