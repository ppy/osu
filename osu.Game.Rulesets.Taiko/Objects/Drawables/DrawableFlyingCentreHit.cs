// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableFlyingCentreHit : DrawableCentreHit
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();
            ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }

        public DrawableFlyingCentreHit(double time, bool isStrong = false)
            : base(new IgnoreHit { StartTime = time, IsStrong = isStrong })
        {
            HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        }
    }
}
