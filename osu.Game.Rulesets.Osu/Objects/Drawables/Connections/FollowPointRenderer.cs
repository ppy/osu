// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    /// <summary>
    /// Visualises connections between <see cref="DrawableOsuHitObject"/>s.
    /// </summary>
    public class FollowPointRenderer : CompositeDrawable
    {
        public override bool RemoveCompletedTransforms => false;

        private DrawablePool<FollowPointConnection> connectionPool;
        private DrawablePool<FollowPoint> pointPool;

        private readonly List<FollowPointLifetimeEntry> lifetimeEntries = new List<FollowPointLifetimeEntry>();
        private readonly Dictionary<LifetimeEntry, FollowPointConnection> connectionsInUse = new Dictionary<LifetimeEntry, FollowPointConnection>();
        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();

        public FollowPointRenderer()
        {
            lifetimeManager.EntryBecameAlive += onEntryBecameAlive;
            lifetimeManager.EntryBecameDead += onEntryBecameDead;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                connectionPool = new DrawablePool<FollowPointConnection>(1, 200),
                pointPool = new DrawablePool<FollowPoint>(50, 1000)
            };

            MakeChildAlive(connectionPool);
            MakeChildAlive(pointPool);
        }

        public void AddFollowPoints2(OsuHitObject hitObject)
        {
            var newEntry = new FollowPointLifetimeEntry(hitObject);

            var index = lifetimeEntries.AddInPlace(newEntry, Comparer<FollowPointLifetimeEntry>.Create((e1, e2) =>
            {
                int comp = e1.Start.StartTime.CompareTo(e2.Start.StartTime);

                if (comp != 0)
                    return comp;

                // we always want to insert the new item after equal ones.
                // this is important for beatmaps with multiple hitobjects at the same point in time.
                // if we use standard comparison insert order, there will be a churn of connections getting re-updated to
                // the next object at the point-in-time, adding a construction/disposal overhead (see FollowPointConnection.End implementation's ClearInternal).
                // this is easily visible on https://osu.ppy.sh/beatmapsets/150945#osu/372245
                return -1;
            }));

            if (index < lifetimeEntries.Count - 1)
            {
                // Update the connection's end point to the next connection's start point
                //     h1 -> -> -> h2
                //    connection    nextGroup

                FollowPointLifetimeEntry nextEntry = lifetimeEntries[index + 1];
                newEntry.End = nextEntry.Start;
            }
            else
            {
                // The end point may be non-null during re-ordering
                newEntry.End = null;
            }

            if (index > 0)
            {
                // Update the previous connection's end point to the current connection's start point
                //     h1 -> -> -> h2
                //  prevGroup    connection

                FollowPointLifetimeEntry previousEntry = lifetimeEntries[index - 1];
                previousEntry.End = newEntry.Start;
            }

            lifetimeManager.AddEntry(newEntry);
        }

        public void RemoveFollowPoints2(OsuHitObject hitObject)
        {
            int index = lifetimeEntries.FindIndex(e => e.Start == hitObject);
            var entry = lifetimeEntries[index];

            lifetimeEntries.RemoveAt(index);
            lifetimeManager.RemoveEntry(entry);

            if (index > 0)
            {
                // Update the previous connection's end point to the next connection's start point
                //     h1 -> -> -> h2 -> -> -> h3
                //  prevGroup    connection       nextGroup
                // The current connection's end point is used since there may not be a next connection
                FollowPointLifetimeEntry previousEntry = lifetimeEntries[index - 1];
                previousEntry.End = entry.End;
            }
        }

        protected override bool CheckChildrenLife() => lifetimeManager.Update(Time.Current);

        private void onEntryBecameAlive(LifetimeEntry entry)
        {
            var connection = connectionPool.Get(c =>
            {
                c.Entry = (FollowPointLifetimeEntry)entry;
                c.Pool = pointPool;
            });

            connectionsInUse[entry] = connection;

            AddInternal(connection);
            MakeChildAlive(connection);
        }

        private void onEntryBecameDead(LifetimeEntry entry)
        {
            RemoveInternal(connectionsInUse[entry]);
            connectionsInUse.Remove(entry);
        }

        public class FollowPointLifetimeEntry : LifetimeEntry
        {
            public readonly OsuHitObject Start;

            public FollowPointLifetimeEntry(OsuHitObject start)
            {
                Start = start;

                LifetimeStart = LifetimeEnd = Start.StartTime;
            }

            private OsuHitObject end;

            public OsuHitObject End
            {
                get => end;
                set
                {
                    end = value;
                    computeLifetimes();
                }
            }

            private void computeLifetimes()
            {
                if (end == null)
                {
                    LifetimeEnd = LifetimeStart;
                    return;
                }

                Vector2 startPosition = Start.StackedEndPosition;
                Vector2 endPosition = End.StackedPosition;
                Vector2 distanceVector = endPosition - startPosition;
                float fraction = (int)(FollowPointConnection.SPACING * 1.5) / distanceVector.Length;

                double duration = End.StartTime - Start.GetEndTime();

                double fadeOutTime = Start.StartTime + fraction * duration;
                double fadeInTime = fadeOutTime - FollowPointConnection.PREEMPT;

                LifetimeStart = fadeInTime;
            }
        }
    }
}
