// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
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
        private OsuSpriteText bpm;
        private OsuSpriteText maxCombo;
        private OsuSpriteText length;

        private FillFlowContainer difficultyFillFlowContainer;
        private FillFlowContainer miscFillFlowContainer;

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
                        difficultyFillFlowContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                circleSize = new OsuSpriteText
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
                                approachRate = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                                overallDifficulty = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                            }
                        },
                        // Misc stats
                        miscFillFlowContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                length = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 14),
                                },
                                bpm = new OsuSpriteText
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

            displayedContent = content;

            // Header row
            starRating.Current.BindTarget = displayedContent.Difficulty;
            difficultyName.Text = displayedContent.BeatmapInfo.DifficultyName;

            // Don't show difficulty stats if showExtendedTooltip is false
            if (!displayedContent.ShowExtendedTooltip)
            {
                difficultyFillFlowContainer.Hide();
                miscFillFlowContainer.Hide();
                return;
            }

            // Show the difficulty stats if showExtendedTooltip is true
            difficultyFillFlowContainer.Show();
            miscFillFlowContainer.Show();

            double rate = 1;

            if (displayedContent.Mods != null)
            {
                foreach (var mod in displayedContent.Mods.OfType<IApplicableToRate>())
                    rate = mod.ApplyToRate(0, rate);
            }

            double bpmAdjusted = displayedContent.BeatmapInfo.BPM * rate;

            BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(displayedContent.BeatmapInfo.Difficulty);

            if (displayedContent.Mods != null)
            {
                foreach (var mod in displayedContent.Mods.OfType<IApplicableToDifficulty>())
                {
                    mod.ApplyToDifficulty(originalDifficulty);
                }
            }

            Ruleset ruleset = displayedContent.Ruleset.CreateInstance();
            BeatmapDifficulty adjustedDifficulty = ruleset.GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);

            // Difficulty row
            circleSize.Text = "CS: " + adjustedDifficulty.CircleSize.ToString("0.##");
            drainRate.Text = " HP: " + adjustedDifficulty.DrainRate.ToString("0.##");
            approachRate.Text = " AR: " + adjustedDifficulty.ApproachRate.ToString("0.##");
            overallDifficulty.Text = " OD: " + adjustedDifficulty.OverallDifficulty.ToString("0.##");

            // Misc row
            length.Text = "Length: " + TimeSpan.FromMilliseconds(displayedContent.BeatmapInfo.Length / rate).ToString("mm\\:ss");
            bpm.Text = " BPM: " + Math.Round(bpmAdjusted, 0);
            maxCombo.Text = " Max Combo: " + displayedContent.BeatmapInfo.TotalObjectCount;
        }

        public void Move(Vector2 pos) => Position = pos;

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
    }

    internal class DifficultyIconTooltipContent
    {
        public readonly IBeatmapInfo BeatmapInfo;
        public readonly IBindable<StarDifficulty> Difficulty;
        public readonly IRulesetInfo Ruleset;
        public readonly Mod[] Mods;
        public readonly bool ShowExtendedTooltip;

        public DifficultyIconTooltipContent(IBeatmapInfo beatmapInfo, IBindable<StarDifficulty> difficulty, IRulesetInfo rulesetInfo, Mod[] mods, bool showExtendedTooltip = false)
        {
            BeatmapInfo = beatmapInfo;
            Difficulty = difficulty;
            Ruleset = rulesetInfo;
            Mods = mods;
            ShowExtendedTooltip = showExtendedTooltip;
        }
    }
}
