// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    [Cached(typeof(IHasCatchObjectState))]
    public abstract partial class DrawablePalpableCatchHitObject : DrawableCatchHitObject, IHasCatchObjectState
    {
        public new PalpableCatchHitObject HitObject => (PalpableCatchHitObject)base.HitObject;

        public double DisplayStartTime => LifetimeStart;

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

        public Vector2 DisplayPosition => DrawPosition;

        public Vector2 DisplaySize => ScalingContainer.Size * ScalingContainer.Scale;

        public float DisplayRotation => ScalingContainer.Rotation;

        protected DrawablePalpableCatchHitObject(CatchHitObject? h)
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
            // same as `CatchHitObject.EffectiveX`.
            // not using that property directly to support scenarios where `HitObject` may not necessarily be present
            // for this pooled drawable.
            X = Math.Clamp(OriginalXBindable.Value + XOffsetBindable.Value, 0, CatchPlayfield.WIDTH);
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

        public void RestoreState(CatchObjectState state) => throw new NotSupportedException("Cannot restore state into a drawable catch hitobject.");
    }
}
