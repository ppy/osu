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
        protected virtual float TextSize => 20;

        protected virtual float Spacing => 15;

        protected virtual float TimestampWidth => 60;

        protected virtual float UsernameWidth => 130;

        private Color4 usernameColour;

        private OsuSpriteText timestamp;

        public ChatLine(Message message)
        {
            Message = message;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [Resolved(CanBeNull = true)]
        private ChannelManager chatManager { get; set; }

        private Message message;
        private OsuSpriteText username;

        public LinkFlowContainer ContentFlow { get; private set; }

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

        private bool senderHasColour => !string.IsNullOrEmpty(message.Sender.Colour);

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved(CanBeNull = true)]
        private OverlayColourProvider colourProvider { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            usernameColour = senderHasColour
                ? Color4Extensions.FromHex(message.Sender.Colour)
                : username_colours[message.Sender.Id % username_colours.Length];

            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, TimestampWidth),
                                    new Dimension(GridSizeMode.Absolute, Spacing),
                                    new Dimension(GridSizeMode.Absolute, UsernameWidth),
                                    new Dimension(GridSizeMode.Absolute, Spacing),
                                },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        timestamp = new OsuSpriteText
                                        {
                                            Shadow = false,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = OsuFont.GetFont(size: TextSize * 0.75f, weight: FontWeight.SemiBold, fixedWidth: true),
                                            Width = TimestampWidth,
                                            Colour = colourProvider?.Background1 ?? Colour4.White,
                                        },
                                        Drawable.Empty(),
                                        new MessageSender(message.Sender)
                                        {
                                            Height = TextSize,
                                            RelativeSizeAxes = Axes.X,
                                            Origin = Anchor.CentreRight,
                                            Anchor = Anchor.CentreRight,
                                            Child = createUsername(),
                                            Masking = true,
                                        },
                                        Drawable.Empty(),
                                    },
                                },
                            },
                            ContentFlow = new LinkFlowContainer(t =>
                            {
                                t.Shadow = false;
                                t.Font = t.Font.With(size: TextSize);
                                t.Colour = colourProvider?.Content1 ?? Colour4.White;

                                if (Message.IsAction && senderHasColour)
                                    t.Colour = usernameColour;

                                if (Message.IsAction)
                                    t.Font = OsuFont.GetFont(italics: true);
                            })
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                            },
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateMessageContent();
            FinishTransforms(true);
        }

        private Container highlight;

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

            return new Container
            {
                Origin = Anchor.CentreRight,
                Anchor = Anchor.CentreRight,
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
                    Y = 0,
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

        private class MessageSender : OsuClickableContainer, IHasContextMenu
        {
            private readonly APIUser sender;

            private Action startChatAction;

            [Resolved]
            private IAPIProvider api { get; set; }

            public MessageSender(APIUser sender)
            {
                this.sender = sender;
            }

            [BackgroundDependencyLoader(true)]
            private void load(UserProfileOverlay profile, ChannelManager chatManager)
            {
                Action = () => profile?.ShowUser(sender);
                startChatAction = () => chatManager?.OpenPrivateChannel(sender);
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
