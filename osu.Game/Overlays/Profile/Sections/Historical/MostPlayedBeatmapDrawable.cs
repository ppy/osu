// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class MostPlayedBeatmapDrawable : Container
    {
        private readonly BeatmapInfo beatmap;
        private readonly OsuHoverContainer mapperContainer;

        private readonly EdgeEffectParameters edgeEffectNormal = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Offset = new Vector2(0, 1f),
            Radius = 2f,
            Colour = Color4.Black.Opacity(0.25f),
        };

        private readonly EdgeEffectParameters edgeEffectHovered = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Offset = new Vector2(0, 5f),
            Radius = 10f,
            Colour = Color4.Black.Opacity(0.25f),
        };

        public MostPlayedBeatmapDrawable(BeatmapInfo beatmap, int playCount)
        {
            this.beatmap = beatmap;
            RelativeSizeAxes = Axes.X;
            Height = 50;
            Margin = new MarginPadding { Bottom = 10 };
            Masking = true;
            EdgeEffect = edgeEffectNormal;

            Children = new Drawable[]
            {
                new Box //Background for this container, otherwise the shadow would be visible
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new Box //Image Background while loading
                {
                   Size = new Vector2(80, 50),
                   Colour = Color4.Black,
                },
                new DelayedLoadWrapper(new BeatmapSetCover(beatmap.BeatmapSet, BeatmapSetCoverType.List)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                }),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10) { Left = 90 },
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new BeatmapMetadataContainer(beatmap),
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = new []
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
                                }
                            },
                        },
                        new FillFlowContainer
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
                        },
                    },
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(UserProfileOverlay profileOverlay)
        {
            if(profileOverlay != null)
                mapperContainer.Action = () => profileOverlay.ShowUser(beatmap.BeatmapSet.Metadata.Author);
        }

        protected override bool OnHover(InputState state)
        {
            TweenEdgeEffectTo(edgeEffectHovered, 120, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            TweenEdgeEffectTo(edgeEffectNormal, 120, Easing.OutQuint);
            base.OnHoverLost(state);
        }
    }
}
