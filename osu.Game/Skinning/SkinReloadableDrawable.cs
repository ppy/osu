// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Pooling;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A poolable drawable implementation which has a pre-wired callback (see <see cref="SkinChanged"/>) that fires
    /// once on load and again on any subsequent skin change.
    /// </summary>
    public abstract partial class SkinReloadableDrawable : PoolableDrawable
    {
        /// <summary>
        /// Invoked when <see cref="CurrentSkin"/> has changed.
        /// </summary>
        public event Action? OnSkinChanged;

        /// <summary>
        /// The current skin source.
        /// </summary>
        protected ISkinSource CurrentSkin { get; private set; } = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource source)
        {
            CurrentSkin = source;
            CurrentSkin.SourceChanged += onChange;
        }

        private void onChange() =>
            // schedule required to avoid calls after disposed.
            // note that this has the side-effect of components only performing a skin change when they are alive.
            Scheduler.AddOnce(skinChanged);

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            skinChanged();
        }

        private void skinChanged()
        {
            SkinChanged(CurrentSkin);
            OnSkinChanged?.Invoke();
        }

        /// <summary>
        /// Called when a change is made to the skin.
        /// </summary>
        /// <param name="skin">The new skin.</param>
        protected virtual void SkinChanged(ISkinSource skin)
        {
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (CurrentSkin.IsNotNull())
                CurrentSkin.SourceChanged -= onChange;

            OnSkinChanged = null;
        }
    }
}
