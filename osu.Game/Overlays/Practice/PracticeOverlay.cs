// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Localisation;
using osu.Game.Overlays.Practice.PracticeOverlayComponents;

namespace osu.Game.Overlays.Practice
{
    public class PracticeOverlay : ShearedOverlayContainer
    {
        [Resolved]
        private PracticePlayer player { get; set; } = null!;

        private PracticeGameplayPreview preview = null!;

        private PracticeSegmentSliderComponent practiceSlider = null!;

        public PracticeOverlay()
            : base(OverlayColourScheme.Green)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Header.Title = PracticeOverlayStrings.PracticeOverlayHeaderTitle;
            Header.Description = PracticeOverlayStrings.PracticeOverlayHeaderDescription;

            MainAreaContent.Add(preview = new PracticeGameplayPreview());

            FooterContent.Add(footerContent());

            practiceSlider.customStartTime.ValueChanged += time => preview.SeekTime.Value = time.NewValue;
            practiceSlider.customEndTime.ValueChanged += time => preview.SeekTime.Value = time.NewValue;
        }

        private Drawable footerContent()
        {
            return new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 100),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 100),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container(),
                        practiceSlider = new PracticeSegmentSliderComponent { RelativeSizeAxes = Axes.Both },
                        new ShearedButton
                        {
                            Text = "Play",
                            Padding = new MarginPadding(10),
                            Action = () => player.Restart(true)
                        }
                    },
                }
            };
        }
    }
}
