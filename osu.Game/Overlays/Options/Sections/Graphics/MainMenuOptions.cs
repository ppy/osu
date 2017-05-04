// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Sections.Graphics
{
    public class MainMenuOptions : OptionsSubsection
    {
        protected override string Header => "User Interface";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new OptionCheckbox
                {
                    LabelText = "Parallax",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuParallax)
                },
            };
        }
    }
}
