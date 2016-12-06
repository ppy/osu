//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class OnlineIntegrationOptions : OptionsSubsection
    {
        protected override string Header => "Integration";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Integrate with Yahoo! status display",
                    Bindable = config.GetBindable<bool>(OsuConfig.YahooIntegration)
                },
                new CheckBoxOption
                {
                    LabelText = "Integrate with MSN Live status display",
                    Bindable = config.GetBindable<bool>(OsuConfig.MsnIntegration)
                },
                new CheckBoxOption
                {
                    LabelText = "Automatically start osu!direct downloads",
                    Bindable = config.GetBindable<bool>(OsuConfig.AutomaticDownload)
                },
                new CheckBoxOption
                {
                    LabelText = "Prefer no-video downloads",
                    Bindable = config.GetBindable<bool>(OsuConfig.AutomaticDownloadNoVideo)
                },
            };
        }
    }
}