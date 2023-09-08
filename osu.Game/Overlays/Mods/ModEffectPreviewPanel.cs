// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;
using System.Threading;

namespace osu.Game.Overlays.Mods
{
    public partial class ModEffectPreviewPanel : CompositeDrawable
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

        public const float HEIGHT = 50; // as ModSelectOverlay footer buttons
        private const float transition_duration = 250;

        private IBeatmapInfo beatmapInfo = null!;

        public IBeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (value == beatmapInfo) return;

                beatmapInfo = value;
                updateStarDifficultyBind();
                UpdateValues();
            }
        }

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private CancellationTokenSource cancellationSource = null!;
        private IBindable<StarDifficulty?> starDifficulty = null!;

        public ModEffectPreviewPanel()
        {
            // values as ModSelectOverlay footer buttons
            const float shear = ShearedOverlayContainer.SHEAR;
            const float corner_radius = 7;
            const float border_thickness = 2;

            AutoSizeAxes = Axes.Both;
            InternalChild = content = new InputBlockingContainer
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                AutoSizeAxes = Axes.X,
                Height = HEIGHT,
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
                                    circleSizeDisplay = new VerticalAttributeDisplay("CS", "0.#"),
                                    drainRateDisplay = new VerticalAttributeDisplay("HP", "0.#"),
                                    approachRateDisplay = new VerticalAttributeDisplay("AR", "0.##"),
                                    overallDifficultyDisplay = new VerticalAttributeDisplay("OD", "0.##"),
                                }
                            }
                        }
                    }
                }
            };
        }
        protected override void LoadComplete()
        {
            background.Colour = colourProvider.Background4;
            innerBackground.Colour = colourProvider.Background3;
            Color4 glow_colour = colourProvider.Background1;

            content.BorderColour = ColourInfo.GradientVertical(background.Colour, glow_colour);
            innerContent.BorderColour = ColourInfo.GradientVertical(innerBackground.Colour, glow_colour);

            updateStarDifficultyBind();
        }
        private void updateStarDifficultyBind()
        {
            if (cancellationSource != null) cancellationSource.Cancel();
            starDifficulty = difficultyCache.GetBindableDifficulty(beatmapInfo, (cancellationSource = new CancellationTokenSource()).Token);
            starDifficulty.BindValueChanged(s =>
            {
                starRatingDisplay.Current.Value = s.NewValue ?? default;

                if (!starRatingDisplay.IsPresent)
                    starRatingDisplay.FinishTransforms(true);

                starRatingDisplay.FadeIn(transition_duration);
            });
        }
        public void UpdateValues()
        {
            if (beatmapInfo == null) return;

            double rate = 1;
            foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                rate = mod.ApplyToRate(0, rate);

            bpmDisplay.Current.Value = beatmapInfo.BPM * rate;

            BeatmapDifficulty adjustedDifficulty = new BeatmapDifficulty(BeatmapInfo.Difficulty);
            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(adjustedDifficulty);

            circleSizeDisplay.Current.Value = adjustedDifficulty.CircleSize;
            drainRateDisplay.Current.Value = adjustedDifficulty.DrainRate;
            approachRateDisplay.Current.Value = adjustedDifficulty.ApproachRate;
            overallDifficultyDisplay.Current.Value = adjustedDifficulty.OverallDifficulty;
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
