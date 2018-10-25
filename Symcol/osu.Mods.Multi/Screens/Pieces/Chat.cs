using System.Linq;
using osu.Core;
using osu.Core.Config;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Mods.Multi.Networking;
using osu.Mods.Multi.Networking.Packets.Match;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Screens.Pieces
{
    public class Chat : MultiplayerContainer
    {
        private string playerColorHex = SymcolOsuModSet.SymcolConfigManager.GetBindable<string>(SymcolSetting.PlayerColor);

        private readonly FillFlowContainer<ChatMessage> messageFlow;

        private readonly OsuScrollContainer scroll;

        public Chat(OsuNetworkingHandler osuNetworkingHandler) : base (osuNetworkingHandler)
        {
            OsuTextBox textBox;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.Both;
            Height = 0.46f;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.8f
                },
                scroll = new OsuScrollContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.8f,

                    Children = new Drawable[]
                    {
                        messageFlow = new FillFlowContainer<ChatMessage>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        }
                    }
                },
                textBox = new OsuTextBox
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.98f,
                    Height = 36,
                    Position = new Vector2(0, -12),
                    Colour = Color4.White,
                    PlaceholderText = "Type Message Here!",
                    ReleaseFocusOnCommit = false,
                }
            };

            textBox.OnCommit += (s, r) =>
            {
                AddMessage(textBox.Text);
                textBox.Text = "";
            };
        }

        protected override void OnPacketRecieve(PacketInfo info)
        {
            if (info.Packet is ChatPacket chatPacket)
                Add(chatPacket);
        }

        public void Add(ChatPacket packet)
        {
            ChatMessage message = new ChatMessage(packet);
            messageFlow.Add(message);

            if (scroll.IsScrolledToEnd(10) || !messageFlow.Children.Any())
                scrollToEnd();
        }

        public void AddMessage(string message)
        {
            if (message == "" | message == " ")
                return;

            try
            {
                OsuColour.FromHex(playerColorHex);
            }
            catch
            {
                playerColorHex = "#ffffff";
            }

            SendPacket(new ChatPacket
            {
                AuthorColor = playerColorHex,
                Message = message,
            });
        }

        private void scrollToEnd() => ScheduleAfterChildren(() => scroll.ScrollToEnd());
    }
}
