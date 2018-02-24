// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which has a callback when the skin changes.
    /// </summary>
    public abstract class SkinReloadableDrawable : CompositeDrawable
    {
        private Bindable<Skin> skin;

        /// <summary>
        /// Whether fallback to default skin should be allowed if the custom skin is missing this resource.
        /// </summary>
        private readonly bool allowDefaultFallback;

        /// <summary>
        /// Create a new <see cref="SkinReloadableDrawable"/>
        /// </summary>
        /// <param name="fallback">Whether fallback to default skin should be allowed if the custom skin is missing this resource.</param>
        protected SkinReloadableDrawable(bool fallback = true)
        {
            allowDefaultFallback = fallback;
        }

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager)
        {
            skin = skinManager.CurrentSkin.GetBoundCopy();
            skin.ValueChanged += skin => SkinChanged(skin, allowDefaultFallback || skin.SkinInfo == SkinInfo.Default);
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            skin.TriggerChange();
        }

        /// <summary>
        /// Called when a change is made to the skin.
        /// </summary>
        /// <param name="skin">The new skin.</param>
        /// <param name="allowFallback">Whether fallback to default skin should be allowed if the custom skin is missing this resource.</param>
        protected virtual void SkinChanged(Skin skin, bool allowFallback)
        {
        }
    }
}
