// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;

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

        protected override Drawable CreateLeftVisual() => new DelayedLoadWrapper(new BeatmapSetCover(beatmap.BeatmapSet, BeatmapSetCoverType.List)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            FillMode = FillMode.Fit,
            RelativeSizeAxes = Axes.Both,
            OnLoadComplete = d => d.FadeInFromZero(500, Easing.OutQuint)
        })
        {
            Origin = Anchor.CentreLeft,
            Anchor = Anchor.CentreLeft,
            RelativeSizeAxes = Axes.None,
            Size = new Vector2(80, 50),
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
