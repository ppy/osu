// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public abstract partial class DrawableOsuHitObject : DrawableHitObject<OsuHitObject>
    {
        public readonly IBindable<Vector2> PositionBindable = new Bindable<Vector2>();
        public readonly IBindable<int> StackHeightBindable = new Bindable<int>();
        public readonly IBindable<float> ScaleBindable = new BindableFloat();
        public readonly IBindable<int> IndexInCurrentComboBindable = new Bindable<int>();

        // Must be set to update IsHovered as it's used in relax mod to detect osu hit objects.
        public override bool HandlePositionalInput => true;

        protected override float SamplePlaybackPosition => CalculateDrawableRelativePosition(this);

        /// <summary>
        /// What action this <see cref="DrawableOsuHitObject"/> should take in response to a
        /// click at the given time value.
        /// If non-null, judgements will be ignored for return values of <see cref="ClickAction.Ignore"/>
        /// and <see cref="ClickAction.Shake"/>, and this hit object will be shaken for return values of
        /// <see cref="ClickAction.Shake"/>.
        /// </summary>
        public Func<DrawableHitObject, double, HitResult, ClickAction> CheckHittable;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Alpha = 0;
        }

        protected override void OnApply()
        {
            base.OnApply();

            IndexInCurrentComboBindable.BindTo(HitObject.IndexInCurrentComboBindable);
            PositionBindable.BindTo(HitObject.PositionBindable);
            StackHeightBindable.BindTo(HitObject.StackHeightBindable);
            ScaleBindable.BindTo(HitObject.ScaleBindable);
        }

        protected override void OnFree()
        {
            base.OnFree();

            IndexInCurrentComboBindable.UnbindFrom(HitObject.IndexInCurrentComboBindable);
            PositionBindable.UnbindFrom(HitObject.PositionBindable);
            StackHeightBindable.UnbindFrom(HitObject.StackHeightBindable);
            ScaleBindable.UnbindFrom(HitObject.ScaleBindable);
        }

        protected virtual IEnumerable<Drawable> DimmablePieces => Enumerable.Empty<Drawable>();

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            foreach (var piece in DimmablePieces)
            {
                piece.FadeColour(new Color4(195, 195, 195, 255));
                using (piece.BeginDelayedSequence(InitialLifetimeOffset - OsuHitWindows.MISS_WINDOW))
                    piece.FadeColour(Color4.White, 100);
            }
        }

        protected sealed override double InitialLifetimeOffset => HitObject.TimePreempt;

        private OsuInputManager osuActionInputManager;
        internal OsuInputManager OsuActionInputManager => osuActionInputManager ??= GetContainingInputManager() as OsuInputManager;

        /// <summary>
        /// Shake the hit object in case it was clicked far too early or late (aka "note lock").
        /// </summary>
        public virtual void Shake() { }

        /// <summary>
        /// Causes this <see cref="DrawableOsuHitObject"/> to get hit, disregarding all conditions in implementations of <see cref="DrawableHitObject.CheckForResult"/>.
        /// </summary>
        public void HitForcefully() => ApplyResult(r => r.Type = r.JudgementCriteria.MaxResult);

        /// <summary>
        /// Causes this <see cref="DrawableOsuHitObject"/> to get missed, disregarding all conditions in implementations of <see cref="DrawableHitObject.CheckForResult"/>.
        /// </summary>
        public void MissForcefully() => ApplyResult(r => r.Type = r.JudgementCriteria.MinResult);

        private RectangleF parentScreenSpaceRectangle => ((DrawableOsuHitObject)ParentHitObject)?.parentScreenSpaceRectangle ?? Parent!.ScreenSpaceDrawQuad.AABBFloat;

        /// <summary>
        /// Calculates the position of the given <paramref name="drawable"/> relative to the playfield area.
        /// </summary>
        /// <param name="drawable">The drawable to calculate its relative position.</param>
        protected float CalculateDrawableRelativePosition(Drawable drawable) => (drawable.ScreenSpaceDrawQuad.Centre.X - parentScreenSpaceRectangle.X) / parentScreenSpaceRectangle.Width;

        protected override Judgement CreateResult(JudgementCriteria judgementCriteria) => new OsuJudgement(HitObject, judgementCriteria);
    }
}
