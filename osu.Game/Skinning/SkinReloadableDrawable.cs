// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which has a callback when the skin changes.
    /// </summary>
    public abstract class SkinReloadableDrawable : CompositeDrawable
    {
        private readonly Func<ISkinSource, bool> allowFallback;
        private ISkinSource skin;

        /// <summary>
        /// Whether fallback to default skin should be allowed if the custom skin is missing this resource.
        /// </summary>
        private bool allowDefaultFallback => allowFallback == null || allowFallback.Invoke(skin);

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
            skin = source;
            skin.SourceChanged += onChange;
        }

        private void onChange() => SkinChanged(skin, allowDefaultFallback);

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            onChange();
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

            if (skin != null)
                skin.SourceChanged -= onChange;
        }
    }
}
