// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Chat
{
    public partial class ChatLine : CompositeDrawable
    {
        private Message message = null!;

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

        public IReadOnlyCollection<Drawable> DrawableContentFlow => drawableContentFlow;

        protected virtual float FontSize => 20;

        protected virtual float Spacing => 15;

        protected virtual float UsernameWidth => 130;

        [Resolved]
        private ChannelManager? chatManager { get; set; }

        [Resolved]
        private OverlayColourProvider? colourProvider { get; set; }

        private readonly OsuSpriteText drawableTimestamp;

        private readonly DrawableUsername drawableUsername;

        private readonly LinkFlowContainer drawableContentFlow;

        private readonly Bindable<bool> prefer24HourTime = new Bindable<bool>();

        private Container? highlight;

        public ChatLine(Message message)
        {
            Message = message;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Absolute, Spacing + UsernameWidth + Spacing),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        drawableTimestamp = new OsuSpriteText
                        {
                            Shadow = false,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: FontSize * 0.75f, weight: FontWeight.SemiBold, fixedWidth: true),
                            AlwaysPresent = true,
                        },
                        drawableUsername = new DrawableUsername(message.Sender)
                        {
                            Width = UsernameWidth,
                            FontSize = FontSize,
                            AutoSizeAxes = Axes.Y,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Margin = new MarginPadding { Horizontal = Spacing },
                        },
                        drawableContentFlow = new LinkFlowContainer(styleMessageContent)
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.Prefer24HourTime, prefer24HourTime);
            prefer24HourTime.BindValueChanged(_ => updateTimestamp());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableTimestamp.Colour = colourProvider?.Background1 ?? Colour4.White;

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
                    Colour = drawableUsername.Colour.Darken(1f),
                    Depth = float.MaxValue,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                });
            }

            highlight.FadeTo(0.5f).FadeOut(1500, Easing.InQuint);
            highlight.Expire();
        }

        private void styleMessageContent(SpriteText text)
        {
            text.Shadow = false;
            text.Font = text.Font.With(size: FontSize, italics: Message.IsAction);

            bool messageHasColour = Message.IsAction && !string.IsNullOrEmpty(message.Sender.Colour);
            text.Colour = messageHasColour ? Color4Extensions.FromHex(message.Sender.Colour) : colourProvider?.Content1 ?? Colour4.White;
        }

        private void updateMessageContent()
        {
            this.FadeTo(message is LocalEchoMessage ? 0.4f : 1.0f, 500, Easing.OutQuint);
            drawableTimestamp.FadeTo(message is LocalEchoMessage ? 0 : 1, 500, Easing.OutQuint);

            updateTimestamp();
            drawableUsername.Text = $@"{message.Sender.Username}";

            // remove non-existent channels from the link list
            message.Links.RemoveAll(link => link.Action == LinkAction.OpenChannel && chatManager?.AvailableChannels.Any(c => c.Name == link.Argument.ToString()) != true);

            drawableContentFlow.Clear();
            drawableContentFlow.AddLinks(message.DisplayContent, message.Links);
        }

        private void updateTimestamp()
        {
            drawableTimestamp.Text = prefer24HourTime.Value
                ? $@"{message.Timestamp.LocalDateTime:HH:mm:ss}"
                : $@"{message.Timestamp.LocalDateTime:hh:mm:ss tt}";
        }

        private partial class DrawableUsername : OsuClickableContainer, IHasContextMenu
        {
            public new Color4 Colour { get; private set; }

            public float FontSize
            {
                set => drawableText.Font = OsuFont.GetFont(size: value, weight: FontWeight.Bold, italics: true);
            }

            public LocalisableString Text
            {
                set => drawableText.Text = value;
            }

            public override float Width
            {
                get => base.Width;
                set => base.Width = drawableText.MaxWidth = value;
            }

            [Resolved(canBeNull: false)]
            private IAPIProvider api { get; set; } = null!;

            [Resolved(canBeNull: false)]
            private OsuColour osuColours { get; set; } = null!;

            [Resolved]
            private ChannelManager? chatManager { get; set; }

            [Resolved]
            private ChatOverlay? chatOverlay { get; set; }

            [Resolved]
            private UserProfileOverlay? profileOverlay { get; set; }

            private readonly APIUser user;
            private readonly OsuSpriteText drawableText;

            private readonly Drawable colouredDrawable;

            public DrawableUsername(APIUser user)
            {
                this.user = user;

                Action = openUserProfile;

                drawableText = new OsuSpriteText
                {
                    Shadow = false,
                    Truncate = true,
                    EllipsisString = "…",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                };

                if (string.IsNullOrWhiteSpace(user.Colour))
                {
                    Colour = default_colours[user.Id % default_colours.Length];

                    Child = colouredDrawable = drawableText;
                }
                else
                {
                    Colour = Color4Extensions.FromHex(user.Colour);

                    Child = new Container
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
                            Children = new[]
                            {
                                colouredDrawable = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Left = 4, Right = 4, Bottom = 1, Top = -2 },
                                    Child = drawableText,
                                }
                            }
                        }
                    };
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                drawableText.Colour = osuColours.ChatBlue;
                colouredDrawable.Colour = Colour;
            }

            public MenuItem[] ContextMenuItems
            {
                get
                {
                    if (user.Equals(APIUser.SYSTEM_USER))
                        return Array.Empty<MenuItem>();

                    List<MenuItem> items = new List<MenuItem>
                    {
                        new OsuMenuItem("View Profile", MenuItemType.Highlighted, openUserProfile)
                    };

                    if (!user.Equals(api.LocalUser.Value))
                        items.Add(new OsuMenuItem("Start Chat", MenuItemType.Standard, openUserChannel));

                    return items.ToArray();
                }
            }

            private void openUserChannel()
            {
                chatManager?.OpenPrivateChannel(user);
                chatOverlay?.Show();
            }

            private void openUserProfile()
            {
                profileOverlay?.ShowUser(user);
            }

            protected override bool OnHover(HoverEvent e)
            {
                colouredDrawable.FadeColour(Colour.Lighten(0.4f), 150, Easing.OutQuint);

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                colouredDrawable.FadeColour(Colour, 250, Easing.OutQuint);
            }

            private static readonly Color4[] default_colours =
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
}
