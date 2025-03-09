// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.Ranking.Statistics
{
    public partial class ProgressiveStatistic : CompositeDrawable
    {
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);
        private const float progress_circle_radius = 0.2f;
        private const int progress_circle_offset = 4;

        private CircularProgress progressCircle = null!;

        private OsuSpriteText attribute = null!;
        private OsuSpriteText value = null!;

        private readonly float achievedValue;
        private readonly float progressTo;

        public ProgressiveStatistic(float achievedValue, float maximumValue)
        {
            progressTo = achievedValue / maximumValue;
            this.achievedValue = achievedValue;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Height = 70,
                    Width = 205,
                    CornerRadius = 10,
                    CornerExponent = 2,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background6,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 8, Horizontal = 10 },
                            Children = new Drawable[]
                            {
                                attribute = new OsuSpriteText
                                {
                                    Colour = Colour4.White,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold)
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                            Width = 26,
                                            Height = 26 + progress_circle_offset,
                                            Padding = new MarginPadding { Bottom = progress_circle_offset }, // slightly nudge circle upwards
                                            Children = new Drawable[]
                                            {
                                                new CircularProgress
                                                {
                                                    Name = "Background circle",
                                                    Colour = Colour4.Black,
                                                    RelativeSizeAxes = Axes.Both,
                                                    InnerRadius = progress_circle_radius,
                                                    Progress = 1,
                                                    RoundedCaps = true,
                                                },
                                                progressCircle = new CircularProgress
                                                {
                                                    Name = "Progress circle",
                                                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#7CF6FF"), Color4Extensions.FromHex("#BAFFA9")),
                                                    RelativeSizeAxes = Axes.Both,
                                                    InnerRadius = progress_circle_radius,
                                                    RoundedCaps = true,
                                                },
                                            }
                                        },
                                        value = new OsuSpriteText
                                        {
                                            Colour = ColourInfo.GradientVertical(Colour4.White, Color4Extensions.FromHex("#B2E5FE")),
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.GetFont(size: 40, weight: FontWeight.Regular)
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(0).Then().ScaleTo(1, 200, Easing.OutQuint);

            using (BeginDelayedSequence(150))
            {
                if (progressTo != 1 && progressTo >= 0.96)
                {
                    progressCircle.ProgressTo(0.96, 3000, Easing.OutPow10);
                }
                else
                {
                    progressCircle.ProgressTo(progressTo, 7000, Easing.OutPow10);
                }

                attribute.Text = "ATTRIBUTE";
                value.Text = $"{achievedValue}".ToString();
            }
        }
    }
}
