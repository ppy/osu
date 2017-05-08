// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options.Sections.General
{
    public class LanguageOptions : OptionsSubsection
    {
        protected override string Header => "Language";

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            Children = new Drawable[]
            {
                new OptionCheckbox
                {
                    LabelText = "Prefer metadata in original language",
                    Bindable = frameworkConfig.GetBindable<bool>(FrameworkConfig.ShowUnicode)
                },
            };
        }
    }
}
