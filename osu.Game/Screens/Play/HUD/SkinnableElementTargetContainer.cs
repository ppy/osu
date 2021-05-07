// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableElementTargetContainer : SkinReloadableDrawable, ISkinnableTarget
    {
        public SkinnableTarget Target { get; }

        public SkinnableElementTargetContainer(SkinnableTarget target)
        {
            Target = target;
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            var loadable = skin.GetDrawableComponent(new SkinnableTargetComponent(Target));

            ClearInternal();
            if (loadable != null)
                LoadComponentAsync(loadable, AddInternal);
        }
    }
}
