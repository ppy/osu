using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class MainMenuOptions : OptionsSubsection
    {
        protected override string Header => "Main Menu";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new CheckBoxOption
                {
                    LabelText = "Snow",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuSnow)
                },
                new CheckBoxOption
                {
                    LabelText = "Parallax",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuParallax)
                },
                new CheckBoxOption
                {
                    LabelText = "Menu tips",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowMenuTips)
                },
                new CheckBoxOption
                {
                    LabelText = "Interface voices",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuVoice)
                },
                new CheckBoxOption
                {
                    LabelText = "osu! music theme",
                    Bindable = config.GetBindable<bool>(OsuConfig.MenuMusic)
                },
            };
        }
    }
}