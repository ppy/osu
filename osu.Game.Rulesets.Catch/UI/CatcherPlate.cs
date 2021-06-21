// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
        /// <summary>
        /// Whether a hit lighting should be generated when a hit object is caught.
        /// </summary>
        public Bindable<bool> GenerateHitLighting = new Bindable<bool>(true);

        /// <summary>
        /// Whether caught objects should be placed on the plate (stacked on the plate or immediately dropped).
        /// </summary>
        public Bindable<bool> PlaceCaughtObject = new Bindable<bool>(true);

        private CaughtObjectPool caughtObjectPool;

        /// <summary>
        /// The destination of objects dropped from the plate.
        /// </summary>
        [Resolved]
        private DroppedObjectContainer droppedObjectContainer { get; set; }

        /// <summary>
        /// Caught objects stacked on the plate.
        /// </summary>
        private readonly Container<CaughtObject> caughtObjectContainer;

        /// <summary>
        /// Contains <see cref="HitExplosion"/>s (aka. hit lighting).
        /// </summary>
        private readonly HitExplosionContainer hitExplosionContainer;

        /// <summary>
        /// The amount by which caught fruit should be scaled down to fit on the plate.
        /// </summary>
        private const float caught_fruit_scale_adjust = 0.5f;

        public CatcherPlate()
        {
            Origin = Anchor.BottomCentre;

            InternalChildren = new Drawable[]
            {
                caughtObjectContainer = new Container<CaughtObject>
                {
                    // offset fruit vertically to better place "above" the plate.
                    Y = -5
                },
                hitExplosionContainer = new HitExplosionContainer()
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // Create a pool if not provided. It is convenient for testing.
            if (!Dependencies.TryGet(out caughtObjectPool))
                AddInternal(caughtObjectPool = new CaughtObjectPool());
        }

        public Drawable CreateBackgroundLayerProxy() => caughtObjectContainer.CreateProxy();

        public void OnHitObjectCaught(DrawablePalpableCatchHitObject drawableHitObject, float catcherPosition)
        {
            var positionInStack = computePositionInStack(new Vector2(drawableHitObject.X - catcherPosition, 0), drawableHitObject.DisplaySize.X);

            if (PlaceCaughtObject.Value)
                placeCaughtObject(drawableHitObject, positionInStack);

            if (GenerateHitLighting.Value)
                addHitLighting(drawableHitObject.HitObject, positionInStack.X, drawableHitObject.AccentColour.Value);
        }

        public void OnRevertResult(DrawableCatchHitObject drawableHitObject)
        {
            caughtObjectContainer.RemoveAll(d => d.HitObject == drawableHitObject.HitObject);
            droppedObjectContainer.OnRevertResult(drawableHitObject);
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
            var caughtObject = caughtObjectPool.Get(drawableObject.HitObject);

            caughtObject.CopyStateFrom(drawableObject);
            caughtObject.Anchor = Anchor.TopCentre;
            caughtObject.Position = position;
            caughtObject.Scale *= caught_fruit_scale_adjust;

            caughtObjectContainer.Add(caughtObject);

            if (!caughtObject.StaysOnPlate)
                removeFromPlate(caughtObject, DropAnimation.Explode);
        }

        #endregion

        #region Caught object dropping

        /// <summary>
        /// Drop all caught objects on the stack.
        /// </summary>
        public void DropAll(DropAnimation animation)
        {
            foreach (var caughtObject in caughtObjectContainer)
                droppedObjectContainer.Add(caughtObject, animation);

            caughtObjectContainer.Clear(false);
        }

        private void removeFromPlate(CaughtObject caughtObject, DropAnimation animation)
        {
            droppedObjectContainer.Add(caughtObject, animation);

            caughtObjectContainer.Remove(caughtObject);
        }

        #endregion

        #region Hit lighting

        private void addHitLighting(CatchHitObject hitObject, float x, Color4 colour) =>
            hitExplosionContainer.Add(new HitExplosionEntry(Time.Current, x, hitObject.Scale, colour, hitObject.RandomSeed));

        #endregion
    }
}
