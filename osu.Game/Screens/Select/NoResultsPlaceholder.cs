// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select
{
    public partial class NoResultsPlaceholder : VisibilityContainer
    {
        private FilterCriteria? filter;

        private LinkFlowContainer textFlow = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private FirstRunSetupOverlay? firstRunSetupOverlay { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        public FilterCriteria Filter
        {
            set
            {
                if (filter == value)
                    return;

                filter = value;
                Scheduler.AddOnce(updateText);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Masking = true;
            CornerRadius = 10;

            Width = 400;
            AutoSizeAxes = Axes.Y;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray2,
                    RelativeSizeAxes = Axes.Both,
                },
                new SpriteIcon
                {
                    Icon = FontAwesome.Regular.SadTear,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding(10),
                    Size = new Vector2(50),
                },
                textFlow = new LinkFlowContainer
                {
                    Y = 60,
                    Padding = new MarginPadding(10),
                    TextAnchor = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            };
        }

        protected override void PopIn()
        {
            this.FadeIn(600, Easing.OutQuint);

            Scheduler.AddOnce(updateText);
        }

        protected override void PopOut()
        {
            this.FadeOut(200, Easing.OutQuint);
        }

        private void updateText()
        {
            // TODO: Refresh this text when new beatmaps are imported. Right now it won't get up-to-date suggestions.

            // Bounce should play every time the filter criteria is updated.
            this.ScaleTo(0.9f)
                .ScaleTo(1f, 1000, Easing.OutElastic);

            textFlow.Clear();

            if (beatmaps.QueryBeatmapSet(s => !s.Protected && !s.DeletePending) == null)
            {
                textFlow.AddParagraph("No beatmaps found!");
                textFlow.AddParagraph(string.Empty);

                textFlow.AddParagraph("- Consider running the \"");
                textFlow.AddLink(FirstRunSetupOverlayStrings.FirstRunSetupTitle, () => firstRunSetupOverlay?.Show());
                textFlow.AddText("\" to download or import some beatmaps!");
            }
            else
            {
                textFlow.AddParagraph("No beatmaps match your filter criteria!");
                textFlow.AddParagraph(string.Empty);

                if (filter?.UserStarDifficulty.HasFilter == true)
                {
                    textFlow.AddParagraph("- Try ");
                    textFlow.AddLink("removing", () =>
                    {
                        config.SetValue(OsuSetting.DisplayStarsMinimum, 0.0);
                        config.SetValue(OsuSetting.DisplayStarsMaximum, 10.1);
                    });

                    string lowerStar = $"{filter.UserStarDifficulty.Min ?? 0:N1}";
                    string upperStar = filter.UserStarDifficulty.Max == null ? "∞" : $"{filter.UserStarDifficulty.Max:N1}";

                    textFlow.AddText($" the {lowerStar} - {upperStar} star difficulty filter.");
                }

                // TODO: Add realm queries to hint at which ruleset results are available in (and allow clicking to switch).
                // TODO: Make this message more certain by ensuring the osu! beatmaps exist before suggesting.
                if (filter?.Ruleset?.OnlineID != 0 && filter?.AllowConvertedBeatmaps == false)
                {
                    textFlow.AddParagraph("- Try");
                    textFlow.AddLink(" enabling ", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));
                    textFlow.AddText("automatic conversion!");
                }
            }

            if (!string.IsNullOrEmpty(filter?.SearchText))
            {
                textFlow.AddParagraph("- Try ");
                textFlow.AddLink("searching online", LinkAction.SearchBeatmapSet, filter.SearchText);
                textFlow.AddText($" for \"{filter.SearchText}\".");
            }
            // TODO: add clickable link to reset criteria.
        }
    }
}
