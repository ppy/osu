// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class NoResultsPlaceholder : VisibilityContainer
    {
        public Action? RequestClearFilterText { get; init; }

        private FilterCriteria? filter;

        private LinkFlowContainer textFlow = null!;

        private GhostIcon icon = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private FirstRunSetupOverlay? firstRunSetupOverlay { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        protected override bool StartHidden => true;

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
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 300,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding(10),
                            Size = new Vector2(50),
                            Child = icon = new GhostIcon
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.Style.Title,
                            Text = "No matching beatmaps"
                        },
                        textFlow = new LinkFlowContainer
                        {
                            Alpha = 0,
                            AlwaysPresent = true,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Padding = new MarginPadding { Top = 20 },
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            icon.Loop(t =>
                t.MoveToY(-10, 2000, Easing.InOutSine)
                 .Then()
                 .MoveToY(0, 2000, Easing.InOutSine)
            );
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
                .ScaleTo(1f, 1000, Easing.OutQuint);

            textFlow.FadeInFromZero(800, Easing.OutQuint);

            textFlow.Clear();

            if (beatmaps.QueryBeatmapSet(s => !s.Protected && !s.DeletePending) == null)
            {
                addBulletPoint();
                textFlow.AddText("Consider running the \"");
                textFlow.AddLink(FirstRunSetupOverlayStrings.FirstRunSetupTitle, () => firstRunSetupOverlay?.Show());
                textFlow.AddText("\" to download or import some beatmaps!");
            }
            else
            {
                textFlow.AddParagraph("No beatmaps match your filter criteria!");
                textFlow.AddParagraph(string.Empty);

                if (!string.IsNullOrEmpty(filter?.SearchText))
                {
                    addBulletPoint();
                    textFlow.AddText("Try ");
                    textFlow.AddLink("clearing", () =>
                    {
                        RequestClearFilterText?.Invoke();
                    });

                    textFlow.AddText(" your current search criteria.");
                }

                if (filter?.UserStarDifficulty.HasFilter == true)
                {
                    addBulletPoint();
                    textFlow.AddText("Try ");
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
                    addBulletPoint();
                    textFlow.AddText("Try ");
                    textFlow.AddLink("enabling ", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));
                    textFlow.AddText("automatic conversion!");
                }
            }

            if (!string.IsNullOrEmpty(filter?.SearchText))
            {
                addBulletPoint();
                textFlow.AddText("Try ");
                textFlow.AddLink("searching online", LinkAction.SearchBeatmapSet, filter.SearchText);
                textFlow.AddText($" for \"{filter.SearchText}\".");
            }
            // TODO: add clickable link to reset criteria.
        }

        private void addBulletPoint()
        {
            textFlow.NewLine();
            textFlow.AddIcon(FontAwesome.Solid.Circle, i =>
            {
                i.Padding = new MarginPadding { Top = 24, Right = 15 };
                i.Scale *= 0.3f;
            });
        }
    }
}
