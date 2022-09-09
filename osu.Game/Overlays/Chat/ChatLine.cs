// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public class ChatLine : CompositeDrawable
    {
        public Message Message
        {
            get => message;
            set
            {
                if (message == value) return;

                message = MessageFormatter.FormatMessage(value);

                if (!IsLoaded)
                    return;

                updateMessageContent();
            }
        }

        public LinkFlowContainer ContentFlow { get; private set; } = null!;

        protected virtual float TextSize => 20;

        protected virtual float Spacing => 15;

        protected virtual float TimestampWidth => 60;

        protected virtual float UsernameWidth => 130;

        private Color4 usernameColour;

        private OsuSpriteText timestamp = null!;

        private Message message = null!;

        private OsuSpriteText username = null!;

        private Container? highlight;

        private bool senderHasColour => !string.IsNullOrEmpty(message.Sender.Colour);

        private bool messageHasColour => Message.IsAction && senderHasColour;

        [Resolved]
        private ChannelManager? chatManager { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public ChatLine(Message message)
        {
            Message = message;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider? colourProvider)
        {
            usernameColour = senderHasColour
                ? Color4Extensions.FromHex(message.Sender.Colour)
                : username_colours[message.Sender.Id % username_colours.Length];

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, TimestampWidth + Spacing + UsernameWidth + Spacing),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                timestamp = new OsuSpriteText
                                {
                                    Shadow = false,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: TextSize * 0.75f, weight: FontWeight.SemiBold, fixedWidth: true),
                                    MaxWidth = TimestampWidth,
                                    Colour = colourProvider?.Background1 ?? Colour4.White,
                                },
                                new MessageSender(message.Sender)
                                {
                                    Width = UsernameWidth,
                                    AutoSizeAxes = Axes.Y,
                                    Origin = Anchor.TopRight,
                                    Anchor = Anchor.TopRight,
                                    Child = createUsername(),
                                    Margin = new MarginPadding { Horizontal = Spacing },
                                },
                            },
                        },
                        ContentFlow = new LinkFlowContainer(t =>
                        {
                            t.Shadow = false;
                            t.Font = t.Font.With(size: TextSize, italics: Message.IsAction);
                            t.Colour = messageHasColour ? Color4Extensions.FromHex(message.Sender.Colour) : colourProvider?.Content1 ?? Colour4.White;
                        })
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateMessageContent();
            FinishTransforms(true);
        }

        /// <summary>
        /// Performs a highlight animation on this <see cref="ChatLine"/>.
        /// </summary>
        public void Highlight()
        {
            if (highlight?.IsAlive != true)
            {
                AddInternal(highlight = new Container
                {
                    CornerRadius = 2f,
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Colour = usernameColour.Darken(1f),
                    Depth = float.MaxValue,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                });
            }

            highlight.FadeTo(0.5f).FadeOut(1500, Easing.InQuint);
            highlight.Expire();
        }

        private void updateMessageContent()
        {
            this.FadeTo(message is LocalEchoMessage ? 0.4f : 1.0f, 500, Easing.OutQuint);
            timestamp.FadeTo(message is LocalEchoMessage ? 0 : 1, 500, Easing.OutQuint);

            timestamp.Text = $@"{message.Timestamp.LocalDateTime:HH:mm:ss}";
            username.Text = $@"{message.Sender.Username}";

            // remove non-existent channels from the link list
            message.Links.RemoveAll(link => link.Action == LinkAction.OpenChannel && chatManager?.AvailableChannels.Any(c => c.Name == link.Argument.ToString()) != true);

            ContentFlow.Clear();
            ContentFlow.AddLinks(message.DisplayContent, message.Links);
        }

        private Drawable createUsername()
        {
            username = new OsuSpriteText
            {
                Shadow = false,
                Colour = senderHasColour ? colours.ChatBlue : usernameColour,
                Truncate = true,
                EllipsisString = "…",
                Font = OsuFont.GetFont(size: TextSize, weight: FontWeight.Bold, italics: true),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                MaxWidth = UsernameWidth,
            };

            if (!senderHasColour)
                return username;

            // Background effect
            return new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 4,
                EdgeEffect = new EdgeEffectParameters
                {
                    Roundness = 1,
                    Radius = 1,
                    Colour = Color4.Black.Opacity(0.3f),
                    Offset = new Vector2(0, 1),
                    Type = EdgeEffectType.Shadow,
                },
                Child = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 4,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = usernameColour,
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = 4, Right = 4, Bottom = 1, Top = -2 },
                            Child = username
                        }
                    }
                }
            };
        }

        private class MessageSender : OsuClickableContainer, IHasContextMenu
        {
            private readonly APIUser sender;

            private Action startChatAction = null!;

            [Resolved]
            private IAPIProvider api { get; set; } = null!;

            public MessageSender(APIUser sender)
            {
                this.sender = sender;
            }

            [BackgroundDependencyLoader]
            private void load(UserProfileOverlay? profile, ChannelManager? chatManager, ChatOverlay? chatOverlay)
            {
                Action = () => profile?.ShowUser(sender);
                startChatAction = () =>
                {
                    chatManager?.OpenPrivateChannel(sender);
                    chatOverlay?.Show();
                };
            }

            public MenuItem[] ContextMenuItems
            {
                get
                {
                    if (sender.Equals(APIUser.SYSTEM_USER))
                        return Array.Empty<MenuItem>();

                    List<MenuItem> items = new List<MenuItem>
                    {
                        new OsuMenuItem("View Profile", MenuItemType.Highlighted, Action)
                    };

                    if (!sender.Equals(api.LocalUser.Value))
                        items.Add(new OsuMenuItem("Start Chat", MenuItemType.Standard, startChatAction));

                    return items.ToArray();
                }
            }
        }

        private static readonly Color4[] username_colours =
        {
            Color4Extensions.FromHex("588c7e"),
            Color4Extensions.FromHex("b2a367"),
            Color4Extensions.FromHex("c98f65"),
            Color4Extensions.FromHex("bc5151"),
            Color4Extensions.FromHex("5c8bd6"),
            Color4Extensions.FromHex("7f6ab7"),
            Color4Extensions.FromHex("a368ad"),
            Color4Extensions.FromHex("aa6880"),

            Color4Extensions.FromHex("6fad9b"),
            Color4Extensions.FromHex("f2e394"),
            Color4Extensions.FromHex("f2ae72"),
            Color4Extensions.FromHex("f98f8a"),
            Color4Extensions.FromHex("7daef4"),
            Color4Extensions.FromHex("a691f2"),
            Color4Extensions.FromHex("c894d3"),
            Color4Extensions.FromHex("d895b0"),

            Color4Extensions.FromHex("53c4a1"),
            Color4Extensions.FromHex("eace5c"),
            Color4Extensions.FromHex("ea8c47"),
            Color4Extensions.FromHex("fc4f4f"),
            Color4Extensions.FromHex("3d94ea"),
            Color4Extensions.FromHex("7760ea"),
            Color4Extensions.FromHex("af52c6"),
            Color4Extensions.FromHex("e25696"),

            Color4Extensions.FromHex("677c66"),
            Color4Extensions.FromHex("9b8732"),
            Color4Extensions.FromHex("8c5129"),
            Color4Extensions.FromHex("8c3030"),
            Color4Extensions.FromHex("1f5d91"),
            Color4Extensions.FromHex("4335a5"),
            Color4Extensions.FromHex("812a96"),
            Color4Extensions.FromHex("992861"),
        };
    }
}
