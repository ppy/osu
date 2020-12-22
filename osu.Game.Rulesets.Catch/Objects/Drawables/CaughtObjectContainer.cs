// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class CaughtObjectContainer : CompositeDrawable
    {
        public Container<CaughtObject> StackedObjectContainer { get; }

        private readonly Container<CaughtObject> droppedObjectTarget;

        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();

        private readonly Dictionary<LifetimeEntry, CaughtObject> drawableMap = new Dictionary<LifetimeEntry, CaughtObject>();

        private readonly HashSet<CaughtObjectEntry> aliveStackedObjects = new HashSet<CaughtObjectEntry>();
        private readonly Dictionary<CaughtObjectEntry, CaughtObjectEntry> dropEntryMap = new Dictionary<CaughtObjectEntry, CaughtObjectEntry>();

        private readonly DrawablePool<CaughtFruit> caughtFruitPool;
        private readonly DrawablePool<CaughtBanana> caughtBananaPool;
        private readonly DrawablePool<CaughtDroplet> caughtDropletPool;

        public CaughtObjectContainer(Container<CaughtObject> droppedObjectTarget)
        {
            this.droppedObjectTarget = droppedObjectTarget;

            InternalChildren = new Drawable[]
            {
                caughtFruitPool = new DrawablePool<CaughtFruit>(50),
                caughtBananaPool = new DrawablePool<CaughtBanana>(100),
                // less capacity is needed compared to fruit because droplet is not stacked
                caughtDropletPool = new DrawablePool<CaughtDroplet>(25),
                StackedObjectContainer = new Container<CaughtObject>()
            };

            lifetimeManager.EntryBecameAlive += entryBecameAlive;
            lifetimeManager.EntryBecameDead += entryBecameDead;
        }

        public void Add(CaughtObjectEntry entry)
        {
            lifetimeManager.AddEntry(entry);

            if (entry.State != CaughtObjectState.Stacked) return;

            // `DropStackedObjects` may be called before lifetime update.
            if (entry.StartTime <= Time.Current)
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

            while (aliveStackedObjects.Any(f => Vector2Extensions.Distance(f.PositionInStack, position) < (displayRadius + radius_div_2) / (allowance / 2)))
            {
                float diff = (displayRadius + radius_div_2) / allowance;

                position.X += (RNG.NextSingle() - 0.5f) * diff * 2;
                position.Y -= RNG.NextSingle() * diff;
            }

            position.X = Math.Clamp(position.X, -CatcherArea.CATCHER_SIZE / 2, CatcherArea.CATCHER_SIZE / 2);

            return position;
        }

        public void DropStackedObjects(Action<CaughtObject> applyTransforms)
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
            Console.WriteLine($"{nameof(entryBecameAlive)} at {Clock.CurrentTime}: {entry.State} {entry.ObjectType} {entry.StartTime}");

            if (entry.State == CaughtObjectState.Stacked)
                aliveStackedObjects.Add(entry);

            var drawable = getPooledDrawable(entry.ObjectType);
            drawable.Apply(entry);

            if (entry.ApplyTransforms != null)
            {
                using (drawable.BeginAbsoluteSequence(entry.LifetimeStart))
                {
                    entry.ApplyTransforms(drawable);
                    entry.LifetimeEnd = drawable.LatestTransformEndTime;
                }
            }

            addDrawable(entry, drawable);
        }

        private void entryBecameDead(LifetimeEntry lifetimeEntry)
        {
            var entry = (CaughtObjectEntry)lifetimeEntry;
            Console.WriteLine($"{nameof(entryBecameDead)} at {Clock.CurrentTime}: {entry.State} {entry.ObjectType} {entry.StartTime}");

            if (entry.State == CaughtObjectState.Stacked)
                aliveStackedObjects.Remove(entry);

            removeDrawable(entry);
        }

        private void dropStackedObjects(double time, Action<CaughtObject> applyTransforms)
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

        private void addDrawable(CaughtObjectEntry entry, CaughtObject drawable)
        {
            if (entry.State == CaughtObjectState.Stacked)
                StackedObjectContainer.Add(drawable);
            else
                droppedObjectTarget.Add(drawable);

            drawableMap[entry] = drawable;
        }

        private void removeDrawable(CaughtObjectEntry entry)
        {
            if (!drawableMap.TryGetValue(entry, out CaughtObject drawable))
                return;

            if (entry.State == CaughtObjectState.Stacked)
                StackedObjectContainer.Remove(drawable);
            else
                droppedObjectTarget.Remove(drawable);

            drawableMap.Remove(entry);
        }

        private CaughtObject getPooledDrawable(CatchObjectType type)
        {
            switch (type)
            {
                case CatchObjectType.Fruit:
                    return caughtFruitPool.Get();

                case CatchObjectType.Banana:
                    return caughtBananaPool.Get();

                case CatchObjectType.Droplet:
                    return caughtDropletPool.Get();

                default:
                    return null;
            }
        }
    }
}
