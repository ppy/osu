// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class StageDisplay : CompositeDrawable
    {
        public static readonly (MatchmakingStage status, LocalisableString text)[] DISPLAYED_STAGES =
        [
            (MatchmakingStage.RoundWarmupTime, "Next Round"),
            (MatchmakingStage.UserBeatmapSelect, "Beatmap Selection"),
            (MatchmakingStage.GameplayWarmupTime, "Get Ready"),
            (MatchmakingStage.ResultsDisplaying, "Results"),
            (MatchmakingStage.Ended, "Match End")
        ];

        public StageDisplay()
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            List<Dimension> columnDimensions = new List<Dimension>();
            List<Drawable> columnContent = new List<Drawable>();

            for (int i = 0; i < DISPLAYED_STAGES.Length; i++)
            {
                if (i > 0)
                {
                    columnDimensions.Add(new Dimension(GridSizeMode.AutoSize));
                    columnContent.Add(new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(16),
                        Icon = FontAwesome.Solid.ArrowRight,
                        Margin = new MarginPadding { Horizontal = 10 }
                    });
                }

                columnDimensions.Add(new Dimension());
                columnContent.Add(new StageBubble(DISPLAYED_STAGES[i].status, DISPLAYED_STAGES[i].text)
                {
                    RelativeSizeAxes = Axes.X
                });
            }

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions =
                [
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize)
                ],
                Content = new Drawable[][]
                {
                    [
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions = columnDimensions.ToArray(),
                            RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                            Content = new[] { columnContent.ToArray() }
                        }
                    ],
                    [
                        new StageText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        }
                    ]
                }
            };
        }
    }
}
