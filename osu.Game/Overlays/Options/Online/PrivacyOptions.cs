//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class PrivacyOptions : OptionsSubsection
    {
        protected override string Header => "Privacy";
    
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Share your city location with others",
                    Bindable = config.GetBindable<bool>(OsuConfig.DisplayCityLocation)
                },
                new CheckBoxOption
                {
                    LabelText = "Allow multiplayer game invites from all users",
                    Bindable = config.GetBindable<bool>(OsuConfig.AllowPublicInvites)
                },
            };
        }
    }
}