// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osuTK.Graphics;
using Message = osu.Game.Online.Chat.Message;

namespace osu.Game.Overlays.Chat
{
    public partial class ChatLine : CompositeDrawable, IHasPopover
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

        private OsuSpriteText drawableTimestamp = null!;

        private DrawableChatUsername drawableUsername = null!;

        private LinkFlowContainer drawableContentFlow = null!;

        private readonly Bindable<bool> prefer24HourTime = new Bindable<bool>();

        private Container? highlight;

        /// <summary>
        /// The colour used to paint the author's username.
        /// </summary>
        /// <remarks>
        /// The colour can be set explicitly by consumers via the property initialiser.
        /// If unspecified, the colour is by default initialised to:
        /// <list type="bullet">
        /// <item><see cref="APIUser.Colour">message.Sender.Colour</see>, if non-empty,</item>
        /// <item>a random colour from <see cref="default_username_colours"/> if the above is empty.</item>
        /// </list>
        /// </remarks>
        public Color4 UsernameColour { get; init; }

        public ChatLine(Message message)
        {
            Message = message;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            // initialise using sane defaults.
            // consumers can use the initialiser of `UsernameColour` to override this if they wish to.
            UsernameColour = !string.IsNullOrEmpty(message.Sender.Colour)
                ? Color4Extensions.FromHex(message.Sender.Colour)
                : default_username_colours[message.SenderId % default_username_colours.Length];
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.Prefer24HourTime, prefer24HourTime);
            prefer24HourTime.BindValueChanged(_ => updateTimestamp());

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
                        drawableUsername = new DrawableChatUsername(message.Sender)
                        {
                            Width = UsernameWidth,
                            FontSize = FontSize,
                            AutoSizeAxes = Axes.Y,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Margin = new MarginPadding { Horizontal = Spacing },
                            AccentColour = UsernameColour,
                            Inverted = !string.IsNullOrEmpty(message.Sender.Colour),
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableTimestamp.Colour = colourProvider?.Background1 ?? Colour4.White;

            updateMessageContent();
            FinishTransforms(true);

            if (this.FindClosestParent<PopoverContainer>() != null)
            {
                // This guards against cases like in-game chat where there's no available popover container.
                // There may be a future where a global one becomes available, at which point this code may be unnecessary.
                //
                // See:
                // https://github.com/ppy/osu/pull/23698
                // https://github.com/ppy/osu/pull/14554
                drawableUsername.ReportRequested = this.ShowPopover;
            }
        }

        public Popover GetPopover() => new ReportChatPopover(message);

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
                    Colour = drawableUsername.AccentColour.Darken(1f),
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
            drawableTimestamp.Text = message.Timestamp.LocalDateTime.ToLocalisableString(prefer24HourTime.Value ? @"HH:mm:ss" : @"hh:mm:ss tt");
        }

        private static readonly Color4[] default_username_colours =
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
