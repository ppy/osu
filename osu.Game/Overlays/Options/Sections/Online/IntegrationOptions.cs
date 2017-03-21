// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Sections.Online
{
    public class IntegrationOptions : OptionsSubsection
    {
        protected override string Header => "Integration";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionCheckbox
                {
                    LabelText = "Integrate with Yahoo! status display",
                    Bindable = config.GetBindable<bool>(OsuConfig.YahooIntegration)
                },
                new OptionCheckbox
                {
                    LabelText = "Integrate with MSN Live status display",
                    Bindable = config.GetBindable<bool>(OsuConfig.MsnIntegration)
                },
                new OptionCheckbox
                {
                    LabelText = "Automatically start osu!direct downloads",
                    Bindable = config.GetBindable<bool>(OsuConfig.AutomaticDownload)
                },
                new OptionCheckbox
                {
                    LabelText = "Prefer no-video downloads",
                    Bindable = config.GetBindable<bool>(OsuConfig.AutomaticDownloadNoVideo)
                },
            };
        }
    }
}