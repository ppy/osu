// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen
    {
        public partial class TitlePanel : CompositeDrawable
        {
            private readonly APIBeatmap beatmap;

            private OsuSpriteText titleText = null!;
            private OsuSpriteText difficultyNameText = null!;
            private OsuSpriteText artistText = null!;
            private OsuSpriteText creatorText = null!;

            private PanelScaffold panelScaffold = null!;
            public TitlePanel(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = panelScaffold = new PanelScaffold
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            // Centered title
                            titleText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 28, weight: FontWeight.Bold, typeface: Typeface.TorusAlternate),
                            },

                            // Centered diffname
                            difficultyNameText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 18)
                            },

                            // Bottom row with artist/mapper
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.5f,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[] { new Dimension(), new Dimension() },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        artistText = new OsuSpriteText { Text = $"Artist: {beatmap.Metadata.Artist}", Anchor = Anchor.CentreLeft, Origin = Anchor.CentreLeft },
                                        creatorText = new OsuSpriteText { Text = $"Creator:{beatmap.Metadata.Author}", Anchor = Anchor.CentreRight, Origin = Anchor.CentreRight }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                titleText.Text = $"{beatmap.Metadata.Title}";
                difficultyNameText.Text = $"[{beatmap.DifficultyName}]";
                artistText.Text = $"Artist: {beatmap.Metadata.Artist}";
                creatorText.Text = $"Creator:{beatmap.Metadata.Author}";
                panelScaffold.BottomOrnament.Alpha = 0; //Hide the BottomOrnament
            }

        }
    }
}
