// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// Contains contents of the catcher plate.
    /// The sprite of the plate is drawn by <see cref="SkinnableCatcher"/>.
    /// </summary>
    public class CatcherPlate : CompositeDrawable
    {
        public Bindable<bool> GenerateHitLighting = new Bindable<bool>(true);

        public Bindable<bool> StackCaughtObject = new Bindable<bool>(true);

        private readonly DrawablePool<CaughtFruit> caughtFruitPool;
        private readonly DrawablePool<CaughtBanana> caughtBananaPool;
        private readonly DrawablePool<CaughtDroplet> caughtDropletPool;

        /// <summary>
        /// Caught objects stacked on the plate.
        /// </summary>
        private readonly Container<CaughtObject> caughtObjectContainer;

        /// <summary>
        /// Contains <see cref="HitExplosion"/>s (aka. hit lighting).
        /// </summary>
        private readonly HitExplosionContainer hitExplosionContainer;

        /// <summary>
        /// Objects dropped from the plate.
        /// </summary>
        private readonly Container<CaughtObject> droppedObjectTarget;

        /// <summary>
        /// The amount by which caught fruit should be scaled down to fit on the plate.
        /// </summary>
        private const float caught_fruit_scale_adjust = 0.5f;

        public CatcherPlate(Container<CaughtObject> droppedObjectTarget)
        {
            this.droppedObjectTarget = droppedObjectTarget;

            Origin = Anchor.BottomCentre;

            InternalChildren = new Drawable[]
            {
                caughtFruitPool = new DrawablePool<CaughtFruit>(50),
                caughtBananaPool = new DrawablePool<CaughtBanana>(100),
                // less capacity is needed compared to fruit because droplet is not stacked
                caughtDropletPool = new DrawablePool<CaughtDroplet>(25),
                caughtObjectContainer = new Container<CaughtObject>
                {
                    // offset fruit vertically to better place "above" the plate.
                    Y = -5
                },
                hitExplosionContainer = new HitExplosionContainer()
            };
        }

        public Drawable CreateBackgroundLayerProxy() => caughtObjectContainer.CreateProxy();

        public void OnHitObjectCaught(DrawablePalpableCatchHitObject drawableHitObject, float catcherPosition)
        {
            var positionInStack = computePositionInStack(new Vector2(drawableHitObject.X - catcherPosition, 0), drawableHitObject.DisplaySize.X);

            if (StackCaughtObject.Value)
                placeCaughtObject(drawableHitObject, positionInStack);

            if (GenerateHitLighting.Value)
                addHitLighting(drawableHitObject.HitObject, positionInStack.X, drawableHitObject.AccentColour.Value);
        }

        public void OnRevertResult(DrawableCatchHitObject drawableHitObject)
        {
            caughtObjectContainer.RemoveAll(d => d.HitObject == drawableHitObject.HitObject);
            droppedObjectTarget.RemoveAll(d => d.HitObject == drawableHitObject.HitObject);
        }

        #region Caught object stacking

        private Vector2 computePositionInStack(Vector2 position, float displayRadius)
        {
            // this is taken from osu-stable (lenience should be 10 * 10 at standard scale).
            const float lenience_adjust = 10 / CatchHitObject.OBJECT_RADIUS;

            float adjustedRadius = displayRadius * lenience_adjust;
            float checkDistance = MathF.Pow(adjustedRadius, 2);

            while (caughtObjectContainer.Any(f => Vector2Extensions.DistanceSquared(f.Position, position) < checkDistance))
            {
                position.X += RNG.NextSingle(-adjustedRadius, adjustedRadius);
                position.Y -= RNG.NextSingle(0, 5);
            }

            return position;
        }

        private void placeCaughtObject(DrawablePalpableCatchHitObject drawableObject, Vector2 position)
        {
            var caughtObject = getCaughtObject(drawableObject.HitObject);

            if (caughtObject == null) return;

            caughtObject.CopyStateFrom(drawableObject);
            caughtObject.Anchor = Anchor.TopCentre;
            caughtObject.Position = position;
            caughtObject.Scale *= caught_fruit_scale_adjust;

            caughtObjectContainer.Add(caughtObject);

            if (!caughtObject.StaysOnPlate)
                removeFromPlate(caughtObject, DropAnimation.Explode);
        }

        private CaughtObject getCaughtObject(PalpableCatchHitObject source)
        {
            switch (source)
            {
                case Fruit _:
                    return caughtFruitPool.Get();

                case Banana _:
                    return caughtBananaPool.Get();

                case Droplet _:
                    return caughtDropletPool.Get();

                default:
                    return null;
            }
        }

        #endregion

        #region Caught object dropping

        public void DropAll(DropAnimation animation)
        {
            var droppedObjects = caughtObjectContainer.Children.Select(getDroppedObject).ToArray();

            caughtObjectContainer.Clear(false);

            droppedObjectTarget.AddRange(droppedObjects);

            foreach (var droppedObject in droppedObjects)
                applyDropAnimation(droppedObject, animation);
        }

        private CaughtObject getDroppedObject(CaughtObject caughtObject)
        {
            var droppedObject = getCaughtObject(caughtObject.HitObject);

            droppedObject.CopyStateFrom(caughtObject);
            droppedObject.Anchor = Anchor.TopLeft;
            droppedObject.Position = caughtObjectContainer.ToSpaceOfOtherDrawable(caughtObject.DrawPosition, droppedObjectTarget);

            return droppedObject;
        }

        private void removeFromPlate(CaughtObject caughtObject, DropAnimation animation)
        {
            var droppedObject = getDroppedObject(caughtObject);

            caughtObjectContainer.Remove(caughtObject);

            droppedObjectTarget.Add(droppedObject);

            applyDropAnimation(droppedObject, animation);
        }

        private void applyDropAnimation(Drawable d, DropAnimation animation)
        {
            switch (animation)
            {
                case DropAnimation.Drop:
                    d.MoveToY(d.Y + 75, 750, Easing.InSine);
                    d.FadeOut(750);
                    break;

                case DropAnimation.Explode:
                    var originalX = droppedObjectTarget.ToSpaceOfOtherDrawable(d.DrawPosition, caughtObjectContainer).X * Scale.X;
                    d.MoveToY(d.Y - 50, 250, Easing.OutSine).Then().MoveToY(d.Y + 50, 500, Easing.InSine);
                    d.MoveToX(d.X + originalX * 6, 1000);
                    d.FadeOut(750);
                    break;
            }

            d.Expire();
        }

        #endregion

        #region Hit lighting

        private void addHitLighting(CatchHitObject hitObject, float x, Color4 colour) =>
            hitExplosionContainer.Add(new HitExplosionEntry(Time.Current, x, hitObject.Scale, colour, hitObject.RandomSeed));

        #endregion
    }
}
