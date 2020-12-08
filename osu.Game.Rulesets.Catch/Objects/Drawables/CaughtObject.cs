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
    [Cached(typeof(IHasCatchObjectState))]
    public abstract class CaughtObject : SkinnableDrawable, IHasCatchObjectState
    {
        public PalpableCatchHitObject HitObject { get; private set; }
        public Bindable<Color4> AccentColour { get; } = new Bindable<Color4>();
        public Bindable<bool> HyperDash { get; } = new Bindable<bool>();

        /// <summary>
        /// Whether this hit object should stay on the catcher plate when the object is caught by the catcher.
        /// </summary>
        public virtual bool StaysOnPlate => true;

        public override bool RemoveWhenNotAlive => true;

        protected CaughtObject(CatchSkinComponents skinComponent, Func<ISkinComponent, Drawable> defaultImplementation)
            : base(new CatchSkinComponent(skinComponent), defaultImplementation)
        {
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.None;
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
        }

        public virtual void CopyFrom(IHasCatchObjectState objectState)
        {
            HitObject = objectState.HitObject;
            Scale = objectState.Scale;
            Rotation = objectState.Rotation;
            AccentColour.Value = objectState.AccentColour.Value;
            HyperDash.Value = objectState.HyperDash.Value;
        }

        protected override void FreeAfterUse()
        {
            ClearTransforms();

            Alpha = 1;
            LifetimeStart = double.MinValue;
            LifetimeEnd = double.MaxValue;

            base.FreeAfterUse();
        }
    }

    public class CaughtFruit : CaughtObject, IHasFruitState
    {
        public Bindable<FruitVisualRepresentation> VisualRepresentation { get; } = new Bindable<FruitVisualRepresentation>();

        public CaughtFruit()
            : base(CatchSkinComponents.Fruit, _ => new FruitPiece())
        {
        }

        public override void CopyFrom(IHasCatchObjectState objectState)
        {
            base.CopyFrom(objectState);

            var fruitState = (IHasFruitState)objectState;
            VisualRepresentation.Value = fruitState.VisualRepresentation.Value;
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
