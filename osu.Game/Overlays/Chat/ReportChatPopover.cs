// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Chat
{
    public partial class ReportChatPopover : OsuPopover
    {
        public Action<ChatReportReason, string>? Action;

        private readonly APIUser? user;

        private OsuEnumDropdown<ChatReportReason> reasonDropdown = null!;
        private OsuTextBox commentsTextBox = null!;

        public ReportChatPopover(APIUser? user)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            Child = new ReverseChildIDFillFlowContainer<Drawable>
            {
                Direction = FillDirection.Vertical,
                Width = 500,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(7),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Icon = FontAwesome.Solid.ExclamationTriangle,
                        Size = new Vector2(36),
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = ReportStrings.UserTitle(user?.Username ?? @"Someone"),
                        Font = OsuFont.Torus.With(size: 25),
                        Margin = new MarginPadding { Bottom = 10 }
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = UsersStrings.ReportReason,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Child = reasonDropdown = new OsuEnumDropdown<ChatReportReason>
                        {
                            RelativeSizeAxes = Axes.X
                        }
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Text = UsersStrings.ReportComments,
                    },
                    commentsTextBox = new OsuTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        PlaceholderText = UsersStrings.ReportPlaceholder,
                    },
                    new RoundedButton
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Width = 200,
                        BackgroundColour = colours.Red3,
                        Text = UsersStrings.ReportActionsSend,
                        Action = () =>
                        {
                            Action?.Invoke(reasonDropdown.Current.Value, commentsTextBox.Text);
                            this.HidePopover();
                        },
                        Margin = new MarginPadding { Bottom = 5, Top = 10 },
                    }
                }
            };
        }
    }
}
