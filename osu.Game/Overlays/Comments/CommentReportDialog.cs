// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Comments
{
    public class CommentReportDialog : VisibilityContainer
    {
        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            RelativeSizeAxes = Axes.Both;

            Child = new Container
            {
                Masking = true,
                CornerRadius = 10,
                Width = 500,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background6,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(10),
                        Children = new[]
                        {
                            new CircularContainer
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Masking = true,
                                Size = new Vector2(100f),
                                BorderColour = Color4.White,
                                BorderThickness = 5f,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black.Opacity(0),
                                    },
                                    new SpriteIcon
                                    {
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        Icon = FontAwesome.Solid.ExclamationTriangle,
                                        Size = new Vector2(50),
                                    },
                                },
                            },
                            new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 25))
                            {
                                Text = UsersStrings.ReportTitle("the comment"),
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                TextAnchor = Anchor.TopCentre,
                            },
                            Empty().With(d => d.Height = 10),
                            new OsuSpriteText
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Text = UsersStrings.ReportReason,
                                Font = OsuFont.Torus.With(size: 20),
                            },
                            new OsuEnumDropdown<CommentReportReason>
                            {
                                RelativeSizeAxes = Axes.X
                            },
                            new OsuSpriteText
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Text = UsersStrings.ReportComments,
                                Font = OsuFont.Torus.With(size: 20),
                            },
                            new OsuTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                                PlaceholderText = UsersStrings.ReportPlaceholder
                            },
                            new RoundedButton
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Width = 200,
                                BackgroundColour = colours.Red3,
                                Text = UsersStrings.ReportActionsSend,
                                Action = send,
                                Margin = new MarginPadding { Bottom = 5, Top = 10 },
                            },
                            new RoundedButton
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Width = 200,
                                Text = UsersStrings.ReportActionsCancel,
                                Action = () =>
                                {
                                    Hide();
                                    Expire();
                                }
                            }
                        }
                    }
                }
            };
        }

        private void send()
        {
        }

        protected override void PopIn()
        {
        }

        protected override void PopOut()
        {
        }
    }
}
