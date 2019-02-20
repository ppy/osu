// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    public class UserDimContainer : Container
    {
        #region User Settings

        protected Bindable<double> DimLevel;

        #endregion

        public Bindable<bool> EnableUserDim = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            EnableUserDim.ValueChanged  += _ => updateBackgroundDim();
            DimLevel.ValueChanged  += _ => updateBackgroundDim();
        }

        private void updateBackgroundDim()
        {
            this.FadeColour(EnableUserDim ? OsuColour.Gray(1 - (float)DimLevel) : Color4.White, 800, Easing.OutQuint);
        }
    }
}
