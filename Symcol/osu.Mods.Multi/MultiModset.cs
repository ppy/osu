using osu.Core.Containers.Shawdooow;
using osu.Core.OsuMods;
using osu.Framework.Graphics;
using osu.Game.Screens;
using osu.Mods.Multi.Screens;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Multi
{
    public class MultiModset : OsuModSet
    {
        public override SymcolButton GetMenuButton() => new SymcolButton
        {
            ButtonName = "Multi",
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            ButtonColorTop = Color4.Blue,
            ButtonColorBottom = Color4.Red,
            ButtonSize = 100,
            ButtonPosition = new Vector2(10, -220),
        };

        public override OsuScreen GetScreen() => new ConnectToServer();
    }
}
