// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Overlays.Team.Header.Components;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Team.Header
{
    public partial class BottomHeaderContainer : CompositeDrawable
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        private TeamChatButton chatButton = null!;
        private TeamActionsButton actionsButton = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public BottomHeaderContainer()
        {
            Height = 60;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Padding = new MarginPadding { Vertical = 10 },
                        Margin = new MarginPadding { Right = WaveOverlayContainer.HORIZONTAL_PADDING },
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            chatButton = new TeamChatButton { TeamData = { BindTarget = TeamData }, Alpha = 0 },
                            actionsButton = new TeamActionsButton { TeamData = { BindTarget = TeamData }, Alpha = 1 },
                        }
                    }
                }
            };

            TeamData.ValueChanged += _ => updateDisplay();
        }

        private void updateDisplay()
        {
            if (api.LocalUser.Value.Team?.Id != TeamData.Value?.Team.Id)
            {
                chatButton.Alpha = 0;
                actionsButton.Alpha = 1;
            }
            else
            {
                chatButton.Alpha = 1;
                actionsButton.Alpha = 0;
            }
        }

        public partial class TeamChatButton : ProfileHeaderButton
        {
            public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

            [Resolved]
            private TeamProfileOverlay? teamProfileOverlay { get; set; }

            [Resolved]
            private ChatOverlay? chatOverlay { get; set; }

            [Resolved]
            private ChannelManager? channelManager { get; set; }

            public TeamChatButton()
            {
                Content.Padding = new MarginPadding { Vertical = 10, Horizontal = 30 };

                Child = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = TeamsStrings.ShowBarChat,
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                };

                Action = () =>
                {
                    // This assumes that a user is only in one team at once,
                    // and that osu-web will provide that channel in the presence response.
                    var channel = channelManager?.JoinedChannels.FirstOrDefault(c => c.Type == ChannelType.Team);

                    if (channel == null || channelManager == null)
                        return;

                    channelManager.CurrentChannel.Value = channel;
                    teamProfileOverlay?.Hide();
                    chatOverlay?.Show();
                };
            }
        }
    }
}
