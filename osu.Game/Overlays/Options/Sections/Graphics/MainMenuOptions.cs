// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Graphics
{
    public class MainMenuOptions : OptionsSubsection
    {
        protected override string Header => "Main Menu";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new OsuCheckbox
                {
                    LabelText = "Snow",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuSnow),
                    TooltipText = "Little representation of your current game mode will fall from the sky."
                },
                new OsuCheckbox
                {
                    LabelText = "Parallax",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuParallax),
                    TooltipText = "Add a parallax effect based on the current cursor position."
                },
                new OsuCheckbox
                {
                    LabelText = "Menu tips",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowMenuTips),
                    TooltipText = "The main menu will display tips on how to get the most out of osu!."
                },
                new OsuCheckbox
                {
                    LabelText = "Interface voices",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuVoice),
                    TooltipText = "osu! will greet you when you start and quit."
                },
                new OsuCheckbox
                {
                    LabelText = "osu! music theme",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuMusic),
                    TooltipText = "osu! will play themed music throughout the game, instead of using random beatmaps."
                },
            };
        }
    }
}