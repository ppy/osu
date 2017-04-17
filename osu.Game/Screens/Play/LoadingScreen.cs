using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Menu;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public class LoadingScreen : OsuScreen
    {

        private string loadingText = "loading...";

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = loadingText,
                    TextSize = 48,
                    Font = @"Exo2.0-MediumItalic"
                }
            );
        }
    }
}
