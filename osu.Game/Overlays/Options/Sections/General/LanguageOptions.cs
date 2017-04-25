// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.General
{
    public class LanguageOptions : OptionsSubsection
    {
        protected override string Header => "Language";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager frameworkConfig)
        {
            Children = new Drawable[]
            {
                new OptionLabel { Text = "TODO: Dropdown" },
                new OsuCheckbox
                {
                    LabelText = "Prefer metadata in original language",
                    Bindable = frameworkConfig.GetBindable<bool>(FrameworkConfig.ShowUnicode)
                },
                new OsuCheckbox
                {
                    LabelText = "Use alternative font for chat display",
                    Bindable = osuConfig.GetBindable<bool>(OsuConfig.AlternativeChatFont)
                },
            };
        }
    }
}
