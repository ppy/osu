// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Pooling;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which has a callback when the skin changes.
    /// </summary>
    public abstract class SkinReloadableDrawable : PoolableDrawable
    {
        /// <summary>
        /// Invoked when <see cref="CurrentSkin"/> has changed.
        /// </summary>
        public event Action OnSkinChanged;

        /// <summary>
        /// The current skin source.
        /// </summary>
        protected ISkinSource CurrentSkin { get; private set; }

        private readonly Func<ISkinSource, bool> allowFallback;

        /// <summary>
        /// Whether fallback to default skin should be allowed if the custom skin is missing this resource.
        /// </summary>
        protected bool AllowDefaultFallback => allowFallback == null || allowFallback.Invoke(CurrentSkin);

        /// <summary>
        /// Create a new <see cref="SkinReloadableDrawable"/>
        /// </summary>
        /// <param name="allowFallback">A conditional to decide whether to allow fallback to the default implementation if a skinned element is not present.</param>
        protected SkinReloadableDrawable(Func<ISkinSource, bool> allowFallback = null)
        {
            this.allowFallback = allowFallback;
        }

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
            SkinChanged(CurrentSkin, AllowDefaultFallback);
            OnSkinChanged?.Invoke();
        }

        /// <summary>
        /// Called when a change is made to the skin.
        /// </summary>
        /// <param name="skin">The new skin.</param>
        /// <param name="allowFallback">Whether fallback to default skin should be allowed if the custom skin is missing this resource.</param>
        protected virtual void SkinChanged(ISkinSource skin, bool allowFallback)
        {
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (CurrentSkin != null)
                CurrentSkin.SourceChanged -= onChange;

            OnSkinChanged = null;
        }
    }
}
