// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject>
    {
        public readonly IBindable<Vector2> PositionBindable = new Bindable<Vector2>();
        public readonly IBindable<int> StackHeightBindable = new Bindable<int>();
        public readonly IBindable<float> ScaleBindable = new BindableFloat();
        public readonly IBindable<int> IndexInCurrentComboBindable = new Bindable<int>();

        // Must be set to update IsHovered as it's used in relax mdo to detect osu hit objects.
        public override bool HandlePositionalInput => true;

        protected override float SamplePlaybackPosition => HitObject.X / OsuPlayfield.BASE_SIZE.X;

        /// <summary>
        /// Whether this <see cref="DrawableOsuHitObject"/> can be hit, given a time value.
        /// If non-null, judgements will be ignored (resulting in a shake) whilst the function returns false.
        /// </summary>
        public Func<DrawableHitObject, double, bool> CheckHittable;

        private ShakeContainer shakeContainer;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Alpha = 0;

            base.AddInternal(shakeContainer = new ShakeContainer
            {
                ShakeDuration = 30,
                RelativeSizeAxes = Axes.Both
            });
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

        // Forward all internal management to shakeContainer.
        // This is a bit ugly but we don't have the concept of InternalContent so it'll have to do for now. (https://github.com/ppy/osu-framework/issues/1690)
        protected override void AddInternal(Drawable drawable) => shakeContainer.Add(drawable);
        protected override void ClearInternal(bool disposeChildren = true) => shakeContainer.Clear(disposeChildren);
        protected override bool RemoveInternal(Drawable drawable) => shakeContainer.Remove(drawable);

        protected sealed override double InitialLifetimeOffset => HitObject.TimePreempt;

        private OsuInputManager osuActionInputManager;
        internal OsuInputManager OsuActionInputManager => osuActionInputManager ??= GetContainingInputManager() as OsuInputManager;

        public virtual void Shake(double maximumLength) => shakeContainer.Shake(maximumLength);

        /// <summary>
        /// Causes this <see cref="DrawableOsuHitObject"/> to get missed, disregarding all conditions in implementations of <see cref="DrawableHitObject.CheckForResult"/>.
        /// </summary>
        public void MissForcefully() => ApplyResult(r => r.Type = r.Judgement.MinResult);

        protected override JudgementResult CreateResult(Judgement judgement) => new OsuJudgementResult(HitObject, judgement);
    }
}
