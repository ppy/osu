﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class DrawableMostPlayedRow : DrawableProfileRow
    {
        private readonly BeatmapInfo beatmap;
        private readonly int playCount;
        private OsuHoverContainer mapperContainer;

        public DrawableMostPlayedRow(BeatmapInfo beatmap, int playCount)
        {
            this.beatmap = beatmap;
            this.playCount = playCount;
        }

        protected override Drawable CreateLeftVisual() => new UpdateableBeatmapSetCover
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Size = new Vector2(80, 50),
            BeatmapSet = beatmap.BeatmapSet,
            CoverType = BeatmapSetCoverType.List,
        };

        [BackgroundDependencyLoader(true)]
        private void load(UserProfileOverlay profileOverlay)
        {
            LeftFlowContainer.Add(new BeatmapMetadataContainer(beatmap));
            LeftFlowContainer.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = @"mapped by ",
                        TextSize = 12,
                    },
                    mapperContainer = new OsuHoverContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = beatmap.Metadata.AuthorString,
                                TextSize = 12,
                                Font = @"Exo2.0-MediumItalic"
                            }
                        }
                    },
                }
            });

            RightFlowContainer.Add(new FillFlowContainer
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Text = playCount.ToString(),
                        TextSize = 18,
                        Font = @"Exo2.0-SemiBoldItalic"
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Text = @"times played ",
                        TextSize = 12,
                        Font = @"Exo2.0-RegularItalic"
                    },
                }
            });

            if (profileOverlay != null)
                mapperContainer.Action = () => profileOverlay.ShowUser(beatmap.BeatmapSet.Metadata.Author);
        }
    }
}
