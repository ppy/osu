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
    public abstract class CaughtObject : SkinnableDrawable, IHasCatchObjectState
    {
        public CaughtObjectEntry Entry { get; private set; }

        public CatchObjectType ObjectType => Entry.ObjectType;
        public double StartTime => Entry.StartTime;
        public Bindable<Color4> AccentColour => Entry.AccentColour;
        public Bindable<bool> HyperDash => Entry.HyperDash;
        public Vector2 DisplaySize => Entry.DisplaySize;
        public float DisplayRotation => Entry.DisplayRotation;

        public override bool RemoveCompletedTransforms => false;

        protected CaughtObject(CatchSkinComponents skinComponent, Func<ISkinComponent, Drawable> defaultImplementation)
            : base(new CatchSkinComponent(skinComponent), defaultImplementation)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.None;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
        }

        public void Apply(CaughtObjectEntry entry)
        {
            Entry = entry;

            Position = entry.PositionInStack;
            Scale = Vector2.Divide(Entry.DisplaySize, Size) * 0.5f;
            Rotation = Entry.DisplayRotation;
        }

        protected override void FreeAfterUse()
        {
            ClearTransforms();
            Alpha = 1;

            base.FreeAfterUse();
        }
    }
}
