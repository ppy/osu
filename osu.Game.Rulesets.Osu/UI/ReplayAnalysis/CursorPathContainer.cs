// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Performance;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class CursorPathContainer : SmoothPath
    {
        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();
        private readonly SortedSet<AnalysisFrameEntry> aliveEntries = new SortedSet<AnalysisFrameEntry>(new AimLinePointComparator());

        public CursorPathContainer()
        {
            lifetimeManager.EntryBecameAlive += entryBecameAlive;
            lifetimeManager.EntryBecameDead += entryBecameDead;

            PathRadius = 1f;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Pink2;
        }

        protected override void Update()
        {
            base.Update();

            lifetimeManager.Update(Time.Current);
        }

        public void Add(AnalysisFrameEntry entry) => lifetimeManager.AddEntry(entry);

        private void entryBecameAlive(LifetimeEntry entry)
        {
            aliveEntries.Add((AnalysisFrameEntry)entry);
            updateVertices();
        }

        private void entryBecameDead(LifetimeEntry entry)
        {
            aliveEntries.Remove((AnalysisFrameEntry)entry);
            updateVertices();
        }

        private void updateVertices()
        {
            ClearVertices();

            Vector2 min = Vector2.Zero;

            foreach (var entry in aliveEntries)
            {
                AddVertex(entry.Position);
                if (entry.Position.X < min.X)
                    min.X = entry.Position.X;

                if (entry.Position.Y < min.Y)
                    min.Y = entry.Position.Y;
            }

            Position = min;
        }

        private sealed class AimLinePointComparator : IComparer<AnalysisFrameEntry>
        {
            public int Compare(AnalysisFrameEntry? x, AnalysisFrameEntry? y)
            {
                ArgumentNullException.ThrowIfNull(x);
                ArgumentNullException.ThrowIfNull(y);

                return x.LifetimeStart.CompareTo(y.LifetimeStart);
            }
        }
    }
}
