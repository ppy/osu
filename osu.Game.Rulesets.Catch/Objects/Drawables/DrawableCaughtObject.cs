// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    /// <summary>
    /// Represents a <see cref="PalpableCatchHitObject"/> caught by the catcher.
    /// </summary>
    [Cached(typeof(IHasCatchObjectState))]
    public abstract class DrawableCaughtObject : SkinnableDrawable, IHasCatchObjectState
    {
        public CatchHitObject HitObject { get; private set; }
        public Bindable<Color4> AccentColour { get; } = new Bindable<Color4>();
        public Bindable<bool> HyperDash { get; } = new Bindable<bool>();
        public Vector2 DisplaySize => Size * Scale;
        public float DisplayRotation => Rotation;

        public override bool RemoveCompletedTransforms => false;

        protected DrawableCaughtObject(CatchSkinComponents skinComponent, Func<ISkinComponent, Drawable> defaultImplementation)
            : base(new CatchSkinComponent(skinComponent), defaultImplementation)
        {
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.None;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
        }

        public virtual void Apply(CaughtObjectEntry entry)
        {
            HitObject = entry.HitObject;

            AccentColour.Value = entry.AccentColour;
            HyperDash.Value = entry.HyperDash;

            Scale = Vector2.Divide(entry.DisplaySize, Size) * 0.5f;
            Rotation = entry.DisplayRotation;

            entry.ApplyTransforms(this);
        }

        protected override void FreeAfterUse()
        {
            ClearTransforms();
            Alpha = 1;

            HitObject = null;

            base.FreeAfterUse();
        }
    }
}
