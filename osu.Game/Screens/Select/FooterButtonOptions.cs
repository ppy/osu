using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class FooterButtonOptions : FooterButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Blue;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"options";
            Hotkey = Key.F3;
        }
    }
}
