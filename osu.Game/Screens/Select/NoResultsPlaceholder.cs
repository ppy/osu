// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select
{
    public class NoResultsPlaceholder : CompositeDrawable
    {
        private FilterCriteria filter;

        private LinkFlowContainer textFlow;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved(CanBeNull = true)]
        private FirstRunSetupOverlay firstRunSetupOverlay { get; set; }

        public FilterCriteria Filter
        {
            get => filter;
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

                // TODO: hint when beatmaps are available in another ruleset
                // TODO: hint when beatmaps are available by toggling "show converted".
                if (!string.IsNullOrEmpty(filter?.SearchText))
                {
                    textFlow.AddParagraph("You can try ");
                    textFlow.AddLink("searching online", LinkAction.SearchBeatmapSet, filter.SearchText);
                    textFlow.AddText(" for this query.");
                }
            }
        }
    }
}
