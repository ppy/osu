// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class PlaybackSettings : ReplayGroup
    {
        protected override string Title => @"playback";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new ReplaySliderBar<double>
                {
                    LabelText = "Playback speed",
                    Bindable = config.GetBindable<double>(OsuSetting.PlaybackSpeed),
                    KeyboardStep = 0.5f
                }
            };
        }
    }
}
