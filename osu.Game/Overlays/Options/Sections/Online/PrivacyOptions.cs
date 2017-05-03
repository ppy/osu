﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Online
{
    public class PrivacyOptions : OptionsSubsection
    {
        protected override string Header => "Privacy";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OsuCheckbox
                {
                    LabelText = "Share your city location with others",
                    Bindable = config.GetBindable<bool>(OsuConfig.DisplayCityLocation),
                    TooltipText = "By default, other users will only be able to see yourr country. Enabling this adds your city to the publicly visible location. This is usually quite accurate."
                },
                new OsuCheckbox
                {
                    LabelText = "Allow multiplayer game invites from all users",
                    Bindable = config.GetBindable<bool>(OsuConfig.AllowPublicInvites),
                    TooltipText = "Uncheck this to limit incoming invites to friends only."
                },
            };
        }
    }
}