// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableScoreCounter : SkinnableDrawable, IScoreCounter
    {
        public Bindable<double> Current { get; } = new Bindable<double>();

        public SkinnableScoreCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.ScoreCounter), _ => new DefaultScoreCounter())
        {
            CentreComponent = false;
        }

        private IScoreCounter skinnedCounter;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            skinnedCounter = Drawable as IScoreCounter;
            skinnedCounter?.Current.BindTo(Current);
        }
    }
}
