using osu.Core.Containers.Shawdooow;
using osu.Core.OsuMods;
using osu.Framework.Graphics;
using osu.Game.Screens;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.MapMixer
{
    public class MapMixerModSet : OsuModSet
    {
        public override SymcolButton GetMenuButton() => new SymcolButton
        {
            ButtonName = "Map Mixer",
            ButtonFontSizeMultiplier = 0.8f,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            ButtonColorTop = Color4.Purple,
            ButtonColorBottom = Color4.HotPink,
            ButtonSize = 120,
            ButtonPosition = new Vector2(-200, -150),
        };

        public override OsuScreen GetScreen() => new MapMixer();

        //public override WikiSet GetWikiSet() => new MapMixerWikiSet();
    }
}
