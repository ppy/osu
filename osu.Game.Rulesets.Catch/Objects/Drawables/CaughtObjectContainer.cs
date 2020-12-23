// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        private readonly HashSet<CaughtObjectEntry> aliveStackedObjects = new HashSet<CaughtObjectEntry>();
        private readonly Dictionary<CaughtObjectEntry, CaughtObjectEntry> dropEntryMap = new Dictionary<CaughtObjectEntry, CaughtObjectEntry>();

        private readonly DrawablePool<DrawableCaughtFruit> caughtFruitPool;
        private readonly DrawablePool<DrawableCaughtBanana> caughtBananaPool;
        private readonly DrawablePool<DrawableCaughtDroplet> caughtDropletPool;

        /// <summary>
        /// The randomness used to compute position in stack.
        /// It is incremented for each <see cref="Add"/> call and decremented for each <see cref="Remove"/> call to make a replay consistent.
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

        public void Add(CaughtObjectEntry entry)
        {
            randomSeed++;

            lifetimeManager.AddEntry(entry);

            if (entry.State != CaughtObjectState.Stacked) return;

            // `DropStackedObjects` may be called before lifetime update.
            if (entry.LifetimeStart <= Time.Current)
                aliveStackedObjects.Add(entry);

            var dropEntry = new CaughtObjectEntry(CaughtObjectState.Dropped, entry.PositionInStack, entry)
            {
                LifetimeStart = double.PositiveInfinity,
                LifetimeEnd = double.PositiveInfinity
            };

            lifetimeManager.AddEntry(dropEntry);
            dropEntryMap[entry] = dropEntry;
        }

        public void Remove(CaughtObjectEntry entry)
        {
            randomSeed--;

            lifetimeManager.RemoveEntry(entry);
            removeDrawable(entry);

            aliveStackedObjects.Remove(entry);

            if (!dropEntryMap.TryGetValue(entry, out var dropEntry)) return;

            dropEntryMap.Remove(entry);

            lifetimeManager.RemoveEntry(dropEntry);
            removeDrawable(dropEntry);
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

        public void DropStackedObjects(Action<DrawableCaughtObject> applyTransforms)
        {
            dropStackedObjects(Clock.CurrentTime, applyTransforms);
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

            if (entry.State == CaughtObjectState.Stacked)
                aliveStackedObjects.Add(entry);

            var drawable = getPooledDrawable(entry.HitObject);
            drawable.Apply(entry);

            addDrawable(entry, drawable);
        }

        private void entryBecameDead(LifetimeEntry lifetimeEntry)
        {
            var entry = (CaughtObjectEntry)lifetimeEntry;

            if (entry.State == CaughtObjectState.Stacked)
                aliveStackedObjects.Remove(entry);

            removeDrawable(entry);
        }

        private void dropStackedObjects(double time, Action<DrawableCaughtObject> applyTransforms)
        {
            foreach (var entry in aliveStackedObjects)
            {
                entry.LifetimeEnd = time;

                if (!dropEntryMap.TryGetValue(entry, out var dropEntry)) continue;

                dropEntry.LifetimeStart = time;
                dropEntry.ApplyTransforms = applyTransforms;
            }

            aliveStackedObjects.Clear();
        }

        private void addDrawable(CaughtObjectEntry entry, DrawableCaughtObject drawable)
        {
            if (entry.State == CaughtObjectState.Stacked)
            {
                drawable.Position = entry.PositionInStack;
                StackedObjectContainer.Add(drawable);
            }
            else
            {
                drawable.Position = StackedObjectContainer.ToSpaceOfOtherDrawable(entry.PositionInStack, droppedObjectTarget);
                droppedObjectTarget.Add(drawable);
            }

            if (entry.ApplyTransforms != null)
            {
                using (drawable.BeginAbsoluteSequence(entry.LifetimeStart))
                {
                    entry.ApplyTransforms(drawable);
                    entry.LifetimeEnd = drawable.LatestTransformEndTime;
                }
            }

            drawableMap[entry] = drawable;
        }

        private void removeDrawable(CaughtObjectEntry entry)
        {
            if (!drawableMap.TryGetValue(entry, out DrawableCaughtObject drawable))
                return;

            if (entry.State == CaughtObjectState.Stacked)
                StackedObjectContainer.Remove(drawable);
            else
                droppedObjectTarget.Remove(drawable);

            drawableMap.Remove(entry);
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
