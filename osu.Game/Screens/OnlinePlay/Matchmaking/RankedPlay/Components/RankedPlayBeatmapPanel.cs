// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;

using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class BeatmapPanel : CompositeDrawable
    {

        public BeatmapPanel(APIBeatmap apibeatmap, Mod[] mods)
        {
            beatmapSet = apibeatmap.BeatmapSet!;
            beatmap = apibeatmap;
            this.mods = mods;
            AutoSizeAxes = Axes.Both;
        }

        private readonly APIBeatmapSet beatmapSet;
        private readonly APIBeatmap beatmap;
        private readonly Mod[] mods;
        private FillFlowContainer idleBottomContent = null!;
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                    {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TruncatingSpriteText
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Text = new RomanisableString(beatmapSet.TitleUnicode, beatmapSet.Title),
                            Font = OsuFont.Default.With(size: 32f, weight: FontWeight.Bold),
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension()
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new TruncatingSpriteText
                                    {
                                        Origin = Anchor.TopLeft,
                                        Anchor = Anchor.TopLeft,
                                        Text = BeatmapsetsStrings.ShowDetailsByArtist(new RomanisableString(beatmapSet.ArtistUnicode, beatmapSet.Artist)),
                                        Font = OsuFont.Default.With(size: 18f, weight: FontWeight.SemiBold),
                                    },
                                    new LinkFlowContainer(s =>
                                        {
                                            s.Shadow = false;
                                            s.Font = OsuFont.Default.With(size: 18f, weight: FontWeight.SemiBold);
                                            Origin = Anchor.TopLeft;
                                            Anchor = Anchor.TopLeft;
                                        }
                                        ).With(d =>
                                        {
                                            d.AddText("    mapped by ", t => t.Colour = colours.Blue);
                                            d.AddUserLink(beatmapSet.Author);
                                        }
                                    )
                                },
                            }
                        },
                        new Container
                        {
                            Name = @"Bottom content",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                idleBottomContent = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 2),
                                    AlwaysPresent = true,
                                    Children = new Drawable[]
                                    {
                                        new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            ColumnDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            RowDimensions = new[]
                                            {
                                                new Dimension(GridSizeMode.AutoSize)
                                            },
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    new Container
                                                    {
                                                        Masking = true,
                                                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Children = new Drawable[]
                                                        {
                                                            new Box
                                                            {
                                                                Colour = colours.ForStarDifficulty(beatmap.StarRating).Darken(0.8f),
                                                                RelativeSizeAxes = Axes.Both,
                                                            },
                                                            new FillFlowContainer
                                                            {
                                                                Padding = new MarginPadding(4),
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Direction = FillDirection.Horizontal,
                                                                Spacing = new Vector2(6, 0),
                                                                Children = new Drawable[]
                                                                {
                                                                    new StarRatingDisplay(new StarDifficulty(beatmap.StarRating, 0), StarRatingDisplaySize.Small, animated: true)
                                                                    {
                                                                        Origin = Anchor.CentreLeft,
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Scale = new Vector2(0.9f),
                                                                    },
                                                                    new TruncatingSpriteText
                                                                    {
                                                                        Text = beatmap.DifficultyName,
                                                                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Origin = Anchor.CentreLeft,
                                                                    },
                                                                }
                                                            },
                                                        }
                                                    },
                                                    new Container
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Alpha = mods.Length > 0 ? 1 : 0,
                                                        Child = new ModFlowDisplay
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Scale = new Vector2(0.5f),
                                                            Margin = new MarginPadding { Left = 5 },
                                                            Current = { Value = mods },
                                                        }
                                                    }
                                                },
                                            }
                                        },
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();
            FinishTransforms(true);
        }
    }
}
