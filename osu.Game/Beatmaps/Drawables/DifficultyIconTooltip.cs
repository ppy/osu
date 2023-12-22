// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    internal partial class DifficultyIconTooltip : VisibilityContainer, ITooltip<DifficultyIconTooltipContent>
    {
        private OsuSpriteText difficultyName;
        private StarRatingDisplay starRating;
        private OsuSpriteText overallDifficulty;
        private OsuSpriteText drainRate;
        private OsuSpriteText circleSize;
        private OsuSpriteText approachRate;
        private OsuSpriteText BPM;
        private OsuSpriteText maxCombo;
        private OsuSpriteText length;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            Children = new Drawable[]
            {
                new Box
                {
                    Alpha = 0.9f,
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both
                },
                // Headers
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 200,
                    AutoSizeEasing = Easing.OutQuint,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        difficultyName = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                        },
                        starRating = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        // Difficulty stats
                        new FillFlowContainer()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                                },
                                overallDifficulty = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                                drainRate = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                                circleSize = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                                approachRate = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                            }
                        },
                        // Misc stats
                        new FillFlowContainer()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                                },
                                length = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                                BPM = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                                maxCombo = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                            }
                        }
                    }
                }
            };
        }

        private DifficultyIconTooltipContent displayedContent;

        public void SetContent(DifficultyIconTooltipContent content)
        {
            if (displayedContent != null)
                starRating.Current.UnbindFrom(displayedContent.Difficulty);

            // Header row
            displayedContent = content;
            starRating.Current.BindTarget = displayedContent.Difficulty;
            difficultyName.Text = displayedContent.BeatmapInfo.DifficultyName;

            // Difficulty row
            overallDifficulty.Text = "OD: " + displayedContent.BeatmapInfo.Difficulty.OverallDifficulty.ToString("0.##");
            drainRate.Text = "| HP: " + displayedContent.BeatmapInfo.Difficulty.DrainRate.ToString("0.##");
            circleSize.Text = "| CS: " + displayedContent.BeatmapInfo.Difficulty.CircleSize.ToString("0.##");
            approachRate.Text = "| AR: " + displayedContent.BeatmapInfo.Difficulty.ApproachRate.ToString("0.##");

            // Misc row
            length.Text = "Length: " + TimeSpan.FromMilliseconds(displayedContent.BeatmapInfo.Length).ToString("mm\\:ss");
            BPM.Text = "| BPM: " + displayedContent.BeatmapInfo.BPM;
            maxCombo.Text = "| Max Combo: " + displayedContent.BeatmapInfo.TotalObjectCount;
        }

        public void Move(Vector2 pos) => Position = pos;

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }

    internal class DifficultyIconTooltipContent
    {
        public readonly IBeatmapInfo BeatmapInfo;
        public readonly IBindable<StarDifficulty> Difficulty;

        public DifficultyIconTooltipContent(IBeatmapInfo beatmapInfo, IBindable<StarDifficulty> difficulty)
        {
            BeatmapInfo = beatmapInfo;
            Difficulty = difficulty;
        }
    }
}
