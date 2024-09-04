// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Performance;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class MovementPathContainer : Path
    {
        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();
        private readonly SortedSet<AimPointEntry> aliveEntries = new SortedSet<AimPointEntry>(new AimLinePointComparator());

        public MovementPathContainer()
        {
            lifetimeManager.EntryBecameAlive += entryBecameAlive;
            lifetimeManager.EntryBecameDead += entryBecameDead;

            PathRadius = 1f;
            Colour = new Color4(255, 255, 255, 127);
        }

        protected override void Update()
        {
            base.Update();

            lifetimeManager.Update(Time.Current);
        }

        public void Add(AimPointEntry entry) => lifetimeManager.AddEntry(entry);

        private void entryBecameAlive(LifetimeEntry entry)
        {
            aliveEntries.Add((AimPointEntry)entry);
            updateVertices();
        }

        private void entryBecameDead(LifetimeEntry entry)
        {
            aliveEntries.Remove((AimPointEntry)entry);
            updateVertices();
        }

        private void updateVertices()
        {
            ClearVertices();

            foreach (var entry in aliveEntries)
            {
                AddVertex(entry.Position);
            }
        }

        private sealed class AimLinePointComparator : IComparer<AimPointEntry>
        {
            public int Compare(AimPointEntry? x, AimPointEntry? y)
            {
                ArgumentNullException.ThrowIfNull(x);
                ArgumentNullException.ThrowIfNull(y);

                return x.LifetimeStart.CompareTo(y.LifetimeStart);
            }
        }
    }
}
