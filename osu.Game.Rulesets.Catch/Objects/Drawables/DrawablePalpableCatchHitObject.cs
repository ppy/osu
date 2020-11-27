// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public abstract class DrawablePalpableCatchHitObject : DrawableCatchHitObject
    {
        public new PalpableCatchHitObject HitObject => (PalpableCatchHitObject)base.HitObject;

        public Bindable<bool> HyperDash { get; } = new Bindable<bool>();

        public Bindable<float> ScaleBindable { get; } = new Bindable<float>(1);

        /// <summary>
        /// The multiplicative factor applied to <see cref="ScaleContainer"/> scale relative to <see cref="HitObject"/> scale.
        /// </summary>
        protected virtual float ScaleFactor => 1;

        /// <summary>
        /// Whether this hit object should stay on the catcher plate when the object is caught by the catcher.
        /// </summary>
        public virtual bool StaysOnPlate => true;

        protected readonly Container ScaleContainer;

        protected DrawablePalpableCatchHitObject(CatchHitObject h)
            : base(h)
        {
            Origin = Anchor.Centre;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);

            AddInternal(ScaleContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            XBindable.BindValueChanged(x =>
            {
                if (!IsOnPlate) X = x.NewValue;
            }, true);

            ScaleBindable.BindValueChanged(scale =>
            {
                ScaleContainer.Scale = new Vector2(scale.NewValue * ScaleFactor);
            }, true);
        }

        protected override void OnApply()
        {
            base.OnApply();

            HyperDash.BindTo(HitObject.HyperDashBindable);
            ScaleBindable.BindTo(HitObject.ScaleBindable);
        }

        protected override void OnFree()
        {
            HyperDash.UnbindFrom(HitObject.HyperDashBindable);
            ScaleBindable.UnbindFrom(HitObject.ScaleBindable);

            base.OnFree();
        }
    }
}
