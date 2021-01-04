// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    /// <summary>
    /// Maintains the object stack on the catcher plate and objects dropped from the catcher plate.
    /// </summary>
    public partial class CaughtObjectContainer : CompositeDrawable
    {
        public Container<DrawableCaughtObject> StackedObjectContainer { get; }

        private readonly Container<DrawableCaughtObject> droppedObjectTarget;

        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();

        private readonly Dictionary<LifetimeEntry, DrawableCaughtObject> drawableMap = new Dictionary<LifetimeEntry, DrawableCaughtObject>();

        private readonly HashSet<StackedObjectEntry> aliveStackedObjects = new HashSet<StackedObjectEntry>();

        private readonly DrawablePool<DrawableCaughtFruit> caughtFruitPool;
        private readonly DrawablePool<DrawableCaughtBanana> caughtBananaPool;
        private readonly DrawablePool<DrawableCaughtDroplet> caughtDropletPool;

        /// <summary>
        /// The randomness used to compute position in stack.
        /// It is incremented for each <see cref="addStackedObject"/> or <see cref="addDroppedObject"/> call and decremented for each <see cref="RemoveCaughtObject"/> call to make a replay consistent.
        /// </summary>
        private int randomSeed = 1;

        public CaughtObjectContainer(Container<DrawableCaughtObject> droppedObjectTarget)
        {
            this.droppedObjectTarget = droppedObjectTarget;

            InternalChildren = new Drawable[]
            {
                caughtFruitPool = new DrawablePool<DrawableCaughtFruit>(50),
                caughtBananaPool = new DrawablePool<DrawableCaughtBanana>(100),
                // less capacity is needed compared to fruit because droplet is not stacked
                caughtDropletPool = new DrawablePool<DrawableCaughtDroplet>(25),
                StackedObjectContainer = new Container<DrawableCaughtObject>()
            };

            lifetimeManager.EntryBecameAlive += entryBecameAlive;
            lifetimeManager.EntryBecameDead += entryBecameDead;
        }

        /// <summary>
        /// Add a caught object to the stack.
        /// If the caught object is a droplet, it is dropped from the stack immediately.
        /// </summary>
        /// <param name="source">The source of the caught object.</param>
        /// <param name="positionInStack">The position of the caught object.</param>
        /// <param name="mirrorDirection">The current direction of the stack, specified by 1 or -1.</param>
        /// <returns>The caught object entry.</returns>
        public CaughtObjectEntry AddCaughtObject(IHasCatchObjectState source, Vector2 positionInStack, int mirrorDirection)
        {
            if (Math.Abs(mirrorDirection) != 1)
                throw new InvalidOperationException($"{nameof(mirrorDirection)} must be either 1 or -1");

            return source.HitObject is Droplet
                ? addDroppedObject(source, positionInStack, DroppedObjectAnimation.Explode, mirrorDirection)
                : addStackedObject(source, positionInStack);
        }

        /// <summary>
        /// Remove a caught object.
        /// </summary>
        /// <param name="entry">The caught object entry to remove.</param>
        /// <returns>Whether the caught object is removed.</returns>
        public bool RemoveCaughtObject(CaughtObjectEntry entry)
        {
            randomSeed--;

            return removeImmediateEntry(entry);
        }

        /// <summary>
        /// Compute the stacked object position.
        /// </summary>
        /// <param name="position">The initial position of the object.</param>
        /// <param name="displayRadius">The size used for the collision detection in the stack.</param>
        /// <returns>The position in the stack.</returns>
        public Vector2 GetPositionInStack(Vector2 position, float displayRadius)
        {
            const float radius_div_2 = CatchHitObject.OBJECT_RADIUS / 2;
            const float allowance = 10;

            int iteration = 0;

            while (aliveStackedObjects.Any(f => Vector2Extensions.Distance(f.PositionInStack, position) < (displayRadius + radius_div_2) / (allowance / 2)))
            {
                float diff = (displayRadius + radius_div_2) / allowance;

                position.X += (StatelessRNG.NextSingle(randomSeed, iteration * 2) - 0.5f) * diff * 2;
                position.Y -= StatelessRNG.NextSingle(randomSeed, iteration * 2 + 1) * diff;

                iteration++;
            }

            position.X = Math.Clamp(position.X, -CatcherArea.CATCHER_SIZE / 2, CatcherArea.CATCHER_SIZE / 2);

            return position;
        }

        /// <summary>
        /// Drop all stacked objects.
        /// </summary>
        /// <param name="animation">The animation played on the dropped objects.</param>
        /// <param name="mirrorDirection">The current direction of the stack, specified by 1 or -1.</param>
        public void DropStackedObjects(DroppedObjectAnimation animation, int mirrorDirection)
        {
            if (Math.Abs(mirrorDirection) != 1)
                throw new InvalidOperationException($"{nameof(mirrorDirection)} must be either 1 or -1");

            double currentTime = Clock.CurrentTime;

            foreach (var stackEntry in aliveStackedObjects)
                dropStackedObject(stackEntry, currentTime, animation, mirrorDirection);

            aliveStackedObjects.Clear();
        }

        protected override bool CheckChildrenLife()
        {
            bool aliveChanged = base.CheckChildrenLife();
            aliveChanged |= lifetimeManager.Update(Time.Current);
            return aliveChanged;
        }

        private void entryBecameAlive(LifetimeEntry lifetimeEntry)
        {
            var entry = (CaughtObjectEntry)lifetimeEntry;

            if (entry is StackedObjectEntry stackEntry)
                aliveStackedObjects.Add(stackEntry);

            var drawable = getPooledDrawable(entry.HitObject);
            drawable.Apply(entry);

            if (entry is DroppedObjectEntry)
                entry.LifetimeEnd = drawable.LatestTransformEndTime;

            addDrawable(entry, drawable);
        }

        private void entryBecameDead(LifetimeEntry lifetimeEntry)
        {
            var entry = (CaughtObjectEntry)lifetimeEntry;

            if (entry is StackedObjectEntry stackEntry)
                aliveStackedObjects.Remove(stackEntry);

            removeDrawable(entry);
        }

        /// <summary>
        /// Add a caught object to the stack.
        /// </summary>
        /// <param name="source">The source of the caught object.</param>
        /// <param name="positionInStack">The position of the caught object.</param>
        /// <returns>The caught object entry of the stacked object.</returns>
        private CaughtObjectEntry addStackedObject(IHasCatchObjectState source, Vector2 positionInStack)
        {
            var delayedDropEntry = new DroppedObjectEntry(positionInStack, source);
            var stackEntry = new StackedObjectEntry(positionInStack, delayedDropEntry, source);

            addImmediateEntry(stackEntry);

            // Add the stack entry to the alive object set now to make it available
            // when `DropStackedObjects` or `GetPositionInStack` is called before `lifetimeManager.Update`.
            aliveStackedObjects.Add(stackEntry);

            randomSeed++;

            return stackEntry;
        }

        /// <summary>
        /// Immediately drop a caught object.
        /// </summary>
        /// <param name="source">The source of the caught object.</param>
        /// <param name="positionInStack">The position the object dropped from.</param>
        /// <param name="animation">The animation played on the dropped object.</param>
        /// <param name="mirrorDirection">The current direction of the stack, specified by 1 or -1.</param>
        /// <returns>The caught object entry of the dropped object.</returns>
        private CaughtObjectEntry addDroppedObject(IHasCatchObjectState source, Vector2 positionInStack, DroppedObjectAnimation animation, int mirrorDirection)
        {
            var dropEntry = new DroppedObjectEntry(positionInStack, source)
            {
                Animation = animation,
                DropPosition = getCurrentDropPosition(positionInStack),
                MirrorDirection = mirrorDirection
            };

            addImmediateEntry(dropEntry);

            randomSeed++;

            return dropEntry;
        }

        private void addImmediateEntry(CaughtObjectEntry entry)
        {
            entry.LifetimeStart = Clock.CurrentTime;

            lifetimeManager.AddEntry(entry);

            if (entry is StackedObjectEntry stackEntry)
                addDelayedDropEntry(stackEntry.DelayedDropEntry);
        }

        private void addDelayedDropEntry(DroppedObjectEntry entry)
        {
            entry.LifetimeStart = double.PositiveInfinity;

            lifetimeManager.AddEntry(entry);
        }

        private bool removeImmediateEntry(CaughtObjectEntry entry)
        {
            if (!lifetimeManager.RemoveEntry(entry))
                return false;

            if (entry is StackedObjectEntry stackEntry)
                removeDelayedDropEntry(stackEntry.DelayedDropEntry);

            return true;
        }

        private void removeDelayedDropEntry(DroppedObjectEntry entry)
        {
            bool removed = lifetimeManager.RemoveEntry(entry);
            Debug.Assert(removed);
        }

        private Vector2 getCurrentDropPosition(Vector2 positionInStack)
        {
            return StackedObjectContainer.ToSpaceOfOtherDrawable(positionInStack, droppedObjectTarget);
        }

        private void dropStackedObject(StackedObjectEntry stackEntry, double time, DroppedObjectAnimation animation, int mirrorDirection)
        {
            stackEntry.LifetimeEnd = time;

            var dropEntry = stackEntry.DelayedDropEntry;
            dropEntry.LifetimeStart = time;
            dropEntry.Animation = animation;
            dropEntry.DropPosition = getCurrentDropPosition(stackEntry.PositionInStack);
            dropEntry.MirrorDirection = mirrorDirection;
        }

        private void addDrawable(CaughtObjectEntry entry, DrawableCaughtObject drawable)
        {
            if (entry is StackedObjectEntry)
                StackedObjectContainer.Add(drawable);
            else
                droppedObjectTarget.Add(drawable);

            drawableMap[entry] = drawable;
        }

        private void removeDrawable(CaughtObjectEntry entry)
        {
            bool removed = drawableMap.Remove(entry, out DrawableCaughtObject drawable);
            Debug.Assert(removed);

            if (entry is StackedObjectEntry)
                StackedObjectContainer.Remove(drawable);
            else
                droppedObjectTarget.Remove(drawable);
        }

        private DrawableCaughtObject getPooledDrawable(CatchHitObject hitObject)
        {
            switch (hitObject)
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
    }
}
