// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class DiscussionSettings : ReplayGroup
    {
        protected override string Title => @"discussions";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new ReplayCheckbox
                {
                    LabelText = "Show floating comments",
                    Bindable = config.GetBindable<bool>(OsuSetting.FloatingComments)
                },
                new FocusedTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 30,
                    PlaceholderText = "Add Comment",
                    HoldFocus = false,
                },
            };
        }
    }
}
