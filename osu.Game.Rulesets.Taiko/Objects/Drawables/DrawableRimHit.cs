// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableRimHit : DrawableHit
    {
        public override TaikoAction[] HitActions { get; } = { TaikoAction.LeftRim, TaikoAction.RightRim };

        public DrawableRimHit(Hit hit)
            : base(hit)
        {
            MainPiece.Add(new RimHitSymbolPiece());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            MainPiece.AccentColour = colours.BlueDarker;
        }
    }

    public class DrawableFlyingRimHit : DrawableRimHit
    {
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            ApplyResult(r => r.Type = HitResult.Good);
        }

        public DrawableFlyingRimHit(double time, bool isStrong = false)
            : base(new Hit { StartTime = time, IsStrong = isStrong })
        {
            HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        }
    }
}
