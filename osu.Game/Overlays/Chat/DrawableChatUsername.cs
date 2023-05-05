// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;
using ChatStrings = osu.Game.Localisation.ChatStrings;

namespace osu.Game.Overlays.Chat
{
    public partial class DrawableChatUsername : OsuClickableContainer, IHasContextMenu
    {
        public Action? ReportRequested;

        public Color4 AccentColour { get; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            colouredDrawable.ReceivePositionalInputAt(screenSpacePos);

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

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private ChannelManager? chatManager { get; set; }

        [Resolved(canBeNull: true)]
        private ChatOverlay? chatOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private UserProfileOverlay? profileOverlay { get; set; }

        [Resolved]
        private Bindable<Channel?>? currentChannel { get; set; }

        private readonly APIUser user;
        private readonly OsuSpriteText drawableText;

        private readonly Drawable colouredDrawable;

        public DrawableChatUsername(APIUser user)
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
                AccentColour = default_colours[user.Id % default_colours.Length];

                Add(colouredDrawable = drawableText);
            }
            else
            {
                AccentColour = Color4Extensions.FromHex(user.Colour);

                Add(new Container
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
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableText.Colour = colours.ChatBlue;
            colouredDrawable.Colour = AccentColour;
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (user.Equals(APIUser.SYSTEM_USER))
                    return Array.Empty<MenuItem>();

                List<MenuItem> items = new List<MenuItem>
                {
                    new OsuMenuItem(ContextMenuStrings.ViewProfile, MenuItemType.Highlighted, openUserProfile)
                };

                if (!user.Equals(api.LocalUser.Value))
                    items.Add(new OsuMenuItem(UsersStrings.CardSendMessage, MenuItemType.Standard, openUserChannel));

                if (currentChannel?.Value != null)
                {
                    items.Add(new OsuMenuItem(ChatStrings.MentionUser, MenuItemType.Standard, () =>
                    {
                        currentChannel.Value.TextBoxMessage.Value += $"@{user.Username} ";
                    }));
                }

                if (!user.Equals(api.LocalUser.Value))
                    items.Add(new OsuMenuItem("Report", MenuItemType.Destructive, ReportRequested));

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
            colouredDrawable.FadeColour(AccentColour.Lighten(0.6f), 30, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            colouredDrawable.FadeColour(AccentColour, 800, Easing.OutQuint);
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
