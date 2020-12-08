// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    [Cached(typeof(CaughtObject))]
    public abstract class CaughtObject : SkinnableDrawable
    {
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();

        public CatchHitObject HitObject { get; private set; }

        /// <summary>
        /// Whether this hit object should stay on the catcher plate when the object is caught by the catcher.
        /// </summary>
        public virtual bool StaysOnPlate => true;

        public override bool RemoveWhenNotAlive => true;

        protected CaughtObject(CatchSkinComponents skinComponent, Func<ISkinComponent, Drawable> defaultImplementation)
            : base(new CatchSkinComponent(skinComponent), defaultImplementation)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.None;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
        }

        public virtual void CopyFrom(DrawablePalpableCatchHitObject drawableObject)
        {
            HitObject = drawableObject.HitObject;
            Scale = drawableObject.Scale / 2;
            Rotation = drawableObject.Rotation;
            AccentColour.Value = drawableObject.AccentColour.Value;
        }
    }

    public class CaughtFruit : CaughtObject
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        public CaughtFruit()
            : base(CatchSkinComponents.Fruit, _ => new FruitPiece())
        {
        }

        public override void CopyFrom(DrawablePalpableCatchHitObject drawableObject)
        {
            base.CopyFrom(drawableObject);

            var drawableFruit = (DrawableFruit)drawableObject;
            VisualRepresentation.Value = drawableFruit.VisualRepresentation.Value;
        }
    }

    public class CaughtBanana : CaughtObject
    {
        public CaughtBanana()
            : base(CatchSkinComponents.Banana, _ => new BananaPiece())
        {
        }
    }

    public class CaughtDroplet : CaughtObject
    {
        public override bool StaysOnPlate => false;

        public CaughtDroplet()
            : base(CatchSkinComponents.Droplet, _ => new DropletPiece())
        {
        }
    }
}
