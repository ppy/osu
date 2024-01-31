// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;

namespace osu.Game.Rulesets.Objects.Pooling
{
    /// <summary>
    /// A container of <typeparamref name="TDrawable"/>s dynamically added/removed by model <typeparamref name="TEntry"/>s.
    /// When an entry became alive, a drawable corresponding to the entry is obtained (potentially pooled), and added to this container.
    /// The drawable is removed when the entry became dead.
    /// </summary>
    /// <typeparam name="TEntry">The type of entries managed by this container.</typeparam>
    /// <typeparam name="TDrawable">The type of drawables corresponding to the entries.</typeparam>
    public abstract partial class PooledDrawableWithLifetimeContainer<TEntry, TDrawable> : CompositeDrawable
        where TEntry : LifetimeEntry
        where TDrawable : Drawable
    {
        /// <summary>
        /// All entries added to this container, including dead entries.
        /// </summary>
        /// <remarks>
        /// The enumeration order is undefined.
        /// </remarks>
        public IEnumerable<TEntry> Entries => allEntries;

        /// <summary>
        /// All alive entries and drawables corresponding to the entries.
        /// </summary>
        /// <remarks>
        /// The enumeration order is undefined.
        /// </remarks>
        public readonly ReadOnlyDictionary<TEntry, TDrawable> AliveEntries;

        /// <summary>
        /// Whether to remove an entry when clock goes backward and crossed its <see cref="LifetimeEntry.LifetimeStart"/>.
        /// Used when entries are dynamically added at its <see cref="LifetimeEntry.LifetimeStart"/> to prevent duplicated entries.
        /// </summary>
        protected virtual bool RemoveRewoundEntry => false;

        /// <summary>
        /// The amount of time prior to the current time within which entries should be considered alive.
        /// </summary>
        internal double PastLifetimeExtension { get; set; }

        /// <summary>
        /// The amount of time after the current time within which entries should be considered alive.
        /// </summary>
        internal double FutureLifetimeExtension { get; set; }

        private readonly Dictionary<TEntry, TDrawable> aliveDrawableMap = new Dictionary<TEntry, TDrawable>();
        private readonly HashSet<TEntry> allEntries = new HashSet<TEntry>();

        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();

        protected PooledDrawableWithLifetimeContainer()
        {
            lifetimeManager.EntryBecameAlive += entryBecameAlive;
            lifetimeManager.EntryBecameDead += entryBecameDead;
            lifetimeManager.EntryCrossedBoundary += entryCrossedBoundary;

            AliveEntries = new ReadOnlyDictionary<TEntry, TDrawable>(aliveDrawableMap);
        }

        /// <summary>
        /// Add a <typeparamref name="TEntry"/> to be managed by this container.
        /// </summary>
        /// <remarks>
        /// The aliveness of the entry is not updated until <see cref="CheckChildrenLife"/>.
        /// </remarks>
        public virtual void Add(TEntry entry)
        {
            allEntries.Add(entry);
            lifetimeManager.AddEntry(entry);
        }

        /// <summary>
        /// Remove a <typeparamref name="TEntry"/> from this container.
        /// </summary>
        /// <remarks>
        /// If the entry was alive, the corresponding drawable is removed.
        /// </remarks>
        /// <returns>Whether the entry was in this container.</returns>
        public virtual bool Remove(TEntry entry)
        {
            if (!lifetimeManager.RemoveEntry(entry)) return false;

            allEntries.Remove(entry);
            return true;
        }

        /// <summary>
        /// Initialize new <typeparamref name="TDrawable"/> corresponding <paramref name="entry"/>.
        /// </summary>
        /// <returns>The <typeparamref name="TDrawable"/> corresponding to the entry.</returns>
        protected abstract TDrawable GetDrawable(TEntry entry);

        private void entryBecameAlive(LifetimeEntry lifetimeEntry)
        {
            var entry = (TEntry)lifetimeEntry;
            Debug.Assert(!aliveDrawableMap.ContainsKey(entry));

            TDrawable drawable = GetDrawable(entry);
            aliveDrawableMap[entry] = drawable;
            AddDrawable(entry, drawable);
        }

        /// <summary>
        /// Add a <typeparamref name="TDrawable"/> corresponding to <paramref name="entry"/> to this container.
        /// </summary>
        /// <remarks>
        /// Invoked when the entry became alive and a <typeparamref name="TDrawable"/> is obtained by <see cref="GetDrawable"/>.
        /// </remarks>
        protected virtual void AddDrawable(TEntry entry, TDrawable drawable) => AddInternal(drawable);

        private void entryBecameDead(LifetimeEntry lifetimeEntry)
        {
            var entry = (TEntry)lifetimeEntry;
            Debug.Assert(aliveDrawableMap.ContainsKey(entry));

            TDrawable drawable = aliveDrawableMap[entry];
            aliveDrawableMap.Remove(entry);
            RemoveDrawable(entry, drawable);
        }

        /// <summary>
        /// Remove a <typeparamref name="TDrawable"/> corresponding to <paramref name="entry"/> from this container.
        /// </summary>
        /// <remarks>
        /// Invoked when the entry became dead.
        /// </remarks>
        protected virtual void RemoveDrawable(TEntry entry, TDrawable drawable) => RemoveInternal(drawable, false);

        private void entryCrossedBoundary(LifetimeEntry lifetimeEntry, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction)
        {
            if (RemoveRewoundEntry && kind == LifetimeBoundaryKind.Start && direction == LifetimeBoundaryCrossingDirection.Backward)
                Remove((TEntry)lifetimeEntry);
        }

        /// <summary>
        /// Remove all <typeparamref name="TEntry"/>s.
        /// </summary>
        public void Clear()
        {
            foreach (var entry in Entries.ToArray())
                Remove(entry);

            Debug.Assert(aliveDrawableMap.Count == 0);
        }

        protected override bool CheckChildrenLife()
        {
            if (!IsPresent)
                return false;

            bool aliveChanged = base.CheckChildrenLife();
            aliveChanged |= lifetimeManager.Update(Time.Current - PastLifetimeExtension, Time.Current + FutureLifetimeExtension);
            return aliveChanged;
        }
    }
}
