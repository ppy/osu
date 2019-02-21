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
        protected Bindable<double> DimLevel;
        protected Bindable<bool> ShowStoryboard;
        public Bindable<bool> EnableUserDim = new Bindable<bool>();
        public Bindable<bool> StoryboardReplacesBackground = new Bindable<bool>();

        private readonly bool isStoryboard;

        private const float background_fade_duration = 800;

        public UserDimContainer(bool isStoryboard = false)
        {
            this.isStoryboard = isStoryboard;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
            EnableUserDim.ValueChanged  += _ => updateBackgroundDim();
            DimLevel.ValueChanged  += _ => updateBackgroundDim();
            ShowStoryboard.ValueChanged += _ => updateBackgroundDim();
        }

        private void updateBackgroundDim()
        {
            if (isStoryboard)
            {
                this.FadeTo(!ShowStoryboard || DimLevel == 1 ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }
            else
            {
                // The background needs to be hidden in the case of it being replaced
                this.FadeTo(ShowStoryboard && StoryboardReplacesBackground ? 0 : 1, background_fade_duration, Easing.OutQuint);
            }

            this.FadeColour(EnableUserDim ? OsuColour.Gray(1 - (float)DimLevel) : Color4.White, background_fade_duration, Easing.OutQuint);
        }
    }
}
