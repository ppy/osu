// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Diagnostics;
using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;

namespace osu.Game.Rulesets.Objects.Pooling
{
    /// <summary>
    /// A <see cref="PoolableDrawable"/> that is controlled by <see cref="Entry"/> to implement drawable pooling and replay rewinding.
    /// </summary>
    /// <typeparam name="TEntry">The <see cref="LifetimeEntry"/> type storing state and controlling this drawable.</typeparam>
    public abstract class PoolableDrawableWithLifetime<TEntry> : PoolableDrawable where TEntry : LifetimeEntry
    {
        /// <summary>
        /// The entry holding essential state of this <see cref="PoolableDrawableWithLifetime{TEntry}"/>.
        /// </summary>
        protected TEntry? Entry { get; private set; }

        /// <summary>
        /// Whether <see cref="Entry"/> is applied to this <see cref="PoolableDrawableWithLifetime{TEntry}"/>.
        /// When an initial entry is specified in the constructor, <see cref="Entry"/> is set but not applied until loading is completed.
        /// </summary>
        protected bool HasEntryApplied { get; private set; }

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set => setLifetime(value, LifetimeEnd);
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set => setLifetime(LifetimeStart, value);
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

            // Apply the initial entry given in the constructor.
            if (Entry != null && !HasEntryApplied)
                Apply(Entry);
        }

        /// <summary>
        /// Applies a new entry to be represented by this drawable.
        /// If there is an existing entry applied, the entry will be replaced.
        /// </summary>
        public void Apply(TEntry entry)
        {
            if (HasEntryApplied)
                free();

            setLifetime(entry.LifetimeStart, entry.LifetimeEnd);
            Entry = entry;

            OnApply(entry);

            HasEntryApplied = true;
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

        private void setLifetime(double start, double end)
        {
            base.LifetimeStart = start;
            base.LifetimeEnd = end;

            if (Entry != null)
            {
                Entry.LifetimeStart = start;
                Entry.LifetimeEnd = end;
            }
        }

        private void free()
        {
            Debug.Assert(Entry != null && HasEntryApplied);

            OnFree(Entry);

            Entry = null;
            setLifetime(double.MaxValue, double.MaxValue);

            HasEntryApplied = false;
        }
    }
}
