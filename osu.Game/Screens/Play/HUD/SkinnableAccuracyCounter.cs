// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableAccuracyCounter : SkinnableDrawable, IAccuracyCounter
    {
        public Bindable<double> Current { get; } = new Bindable<double>();

        public SkinnableAccuracyCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.AccuracyCounter), _ => new DefaultAccuracyCounter())
        {
            CentreComponent = false;
        }

        private IAccuracyCounter skinnedCounter;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            skinnedCounter = Drawable as IAccuracyCounter;
            skinnedCounter?.Current.BindTo(Current);
        }
    }
}
