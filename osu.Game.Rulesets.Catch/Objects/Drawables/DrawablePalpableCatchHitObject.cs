// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        /// <summary>
        /// The container internal transforms (such as scaling based on the circle size) are applied to.
        /// </summary>
        protected readonly Container ScalingContainer;

        public Vector2 DisplaySize => ScalingContainer.Size * ScalingContainer.Scale;

        public float DisplayRotation => ScalingContainer.Rotation;

        protected DrawablePalpableCatchHitObject([CanBeNull] CatchHitObject h)
            : base(h)
        {
            Origin = Anchor.Centre;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);

            AddInternal(ScalingContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2)
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            OriginalXBindable.BindValueChanged(updateXPosition);
            XOffsetBindable.BindValueChanged(updateXPosition, true);

            ScaleBindable.BindValueChanged(scale =>
            {
                ScalingContainer.Scale = new Vector2(scale.NewValue * ScaleFactor);
                Size = DisplaySize;
            }, true);

            IndexInBeatmap.BindValueChanged(_ => UpdateComboColour());
        }

        private void updateXPosition(ValueChangedEvent<float> _)
        {
            X = OriginalXBindable.Value + XOffsetBindable.Value;
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
