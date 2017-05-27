// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class DiscussionSettings : SettingsDropdownContainer
    {
        protected override string Title => @"discussions";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Add(new SettingsCheckbox
            {
                LabelText = "Show floating coments",
                Bindable = config.GetBindable<bool>(OsuSetting.FloatingComments)
            });
            Add(new FocusedTextBox
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                PlaceholderText = "Add Comment",
                HoldFocus = false,
            });
        }
    }
}
