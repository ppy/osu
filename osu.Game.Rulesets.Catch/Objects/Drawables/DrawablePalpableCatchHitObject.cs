// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    [Cached(typeof(IHasCatchObjectState))]
    public abstract class DrawablePalpableCatchHitObject : DrawableCatchHitObject, IHasCatchObjectState
    {
        public new PalpableCatchHitObject HitObject => (PalpableCatchHitObject)base.HitObject;

        Bindable<Color4> IHasCatchObjectState.AccentColour => AccentColour;

        public Bindable<bool> HyperDash { get; } = new Bindable<bool>();

        public Bindable<float> ScaleBindable { get; } = new Bindable<float>(1);

        public Bindable<int> IndexInBeatmap { get; } = new Bindable<int>();

        /// <summary>
        /// The multiplicative factor applied to <see cref="Drawable.Scale"/> relative to <see cref="HitObject"/> scale.
        /// </summary>
        protected virtual float ScaleFactor => 1;

        public float DisplayRadius => CatchHitObject.OBJECT_RADIUS * HitObject.Scale * ScaleFactor;

        protected DrawablePalpableCatchHitObject([CanBeNull] CatchHitObject h)
            : base(h)
        {
            Origin = Anchor.Centre;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            XBindable.BindValueChanged(x =>
            {
                X = x.NewValue;
            }, true);

            ScaleBindable.BindValueChanged(scale =>
            {
                Scale = new Vector2(scale.NewValue * ScaleFactor);
            }, true);

            IndexInBeatmap.BindValueChanged(_ => UpdateComboColour());
        }

        protected override void OnApply()
        {
            base.OnApply();

            HyperDash.BindTo(HitObject.HyperDashBindable);
            ScaleBindable.BindTo(HitObject.ScaleBindable);
            IndexInBeatmap.BindTo(HitObject.IndexInBeatmapBindable);
        }

        protected override void OnFree()
        {
            HyperDash.UnbindFrom(HitObject.HyperDashBindable);
            ScaleBindable.UnbindFrom(HitObject.ScaleBindable);
            IndexInBeatmap.UnbindFrom(HitObject.IndexInBeatmapBindable);

            base.OnFree();
        }
    }
}
