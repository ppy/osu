//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.General
{
    public class LanguageOptions : OptionsSubsection
    {
        protected override string Header => "Language";
        
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "TODO: Dropdown" },
                new CheckBoxOption
                {
                    LabelText = "Prefer metadata in original language",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowUnicode)
                },
                new CheckBoxOption
                {
                    LabelText = "Use alternative font for chat display",
                    Bindable = config.GetBindable<bool>(OsuConfig.AlternativeChatFont)
                },
            };
        }
    }
}
