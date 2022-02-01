// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Scoring;
using osuTK;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

#nullable enable

namespace osu.Game.Online.Leaderboards
{
    public class LeaderboardScoreTooltip : VisibilityContainer, ITooltip<ScoreInfo>
    {
        private readonly OsuSpriteText timestampLabel;
        private readonly FillFlowContainer<HitResultCell> topScoreStatistics;
        private readonly FillFlowContainer<HitResultCell> bottomScoreStatistics;
        private readonly FillFlowContainer<ModCell> modStatistics;

        public LeaderboardScoreTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.7f,
                    Colour = Colour4.Black,
                },
                new GridContainer
                {
                    Margin = new MarginPadding(5f),
                    AutoSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        // Info row
                        new Drawable[]
                        {
                            timestampLabel = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                            }
                        },
                        // Mods row
                        new Drawable[]
                        {
                            modStatistics = new FillFlowContainer<ModCell>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                            }
                        },
                        // Actual stats rows
                        new Drawable[]
                        {
                            topScoreStatistics = new FillFlowContainer<HitResultCell>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                            }
                        },
                        new Drawable[]
                        {
                            bottomScoreStatistics = new FillFlowContainer<HitResultCell>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                            }
                        },
                    }
                }
            };
        }

        private ScoreInfo? displayedScore;

        public void SetContent(ScoreInfo score)
        {
            if (displayedScore?.Equals(score) == true)
                return;

            displayedScore = score;

            timestampLabel.Text = $"Played on {score.Date.ToLocalTime():d MMMM yyyy HH:mm}";

            modStatistics.Clear();
            topScoreStatistics.Clear();
            bottomScoreStatistics.Clear();

            foreach (var mod in score.Mods)
            {
                modStatistics.Add(new ModCell(mod));
            }

            foreach (var result in score.GetStatisticsForDisplay())
            {
                (result.Result > HitResult.Perfect
                        ? bottomScoreStatistics
                        : topScoreStatistics
                    ).Add(new HitResultCell(result));
            }
        }

        protected override void PopIn() => this.FadeIn(20, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(80, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        private class HitResultCell : CompositeDrawable
        {
            private readonly string displayName;
            private readonly HitResult result;
            private readonly int count;

            public HitResultCell(HitResultDisplayStatistic stat)
            {
                AutoSizeAxes = Axes.Both;
                Padding = new MarginPadding { Horizontal = 5f };

                displayName = stat.DisplayName;
                result = stat.Result;
                count = stat.Count;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChild = new FillFlowContainer
                {
                    Height = 12,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2f, 0f),
                    Children = new Drawable[]
                    {
                        new CircularContainer
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("#222")
                                },
                                new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Horizontal = 2f },
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                                    Text = displayName.ToUpperInvariant(),
                                    Colour = colours.ForHitResult(result),
                                }
                            }
                        },
                        new OsuSpriteText
                        {
                            RelativeSizeAxes = Axes.Y,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                            Text = count.ToString(),
                        },
                    }
                };
            }
        }

        private class ModCell : CompositeDrawable
        {
            private readonly Mod mod;

            public ModCell(Mod mod)
            {
                AutoSizeAxes = Axes.Both;
                Padding = new MarginPadding { Horizontal = 5f };
                this.mod = mod;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                FillFlowContainer container;
                InternalChild = container = new FillFlowContainer
                {
                    Height = 15,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2f, 0f),
                    Children = new Drawable[]
                    {
                        new ModIcon(mod, showTooltip: false).With(icon =>
                        {
                            icon.Origin = Anchor.CentreLeft;
                            icon.Anchor = Anchor.CentreLeft;
                            icon.Scale = new Vector2(15f / icon.Height);
                        }),
                    }
                };

                string description = mod.SettingDescription;

                if (!string.IsNullOrEmpty(description))
                {
                    container.Add(new OsuSpriteText
                    {
                        RelativeSizeAxes = Axes.Y,
                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                        Text = mod.SettingDescription,
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                    });
                }
            }
        }
    }
}
