// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public abstract partial class ReportPopover<T> : OsuPopover
        where T : struct, Enum
    {
        public Action<T, string>? Action;

        private OsuEnumDropdown<T> reasonDropdown = null!;
        private OsuTextBox commentsTextBox = null!;
        private RoundedButton submitButton = null!;

        private readonly LocalisableString header;

        protected ReportPopover(LocalisableString headerString)
        {
            header = headerString;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
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
                        Text = header,
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
                        Child = reasonDropdown = new OsuEnumDropdown<T>
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
                    submitButton = new RoundedButton
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

            commentsTextBox.Current.BindValueChanged(_ => updateStatus());

            reasonDropdown.Current.BindValueChanged(_ => updateStatus());

            updateStatus();
        }

        private void updateStatus()
        {
            submitButton.Enabled.Value = !string.IsNullOrWhiteSpace(commentsTextBox.Current.Value) || CheckCanSubmitEmptyComment(reasonDropdown.Current.Value);
        }

        protected virtual bool CheckCanSubmitEmptyComment(T reason)
        {
            return false;
        }
    }
}
