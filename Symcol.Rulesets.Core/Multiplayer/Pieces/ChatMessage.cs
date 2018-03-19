using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using Symcol.Rulesets.Core.Multiplayer.Networking;

namespace Symcol.Rulesets.Core.Multiplayer.Pieces
{
    public class ChatMessage : Container
    {
        public ChatMessage(ChatPacket packet)
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Colour = OsuColour.FromHex(packet.AuthorColor),
                    TextSize = 24,
                    Text = packet.Author + ":"
                },
                new OsuTextFlowContainer(t => { t.TextSize = 24; })
                {
                    Position = new OpenTK.Vector2(140, 0),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Text = packet.Message
                }
            };
        }
    }
}
