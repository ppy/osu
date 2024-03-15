// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;

using osu.Framework.Graphics;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;

namespace osu.Game.Rulesets.Objects.Pooling
{
    /// <summary>
    /// A <see cref="PoolableDrawable"/> that is controlled by <see cref="Entry"/> to implement drawable pooling and replay rewinding.
    /// </summary>
    /// <typeparam name="TEntry">The <see cref="LifetimeEntry{T}"/> type storing state and controlling this drawable.</typeparam>
    public abstract partial class PoolableDrawableWithLifetime<TEntry> : PoolableDrawable where TEntry : LifetimeEntry<TEntry>
    {
        private TEntry? entry;

        /// <summary>
        /// The entry holding essential state of this <see cref="PoolableDrawableWithLifetime{TEntry}"/>.
        /// </summary>
        /// <remarks>
        /// If a non-null value is set before loading is started, the entry is applied when the loading is completed.
        /// It is not valid to set an entry while this <see cref="PoolableDrawableWithLifetime{TEntry}"/> is loading.
        /// </remarks>
        public TEntry? Entry
        {
            get => entry;
            set
            {
                if (LoadState == LoadState.NotLoaded)
                    entry = value;
                else if (value != null)
                    Apply(value);
                else if (HasEntryApplied)
                    free();
            }
        }

        /// <summary>
        /// Whether <see cref="Entry"/> is applied to this <see cref="PoolableDrawableWithLifetime{TEntry}"/>.
        /// When an <see cref="Entry"/> is set during initialization, it is not applied until loading is completed.
        /// </summary>
        protected bool HasEntryApplied { get; private set; }

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                if (Entry == null && LifetimeStart != value)
                    throw new InvalidOperationException($"Cannot modify lifetime of {nameof(PoolableDrawableWithLifetime<TEntry>)} when entry is not set");

                if (Entry != null)
                    Entry.LifetimeStart = value;
            }
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                if (Entry == null && LifetimeEnd != value)
                    throw new InvalidOperationException($"Cannot modify lifetime of {nameof(PoolableDrawableWithLifetime<TEntry>)} when entry is not set");

                if (Entry != null)
                    Entry.LifetimeEnd = value;
            }
        }

        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;

        protected PoolableDrawableWithLifetime(TEntry? initialEntry = null)
        {
            Entry = initialEntry;
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();

            // Apply the initial entry.
            if (Entry != null && !HasEntryApplied)
                apply(Entry);
        }

        /// <summary>
        /// Applies a new entry to be represented by this drawable.
        /// If there is an existing entry applied, the entry will be replaced.
        /// </summary>
        public void Apply(TEntry entry)
        {
            if (LoadState == LoadState.Loading)
                throw new InvalidOperationException($"Cannot apply a new {nameof(TEntry)} while currently loading.");

            apply(entry);
        }

        protected sealed override void FreeAfterUse()
        {
            base.FreeAfterUse();

            // We preserve the existing entry in case we want to move a non-pooled drawable between different parent drawables.
            if (HasEntryApplied && IsInPool)
                free();
        }

        /// <summary>
        /// Invoked to apply a new entry to this drawable.
        /// </summary>
        protected virtual void OnApply(TEntry entry)
        {
        }

        /// <summary>
        /// Invoked to revert application of the entry to this drawable.
        /// </summary>
        protected virtual void OnFree(TEntry entry)
        {
        }

        private void apply(TEntry entry)
        {
            if (HasEntryApplied)
                free();

            this.entry = entry;
            entry.LifetimeChanged += setLifetimeFromEntry;
            setLifetimeFromEntry(entry);

            OnApply(entry);

            HasEntryApplied = true;
        }

        private void free()
        {
            Debug.Assert(Entry != null && HasEntryApplied);

            OnFree(Entry);

            Entry.LifetimeChanged -= setLifetimeFromEntry;
            entry = null;
            base.LifetimeStart = double.MinValue;
            base.LifetimeEnd = double.MaxValue;

            HasEntryApplied = false;
        }

        private void setLifetimeFromEntry(TEntry entry)
        {
            Debug.Assert(entry == Entry);
            base.LifetimeStart = entry.LifetimeStart;
            base.LifetimeEnd = entry.LifetimeEnd;
        }
    }
}
