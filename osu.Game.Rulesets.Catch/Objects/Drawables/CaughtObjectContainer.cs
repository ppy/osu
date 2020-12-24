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
    public class CaughtObjectContainer : CompositeDrawable
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
        /// It is incremented for each <see cref="AddStackObject"/> or <see cref="AddDropObject"/> call and decremented for each <see cref="RemoveEntry"/> call to make a replay consistent.
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
        /// </summary>
        public CaughtObjectEntry AddStackObject(IHasCatchObjectState source, Vector2 positionInStack)
        {
            var delayedDropEntry = new DroppedObjectEntry(positionInStack, source);
            var stackEntry = new StackedObjectEntry(positionInStack, delayedDropEntry, source);

            addImmediateEntry(stackEntry);

            // `DropStackedObjects` may be called before lifetime update.
            if (stackEntry.LifetimeStart <= Clock.CurrentTime)
                aliveStackedObjects.Add(stackEntry);

            randomSeed++;

            return stackEntry;
        }

        /// <summary>
        /// Immediately drop a caught object.
        /// </summary>
        public CaughtObjectEntry AddDropObject(IHasCatchObjectState source, Vector2 positionInStack, DroppedObjectAnimation animation, int mirrorDirection)
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

        /// <summary>
        /// Remove caught objects
        /// </summary>
        public bool RemoveEntry(CaughtObjectEntry entry)
        {
            randomSeed--;

            return removeImmediateEntry(entry);
        }

        public Vector2 GetPositionInStack(Vector2 position, float displayRadius)
        {
            const float radius_div_2 = CatchHitObject.OBJECT_RADIUS / 2;
            const float allowance = 10;

            int iteration = 0;

            while (aliveStackedObjects.Any(f => Vector2Extensions.Distance(f.PositionInStack, position) < (displayRadius + radius_div_2) / (allowance / 2)))
            {
                float diff = (displayRadius + radius_div_2) / allowance;

                position.X += (StatelessRNG.NextSingle(randomSeed, iteration + 2) - 0.5f) * diff * 2;
                position.Y -= StatelessRNG.NextSingle(randomSeed, iteration * 2 + 1) * diff;

                iteration++;
            }

            position.X = Math.Clamp(position.X, -CatcherArea.CATCHER_SIZE / 2, CatcherArea.CATCHER_SIZE / 2);

            return position;
        }

        public void DropStackedObjects(DroppedObjectAnimation animation, int mirrorDirection)
        {
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

        private class StackedObjectEntry : CaughtObjectEntry
        {
            /// <summary>
            /// The position of this object in relative to the catcher.
            /// </summary>
            public readonly Vector2 PositionInStack;

            public readonly DroppedObjectEntry DelayedDropEntry;

            public StackedObjectEntry(Vector2 positionInStack, DroppedObjectEntry delayedDropEntry, IHasCatchObjectState source)
                : base(source)
            {
                PositionInStack = positionInStack;
                DelayedDropEntry = delayedDropEntry;
            }

            public override void ApplyTransforms(Drawable d)
            {
                d.Position = PositionInStack;
            }
        }

        private class DroppedObjectEntry : CaughtObjectEntry
        {
            public DroppedObjectAnimation Animation;

            /// <summary>
            /// The initial position of the dropped object.
            /// </summary>
            public Vector2 DropPosition;

            /// <summary>
            /// 1 or -1 representing visual mirroring of the object.
            /// </summary>
            public int MirrorDirection = 1;

            private readonly Vector2 positionInStack;

            public DroppedObjectEntry(Vector2 positionInStack, IHasCatchObjectState source)
                : base(source)
            {
                this.positionInStack = positionInStack;
            }

            public override void ApplyTransforms(Drawable d)
            {
                d.Position = DropPosition;
                d.Scale *= new Vector2(MirrorDirection, 1);

                using (d.BeginAbsoluteSequence(LifetimeStart))
                {
                    switch (Animation)
                    {
                        case DroppedObjectAnimation.Explode:
                            var xMovement = positionInStack.X * MirrorDirection * 6;
                            d.MoveToY(d.Y - 50, 250, Easing.OutSine).Then().MoveToY(d.Y + 50, 500, Easing.InSine);
                            d.MoveToX(d.X + xMovement, 1000);
                            d.FadeOut(750);
                            break;

                        case DroppedObjectAnimation.Drop:
                            d.MoveToY(d.Y + 75, 750, Easing.InSine);
                            d.FadeOut(750);
                            break;
                    }
                }
            }
        }
    }

    public enum DroppedObjectAnimation
    {
        Explode,
        Drop
    }
}
