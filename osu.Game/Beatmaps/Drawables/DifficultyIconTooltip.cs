// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    internal partial class DifficultyIconTooltip : VisibilityContainer, ITooltip<DifficultyIconTooltipContent>
    {
        private OsuSpriteText difficultyName = null!;
        private StarRatingDisplay starRating = null!;
        private OsuSpriteText overallDifficulty = null!;
        private OsuSpriteText drainRate = null!;
        private OsuSpriteText circleSize = null!;
        private OsuSpriteText approachRate = null!;
        private OsuSpriteText bpm = null!;
        private OsuSpriteText length = null!;

        private FillFlowContainer difficultyFillFlowContainer = null!;
        private FillFlowContainer miscFillFlowContainer = null!;

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
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both
                },
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
                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold)
                        },
                        starRating = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
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
                                circleSize = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                                drainRate = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                                overallDifficulty = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                                approachRate = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                            }
                        },
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
                                length = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                                bpm = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                            }
                        }
                    }
                }
            };
        }

        private DifficultyIconTooltipContent? displayedContent;

        public void SetContent(DifficultyIconTooltipContent content)
        {
            if (displayedContent != null)
                starRating.Current.UnbindFrom(displayedContent.Difficulty);

            displayedContent = content;

            starRating.Current.BindTarget = displayedContent.Difficulty;
            difficultyName.Text = displayedContent.BeatmapInfo.DifficultyName;

            if (displayedContent.TooltipType == DifficultyIconTooltipType.StarRating)
            {
                difficultyFillFlowContainer.Hide();
                miscFillFlowContainer.Hide();
                return;
            }

            difficultyFillFlowContainer.Show();
            miscFillFlowContainer.Show();

            double rate = 1;
            if (displayedContent.Mods != null)
                rate = ModUtils.CalculateRateWithMods(displayedContent.Mods);

            double bpmAdjusted = displayedContent.BeatmapInfo.BPM * rate;

            BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(displayedContent.BeatmapInfo.Difficulty);

            if (displayedContent.Mods != null)
            {
                foreach (var mod in displayedContent.Mods.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(originalDifficulty);
            }

            Ruleset ruleset = displayedContent.Ruleset.CreateInstance();
            BeatmapDifficulty adjustedDifficulty = ruleset.GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);

            circleSize.Text = @"CS: " + adjustedDifficulty.CircleSize.ToString(@"0.##");
            drainRate.Text = @" HP: " + adjustedDifficulty.DrainRate.ToString(@"0.##");
            approachRate.Text = @" AR: " + adjustedDifficulty.ApproachRate.ToString(@"0.##");
            overallDifficulty.Text = @" OD: " + adjustedDifficulty.OverallDifficulty.ToString(@"0.##");

            length.Text = "Length: " + TimeSpan.FromMilliseconds(displayedContent.BeatmapInfo.Length / rate).ToString(@"mm\:ss");
            bpm.Text = " BPM: " + Math.Round(bpmAdjusted, 0);
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
        public readonly Mod[]? Mods;
        public readonly DifficultyIconTooltipType TooltipType;

        public DifficultyIconTooltipContent(IBeatmapInfo beatmapInfo, IBindable<StarDifficulty> difficulty, IRulesetInfo rulesetInfo, Mod[]? mods, DifficultyIconTooltipType tooltipType)
        {
            if (tooltipType == DifficultyIconTooltipType.None)
                throw new ArgumentOutOfRangeException(nameof(tooltipType), tooltipType, "Cannot instantiate a tooltip without a type");

            BeatmapInfo = beatmapInfo;
            Difficulty = difficulty;
            Ruleset = rulesetInfo;
            Mods = mods;
            TooltipType = tooltipType;
        }
    }
}
