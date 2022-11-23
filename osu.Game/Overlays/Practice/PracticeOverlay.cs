// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Localisation;
using osu.Game.Overlays.Practice.PracticeOverlayComponents;
using osu.Game.Rulesets.UI;

namespace osu.Game.Overlays.Practice
{
    public class PracticeOverlay : ShearedOverlayContainer
    {
        [Resolved]
        private PracticePlayer player { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private DrawableRuleset? drawableRuleset { get; set; }

        private PracticeGameplayPreview preview = null!;

        private PracticeSegmentSliderComponent practiceSlider = null!;

        public PracticeOverlay()
            : base(OverlayColourScheme.Green)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            double? lastTime = drawableRuleset?.Objects.Last().StartTime;

            Header.Title = PracticeOverlayStrings.PracticeOverlayHeaderTitle;
            Header.Description = PracticeOverlayStrings.PracticeOverlayHeaderDescription;

            MainAreaContent.Add(preview = new PracticeGameplayPreview());

            FooterContent.Add(footerContent());

            practiceSlider.CustomStart.ValueChanged += startPercent => preview.SeekTo(startPercent.NewValue * lastTime!.Value);
            practiceSlider.CustomEnd.ValueChanged += endPercent => preview.SeekTo(endPercent.NewValue * lastTime!.Value);
        }

        private Drawable footerContent()
        {
            return new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 150),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 150),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container(),
                        practiceSlider = new PracticeSegmentSliderComponent { RelativeSizeAxes = Axes.Both },
                        new ShearedButton(150)
                        {
                            Y = -5,
                            Text = "Play",
                            LighterColour = ColourProvider.Colour1,
                            DarkerColour = ColourProvider.Colour3,
                            Action = () => player.Restart(true)
                        }
                    },
                }
            };
        }
    }
}
