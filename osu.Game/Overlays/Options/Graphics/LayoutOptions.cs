//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class LayoutOptions : OptionsSubsection
    {
        protected override string Header => "Layout";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Resolution: TODO dropdown" },
                new CheckBoxOption
                {
                    LabelText = "Fullscreen mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.Fullscreen),
                },
                new CheckBoxOption
                {
                    LabelText = "Letterboxing",
                    Bindable = config.GetBindable<bool>(OsuConfig.Letterboxing),
                },
                new SpriteText { Text = "Horizontal position" },
                new SpriteText { Text = "TODO: slider" },
                new SpriteText { Text = "Vertical position" },
                new SpriteText { Text = "TODO: slider" },
            };
        }
    }
}