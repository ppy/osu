// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public partial class ModMapInfoContainer : Container
    {
        private Container content;
        private Container innerContent;

        private Box background;
        private Box innerBackground;

        private StarRatingDisplay starRatingDisplay;
        private BPMDisplay bpmDisplay;

        private VerticalAttributeDisplay circleSizeDisplay;
        private VerticalAttributeDisplay drainRateDisplay;
        private VerticalAttributeDisplay approachRateDisplay;
        private VerticalAttributeDisplay overallDifficultyDisplay;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        [Resolved]
        private Bindable<BeatmapShortInfo> adjustedInfo { get; set; }

        public ModMapInfoContainer()
        {
            // values as ModSelectOverlay footer buttons
            const float shear = ShearedOverlayContainer.SHEAR;
            const float corner_radius = 7;
            const float border_thickness = 2;

            InternalChild = content = new InputBlockingContainer
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                AutoSizeAxes = Axes.X,
                Height = 50, // as ModSelectOverlay footer buttons
                Shear = new Vector2(shear, 0),
                CornerRadius = corner_radius,
                BorderThickness = border_thickness,
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer // divide inner and outer content
                    {
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            innerContent = new Container
                            {
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                BorderThickness = border_thickness,
                                CornerRadius = corner_radius,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    innerBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    new FillFlowContainer // actual inner content
                                    {
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.X,
                                        Direction = FillDirection.Horizontal,
                                        Margin = new MarginPadding { Horizontal = 15 },
                                        Children = new Drawable[]
                                        {
                                            new Container // wrap to reserve space for StarRatingDisplay
                                            {
                                                Width = 70, // can be up to 70px on extra high SR
                                                Child = starRatingDisplay = new StarRatingDisplay(default, animated: true)
                                                {
                                                    Origin = Anchor.CentreLeft,
                                                    Anchor = Anchor.CentreLeft,
                                                    Shear = new Vector2(-shear, 0),
                                                }
                                            },
                                            new Container // wrap to reserve space for BPM
                                            {
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Width = 70,
                                                Child = bpmDisplay = new BPMDisplay
                                                {
                                                    Origin = Anchor.CentreLeft,
                                                    Anchor = Anchor.CentreLeft,
                                                    Shear = new Vector2(-shear, 0),
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new FillFlowContainer<VerticalAttributeDisplay> // outer content
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Children = new[]
                                {
                                    circleSizeDisplay = new VerticalAttributeDisplay("CS"),
                                    drainRateDisplay = new VerticalAttributeDisplay("HP"),
                                    approachRateDisplay = new VerticalAttributeDisplay("AR"),
                                    overallDifficultyDisplay = new VerticalAttributeDisplay("OD"),
                                }
                            }
                        }
                    }
                }
            };
        }
        protected override void LoadComplete()
        {
            adjustedInfo.BindValueChanged(e => { UpdateValues(); }, true);

            background.Colour = colourProvider.Background4;
            innerBackground.Colour = colourProvider.Background3;
            Color4 glow_colour = colourProvider.Background1;

            content.BorderColour = ColourInfo.GradientVertical(background.Colour, glow_colour);
            innerContent.BorderColour = ColourInfo.GradientVertical(innerBackground.Colour, glow_colour);
        }

        public void UpdateValues()
        {
            if (adjustedInfo.Value == null) return;

            starRatingDisplay.Current.Value = adjustedInfo.Value.StarDifficulty;
            bpmDisplay.Current.Value = adjustedInfo.Value.BPM;

            circleSizeDisplay.Current.Value = adjustedInfo.Value.CircleSize;
            drainRateDisplay.Current.Value = adjustedInfo.Value.DrainRate;
            approachRateDisplay.Current.Value = adjustedInfo.Value.ApproachRate;
            overallDifficultyDisplay.Current.Value = adjustedInfo.Value.OverallDifficulty;
        }

        private partial class BPMDisplay : RollingCounter<double>
        {
            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString("0 BPM");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 20, weight: FontWeight.SemiBold)
            };
        }
    }
}
