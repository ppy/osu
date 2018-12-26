using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Online.Multiplayer.GameTypes
{
    public class GameTypeVersus : GameType
    {
        public override string Name => "Versus";

        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new VersusRow(colours.Blue, colours.Blue, size * 0.6f)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }
    }
}