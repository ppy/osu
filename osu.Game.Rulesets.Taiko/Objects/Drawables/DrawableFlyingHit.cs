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
            : base(new IgnoreHit(drumRollTick.JudgementType)
            {
                StartTime = drumRollTick.HitObject.StartTime + drumRollTick.Result.TimeOffset,
                IsStrong = drumRollTick.HitObject.IsStrong,
            })
        {
            HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
        }

        private static float degree = 0f;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ApplyMaxResult();
            Size = new osuTK.Vector2(0.3f);
        }

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


        //public override TaikoAction[] HitActions { get; protected set; } = Array.Empty<TaikoAction>();
        public override TaikoAction[] HitActions
        {
            //get => HitObject.Type == HitType.Centre ? [TaikoAction.LeftCentre, TaikoAction.RightCentre] : [TaikoAction.LeftRim, TaikoAction.RightRim];
            get => [TaikoAction.LeftCentre, TaikoAction.RightCentre, TaikoAction.LeftRim, TaikoAction.RightRim];
            protected set { }
        }

        protected override SkinnableDrawable? OnLoadCreateMainPiece()
            //=> null;
            //=> new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.DrumRollTick), _ => new TickPiece(), confineMode: ConfineMode.ScaleToFit);
            //
            // TODO: idk, seems like there need to be added new skin component & in it's Animation should be changed it's collor in depends of `hit / this_drum_roll_hits`
            //     | just like in stable version
            => new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Swell), _ => new SwellCirclePiece(), confineMode: ConfineMode.ScaleToFit);
    }
}
