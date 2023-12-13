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
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardScoreTooltip : VisibilityContainer, ITooltip<ScoreInfo>
    {
        private OsuSpriteText timestampLabel = null!;
        private FillFlowContainer<HitResultCell> topScoreStatistics = null!;
        private FillFlowContainer<HitResultCell> bottomScoreStatistics = null!;
        private FillFlowContainer<ModCell> modStatistics = null!;
        private readonly Bindable<bool> prefer24HourTime = new Bindable<bool>();

        public LeaderboardScoreTooltip()
        {
            AutoSizeAxes = Axes.Both;
            AutoSizeDuration = 200;
            AutoSizeEasing = Easing.OutQuint;

            Masking = true;
            CornerRadius = 5;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.Prefer24HourTime, prefer24HourTime);
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.9f,
                    Colour = colours.Gray3,
                },
                new FillFlowContainer
                {
                    Margin = new MarginPadding(5),
                    Spacing = new Vector2(10),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        // Info row
                        timestampLabel = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                        },
                        // Mods row
                        modStatistics = new FillFlowContainer<ModCell>
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5, 0),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                // Actual stats rows
                                topScoreStatistics = new FillFlowContainer<HitResultCell>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10, 0),
                                },
                                bottomScoreStatistics = new FillFlowContainer<HitResultCell>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(10, 0),
                                },
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            prefer24HourTime.BindValueChanged(_ => updateTimestampLabel(), true);
        }

        private ScoreInfo? displayedScore;

        public void SetContent(ScoreInfo score)
        {
            if (displayedScore?.Equals(score) == true)
                return;

            displayedScore = score;

            updateTimestampLabel();

            modStatistics.Clear();
            topScoreStatistics.Clear();
            bottomScoreStatistics.Clear();

            foreach (var mod in score.Mods.AsOrdered())
            {
                modStatistics.Add(new ModCell(mod));
            }

            foreach (var result in score.GetStatisticsForDisplay())
            {
                if (result.Result > HitResult.Perfect)
                    bottomScoreStatistics.Add(new HitResultCell(result));
                else
                    topScoreStatistics.Add(new HitResultCell(result));
            }
        }

        private void updateTimestampLabel()
        {
            if (displayedScore != null)
            {
                timestampLabel.Text = LocalisableString.Format("Played on {0}",
                    displayedScore.Date.ToLocalTime().ToLocalisableString(prefer24HourTime.Value ? @"d MMMM yyyy HH:mm" : @"d MMMM yyyy h:mm tt"));
            }
        }

        protected override void PopIn() => this.FadeIn(20, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(80, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;

        private partial class HitResultCell : CompositeDrawable
        {
            private readonly LocalisableString displayName;
            private readonly HitResult result;
            private readonly int count;

            public HitResultCell(HitResultDisplayStatistic stat)
            {
                AutoSizeAxes = Axes.Both;

                displayName = stat.DisplayName.ToUpper();
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
                    Spacing = new Vector2(5f, 0f),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                            Text = displayName.ToUpper(),
                            Colour = colours.ForHitResult(result),
                        },
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                            Text = count.ToString(),
                        },
                    }
                };
            }
        }

        private partial class ModCell : CompositeDrawable
        {
            private readonly Mod mod;

            public ModCell(Mod mod)
            {
                AutoSizeAxes = Axes.Both;
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
                        new ModIcon(mod, showTooltip: false, showExtendedInformation: false).With(icon =>
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
                        Margin = new MarginPadding { Top = 1 },
                    });
                }
            }
        }
    }
}
