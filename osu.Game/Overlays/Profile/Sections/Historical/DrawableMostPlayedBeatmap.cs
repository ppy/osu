// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class DrawableMostPlayedBeatmap : OsuHoverContainer
    {
        private readonly Box background;
        private const int cover_width = 100;
        private const int corner_radius = 10;
        private readonly SpriteIcon icon;
        private readonly OsuSpriteText playCountText;
        private readonly LinkFlowContainer mapper;

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        public DrawableMostPlayedBeatmap(BeatmapInfo beatmap, int playCount)
        {
            Enabled.Value = true; //manually enabled, because we have no action

            RelativeSizeAxes = Axes.X;
            Height = 60;
            Masking = true;
            CornerRadius = corner_radius;
            Children = new Drawable[]
            {
                new UpdateableBeatmapSetCover
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = cover_width,
                    BeatmapSet = beatmap.BeatmapSet,
                    CoverType = BeatmapSetCoverType.List,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = cover_width - corner_radius },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = corner_radius,
                            Children = new Drawable[]
                            {
                                background = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Left = 15, Right = 20 },
                                    Children = new Drawable[]
                                    {
                                        new MostPlayedBeatmapMetadataContainer(beatmap)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.BottomLeft,
                                            Margin = new MarginPadding { Bottom = 2 },
                                        },
                                        mapper = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: 15, weight: FontWeight.Regular))
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.TopLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Margin = new MarginPadding { Top = 2 },
                                        }.With(d =>
                                        {
                                            d.AddText("mapped by ");
                                            d.AddUserLink(beatmap.Metadata.Author);
                                        }),
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new Drawable[]
                                            {
                                                icon = new SpriteIcon
                                                {
                                                    Icon = FontAwesome.Solid.CaretRight,
                                                    Origin = Anchor.Centre,
                                                    Anchor = Anchor.Centre,
                                                    Size = new Vector2(20),
                                                },
                                                playCountText = new OsuSpriteText
                                                {
                                                    Origin = Anchor.Centre,
                                                    Anchor = Anchor.Centre,
                                                    Text = playCount.ToString(),
                                                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Regular, fixedWidth: true),
                                                },
                                            }
                                        }
                                    }
                                },
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colors)
        {
            IdleColour = colors.GreySeafoam;
            HoverColour = colors.GreySeafoamLight;
            mapper.Colour = colors.GreySeafoamLighter;
            icon.Colour = playCountText.Colour = colors.Yellow;
        }

        private class MostPlayedBeatmapMetadataContainer : BeatmapMetadataContainer
        {
            public MostPlayedBeatmapMetadataContainer(BeatmapInfo beatmap)
                : base(beatmap)
            {
            }

            protected override Drawable[] CreateText(BeatmapInfo beatmap) => new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = new LocalisedString((
                        $"{beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title} [{beatmap.Version}] ",
                        $"{beatmap.Metadata.Title ?? beatmap.Metadata.TitleUnicode} [{beatmap.Version}] ")),
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold)
                },
                new OsuSpriteText
                {
                    Text = "by " + new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)),
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular)
                },
            };
        }
    }
}
