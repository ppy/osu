using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Users;
using Symcol.Core.Networking;
using Symcol.Rulesets.Core.Multiplayer.Networking;

namespace Symcol.Rulesets.Core.Multiplayer.Pieces
{
    public class Chat : Container, IOnlineComponent
    {
        private readonly RulesetNetworkingClientHandler rulesetNetworkingClientHandler;

        private string playerColorHex = SymcolSettingsSubsection.SymcolConfigManager.GetBindable<string>(SymcolSetting.PlayerColor);

        private User user;

        private readonly FillFlowContainer<ChatMessage> messageContainer;
        private readonly OsuTextBox textBox;

        public Chat(RulesetNetworkingClientHandler rulesetNetworkingClientHandler)
        {
            this.rulesetNetworkingClientHandler = rulesetNetworkingClientHandler;

            rulesetNetworkingClientHandler.OnPacketReceive += (packet) =>
            {
                if (packet is ChatPacket chatPacket)
                    Add(chatPacket);
                if (rulesetNetworkingClientHandler.ClientType == ClientType.Host)
                    rulesetNetworkingClientHandler.ShareWithOtherPeers(packet);
            };

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
                new OsuScrollContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.9f,

                    Children = new Drawable[]
                    {
                        messageContainer = new FillFlowContainer<ChatMessage>
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
                    Text = "Type here!"
                }
            };

            textBox.OnCommit += (s, r) =>
            {
                AddMessage(textBox.Text);
                textBox.Text = "";
            };
        }

        public void Add(ChatPacket packet)
        {
            ChatMessage message = new ChatMessage(packet);
            messageContainer.Add(message);
        }

        public void AddMessage(string message)
        {
            if (message == "" | message == " ")
                return;

            if (user != null)
            {
                try
                {
                    OsuColour.FromHex(playerColorHex);
                }
                catch
                {
                    playerColorHex = "#ffffff";
                }

                ChatPacket packet = new ChatPacket(rulesetNetworkingClientHandler.ClientInfo)
                {
                    Author = user.Username,
                    AuthorColor = playerColorHex,
                    Message = message,
                };

                rulesetNetworkingClientHandler.SendToHost(packet);
                rulesetNetworkingClientHandler.SendToInMatchClients(packet);
                Add(packet);
            }
            else
                Logger.Log("You must be logged in to message!", LoggingTarget.Network, LogLevel.Error);
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            api.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                default:
                    user = null;
                    break;
                case APIState.Online:
                    user = api.LocalUser.Value;
                    break;
            }
        }
    }
}
