// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Localisation;
using osu.Game.Overlays.Practice.PracticeOverlayComponents;

namespace osu.Game.Overlays.Practice
{
    public partial class PracticeOverlay : ShearedOverlayContainer
    {
        public Action Restart = null!;
        public Action OnHide = null!;

        [Resolved]
        private PracticePlayerLoader playerLoader { get; set; } = null!;

        private readonly BindableNumber<double> customStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 0.001f
        };

        private readonly BindableNumber<double> customEnd = new BindableNumber<double>(100)
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 0.001f
        };

        private PracticeGameplayPreview preview = null!;

        public PracticeOverlay()
            : base(OverlayColourScheme.Green)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            var playableBeatmap = beatmap.Value.GetPlayableBeatmap(beatmap.Value.BeatmapInfo.Ruleset);

            createContent();

            double lastTime = playableBeatmap.HitObjects.Last().StartTime;
            double startTime = playableBeatmap.HitObjects.First().StartTime;

            customStart.BindTo(playerLoader.CustomStart);
            customEnd.BindTo(playerLoader.CustomEnd);

            customStart.BindValueChanged(startPercent =>
                preview.SeekTo(startPercent.NewValue / 100 * (lastTime - startTime)));
            customEnd.BindValueChanged(endPercent =>
                preview.SeekTo(endPercent.NewValue / 100 * (lastTime - startTime)));
        }

        private void createContent()
        {
            Add(new InputBlockingContainer
            {
                Depth = float.MaxValue,
                RelativeSizeAxes = Axes.Both,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black.Opacity(.8f)
                }
            });
            Header.Title = PracticeOverlayStrings.PracticeOverlayHeaderTitle;
            Header.Description = PracticeOverlayStrings.PracticeOverlayHeaderDescription;

            MainAreaContent.Add(preview = new PracticeGameplayPreview());

            FooterContent.Add(footerContent());
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
                        new RangeSlider
                        {
                            RelativeSizeAxes = Axes.Both,
                            LowerBound = customStart,
                            UpperBound = customEnd,
                            NubWidth = Nub.HEIGHT * 2,
                            MinRange = 0.01f,
                            DefaultStringLowerBound = "Start",
                            DefaultStringUpperBound = "End",
                            DefaultTooltipLowerBound = "Start of beatmap",
                            DefaultTooltipUpperBound = "End of beatmap"
                        },
                        new ShearedButton(150)
                        {
                            Y = -5,
                            Text = "Practice!",
                            LighterColour = ColourProvider.Colour1,
                            DarkerColour = ColourProvider.Colour3,
                            Action = () => Restart.Invoke()
                        }
                    }
                }
            };
        }

        protected override void PopOut()
        {
            base.PopOut();
            OnHide.Invoke();
        }
    }
}
