// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Input
{
    public class OtherInputOptions : OptionsSubsection
    {
        protected override string Header => "Other";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OsuCheckbox
                {
                    LabelText = "OS TabletPC support",
                    Bindable = config.GetBindable<bool>(OsuConfig.Tablet)
                },
                new OsuCheckbox
                {
                    LabelText = "Wiimote/TaTaCon Drum Support",
                    Bindable = config.GetBindable<bool>(OsuConfig.Wiimote)
                },
            };
        }
    }
}

