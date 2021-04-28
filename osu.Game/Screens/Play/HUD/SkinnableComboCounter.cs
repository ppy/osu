// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableComboCounter : SkinnableDrawable, IComboCounter
    {
        public Bindable<int> Current { get; } = new Bindable<int>();

        public SkinnableComboCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.ComboCounter), skinComponent => new DefaultComboCounter())
        {
            CentreComponent = false;
        }

        private IComboCounter skinnedCounter;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            skinnedCounter = Drawable as IComboCounter;
            skinnedCounter?.Current.BindTo(Current);
        }
    }
}
