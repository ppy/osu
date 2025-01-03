// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// A hit used specifically for drum rolls, where spawning flying hits is required.
    /// </summary>
    public partial class DrawableFlyingHit : DrawableHit
    {
        public DrawableFlyingHit(DrawableDrumRollTick drumRollTick)
            : base(new IgnoreHit
            {
                StartTime = drumRollTick.HitObject.StartTime + drumRollTick.Result.TimeOffset,
                IsStrong = drumRollTick.HitObject.IsStrong,
                Type = drumRollTick.JudgementType
            })
        {
            HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ApplyMaxResult();
            Size = new osuTK.Vector2(0.3f);
        }

        private static float degree = 0f;
        protected override void PrepareForUse()
        {
            const float single_rotation_degree = 7f;

            base.PrepareForUse();
            degree = (degree + single_rotation_degree) % 360f;
            Rotation = degree;
        }

        protected override void LoadSamples()
        {
            // block base call - flying hits are not supposed to play samples
            // the base call could overwrite the type of this hit
        }

        // TODO: which skin use?
        protected override SkinnableDrawable? OnLoadCreateMainPiece()
            => new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Swell), _ => new SwellCirclePiece(), confineMode: ConfineMode.ScaleToFit);
    }
}
