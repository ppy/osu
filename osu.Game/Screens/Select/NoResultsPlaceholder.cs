// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select
{
    public class NoResultsPlaceholder : CompositeDrawable
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
                filter = value;
                Scheduler.AddOnce(updateText);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Masking = true;
            CornerRadius = 10;

            Width = 300;
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
                    Icon = FontAwesome.Regular.QuestionCircle,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding(10),
                    Size = new Vector2(50),
                },
                textFlow = new LinkFlowContainer
                {
                    Y = 70,
                    Padding = new MarginPadding(10),
                    TextAnchor = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }
            };
        }

        public override void Show()
        {
            this.FadeIn(600, Easing.OutQuint);

            this.ScaleTo(0.8f)
                .ScaleTo(1f, 1000, Easing.OutElastic);

            Scheduler.AddOnce(updateText);
        }

        public override void Hide()
        {
            this.FadeOut(200, Easing.OutQuint);
        }

        private void updateText()
        {
            textFlow.Clear();

            if (beatmaps.QueryBeatmapSet(s => !s.Protected && !s.DeletePending) == null)
            {
                textFlow.AddParagraph("No beatmaps found!");
                textFlow.AddParagraph(string.Empty);

                textFlow.AddParagraph("Consider running the ");
                textFlow.AddLink("first run setup", () => firstRunSetupOverlay?.Show());
                textFlow.AddText(" to load or import some beatmaps!");
            }
            else
            {
                textFlow.AddParagraph("No beatmaps match your filter criteria!");
                textFlow.AddParagraph(string.Empty);

                if (string.IsNullOrEmpty(filter?.SearchText))
                {
                    // TODO: Add realm queries to hint at which ruleset results are available in (and allow clicking to switch).
                    // TODO: Make this message more certain by ensuring the osu! beatmaps exist before suggesting.
                    if (filter?.Ruleset.OnlineID > 0 && !filter.AllowConvertedBeatmaps)
                    {
                        textFlow.AddParagraph("Beatmaps may be available by ");
                        textFlow.AddLink("enabling automatic conversion", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));
                        textFlow.AddText("!");
                    }
                }
                else
                {
                    textFlow.AddParagraph("You can try ");
                    textFlow.AddLink("searching online", LinkAction.SearchBeatmapSet, filter.SearchText);
                    textFlow.AddText(" for this query.");
                }
            }
        }
    }
}
