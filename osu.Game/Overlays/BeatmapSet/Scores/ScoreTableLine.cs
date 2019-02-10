// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTableLine : GridContainer
    {
        private const float rank_position = 30;
        private const float drawable_rank_position = 45;
        private const float score_position = 90;
        private const float accuracy_position = 170;
        private const float flag_position = 220;
        private const float player_position = 250;

        private const float max_combo_position = 0.1f;
        private const float hit_great_position = 0.3f;
        private const float hit_good_position = 0.45f;
        private const float hit_meh_position = 0.6f;
        private const float hit_miss_position = 0.75f;
        private const float pp_position = 0.9f;

        protected readonly Container RankContainer;
        protected readonly Container DrawableRankContainer;
        protected readonly Container ScoreContainer;
        protected readonly Container AccuracyContainer;
        protected readonly Container FlagContainer;
        protected readonly Container PlayerContainer;
        protected readonly Container MaxComboContainer;
        protected readonly Container HitGreatContainer;
        protected readonly Container HitGoodContainer;
        protected readonly Container HitMehContainer;
        protected readonly Container HitMissContainer;
        protected readonly Container PPContainer;
        protected readonly Container ModsContainer;

        public ScoreTableLine(int maxModsAmount)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, 25),
            };
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, 300),
                new Dimension(),
                new Dimension(GridSizeMode.AutoSize),
            };
            Content = new[]
            {
                new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            RankContainer = new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreRight,
                                AutoSizeAxes = Axes.Both,
                                X = rank_position,
                            },
                            DrawableRankContainer = new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                X = drawable_rank_position,
                            },
                            ScoreContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                X = score_position,
                            },
                            AccuracyContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                X = accuracy_position,
                            },
                            FlagContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                X = flag_position,
                            },
                            PlayerContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                X = player_position,
                            }
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            MaxComboContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                X = max_combo_position,
                            },
                            HitGreatContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                X = hit_great_position,
                            },
                            HitGoodContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                X = hit_good_position,
                            },
                            HitMehContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                X = hit_meh_position,
                            },
                            HitMissContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                X = hit_miss_position,
                            },
                            PPContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                                RelativePositionAxes = Axes.X,
                                X = pp_position,
                            }
                        }
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = ModsContainer = new Container
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            X = -30 * ((maxModsAmount == 0) ? 1 : maxModsAmount),
                        }
                    }
                }
            };
        }
    }
}
