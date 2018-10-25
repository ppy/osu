using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;
using Symcol.osu.Mods.Caster.Pieces;

namespace Symcol.osu.Mods.Caster.CasterScreens
{
    public class Maps : CasterSubScreen
    {
        public Maps(CasterControlPanel controlPanel)
            : base(controlPanel)
        {
            Child = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Colour = Color4.White,
                Text = "Check back later!",
                TextSize = 80
            };
        }
    }
}
