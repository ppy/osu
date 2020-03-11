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
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public class ChatLine : CompositeDrawable
    {
        public const float LEFT_PADDING = default_message_padding + default_horizontal_padding * 2;

        private const float default_message_padding = 200;

        protected virtual float MessagePadding => default_message_padding;

        private const float default_timestamp_padding = 65;

        protected virtual float TimestampPadding => default_timestamp_padding;

        private const float default_horizontal_padding = 15;

        protected virtual float HorizontalPadding => default_horizontal_padding;

        protected virtual float TextSize => 20;

        private Color4 customUsernameColour;

        private OsuSpriteText timestamp;

        public ChatLine(Message message)
        {
            Message = message;
            Padding = new MarginPadding { Left = HorizontalPadding, Right = HorizontalPadding };
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

        private bool senderHasBackground => !string.IsNullOrEmpty(message.Sender.Colour);

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            customUsernameColour = colours.ChatBlue;

            bool hasBackground = senderHasBackground;

            Drawable effectedUsername = username = new OsuSpriteText
            {
                Shadow = false,
                Colour = hasBackground ? customUsernameColour : username_colours[message.Sender.Id % username_colours.Length],
                Truncate = true,
                EllipsisString = "… :",
                Font = OsuFont.GetFont(size: TextSize, weight: FontWeight.Bold, italics: true),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                MaxWidth = MessagePadding - TimestampPadding
            };

            if (hasBackground)
            {
                // Background effect
                effectedUsername = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 4,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Roundness = 1,
                        Offset = new Vector2(0, 3),
                        Radius = 3,
                        Colour = Color4.Black.Opacity(0.3f),
                        Type = EdgeEffectType.Shadow,
                    },
                    // Drop shadow effect
                    Child = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 4,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Radius = 1,
                            Colour = Color4Extensions.FromHex(message.Sender.Colour),
                            Type = EdgeEffectType.Shadow,
                        },
                        Padding = new MarginPadding { Left = 3, Right = 3, Bottom = 1, Top = -3 },
                        Y = 3,
                        Child = username,
                    }
                };
            }

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(MessagePadding, TextSize),
                    Children = new Drawable[]
                    {
                        timestamp = new OsuSpriteText
                        {
                            Shadow = false,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: TextSize * 0.75f, weight: FontWeight.SemiBold, fixedWidth: true)
                        },
                        new MessageSender(message.Sender)
                        {
                            AutoSizeAxes = Axes.Both,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Child = effectedUsername,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = MessagePadding + HorizontalPadding },
                    Children = new Drawable[]
                    {
                        ContentFlow = new LinkFlowContainer(t =>
                        {
                            t.Shadow = false;

                            if (Message.IsAction)
                            {
                                t.Font = OsuFont.GetFont(italics: true);

                                if (senderHasBackground)
                                    t.Colour = Color4Extensions.FromHex(message.Sender.Colour);
                            }

                            t.Font = t.Font.With(size: TextSize);
                        })
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                }
            };

            updateMessageContent();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            FinishTransforms(true);
        }

        private void updateMessageContent()
        {
            this.FadeTo(message is LocalEchoMessage ? 0.4f : 1.0f, 500, Easing.OutQuint);
            timestamp.FadeTo(message is LocalEchoMessage ? 0 : 1, 500, Easing.OutQuint);

            timestamp.Text = $@"{message.Timestamp.LocalDateTime:HH:mm:ss}";
            username.Text = $@"{message.Sender.Username}" + (senderHasBackground || message.IsAction ? "" : ":");

            // remove non-existent channels from the link list
            message.Links.RemoveAll(link => link.Action == LinkAction.OpenChannel && chatManager?.AvailableChannels.Any(c => c.Name == link.Argument) != true);

            ContentFlow.Clear();
            ContentFlow.AddLinks(message.DisplayContent, message.Links);
        }

        private class MessageSender : OsuClickableContainer, IHasContextMenu
        {
            private readonly User sender;

            private Action startChatAction;

            [Resolved]
            private IAPIProvider api { get; set; }

            public MessageSender(User sender)
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
                    List<MenuItem> items = new List<MenuItem>
                    {
                        new OsuMenuItem("View Profile", MenuItemType.Highlighted, Action)
                    };

                    if (sender.Id != api.LocalUser.Value.Id)
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
