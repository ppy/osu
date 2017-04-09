// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.General
{
    public class LanguageOptions : OptionsSubsection
    {
        protected override string Header => "Language";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionLabel { Text = "TODO: Dropdown" },
                new OsuCheckbox
                {
                    LabelText = "Prefer metadata in original language",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowUnicode)
                },
                new OsuCheckbox
                {
                    LabelText = "Use alternative font for chat display",
                    Bindable = config.GetBindable<bool>(OsuConfig.AlternativeChatFont)
                },
            };
        }
    }
}
