// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class SetPanelContent : CompositeDrawable
    {
        // Disallow interacting with difficulty icons on a panel until the panel has been selected.
        public override bool PropagatePositionalInputSubTree => carouselSet.State.Value == CarouselItemState.Selected;

        private readonly CarouselBeatmapSet carouselSet;

        private FillFlowContainer<DifficultyIcon> iconFlow = null!;

        public SetPanelContent(CarouselBeatmapSet carouselSet)
        {
            this.carouselSet = carouselSet;

            // required to ensure we load as soon as any part of the panel comes on screen
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var beatmapSet = carouselSet.BeatmapSet;

            InternalChild = new FillFlowContainer
            {
                // required to ensure we load as soon as any part of the panel comes on screen
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding { Top = 5, Left = 18, Right = 10, Bottom = 10 },
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = new RomanisableString(beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title),
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22, italics: true),
                        Shadow = true,
                    },
                    new OsuSpriteText
                    {
                        Text = new RomanisableString(beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist),
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 17, italics: true),
                        Shadow = true,
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                        Margin = new MarginPadding { Top = 5 },
                        Spacing = new Vector2(5),
                        Children = new[]
                        {
                            beatmapSet.AllBeatmapsUpToDate
                                ? Empty()
                                : new Container
                                {
                                    AutoSizeAxes = Axes.X,
                                    RelativeSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        new UpdateBeatmapSetButton(beatmapSet),
                                    }
                                },
                            new BeatmapSetOnlineStatusPill
                            {
                                AutoSizeAxes = Axes.Both,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                TextSize = 11,
                                TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                Status = beatmapSet.Status
                            },
                            iconFlow = new FillFlowContainer<DifficultyIcon>
                            {
                                AutoSizeAxes = Axes.Both,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Spacing = new Vector2(3),
                            },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            iconFlow.ChildrenEnumerable = getDifficultyIcons();
        }

        private const int maximum_difficulty_icons = 18;

        private IEnumerable<DifficultyIcon> getDifficultyIcons()
        {
            var beatmaps = carouselSet.Beatmaps.ToList();

            return beatmaps.Count > maximum_difficulty_icons
                ? beatmaps.GroupBy(b => b.BeatmapInfo.Ruleset)
                          .Select(group => new GroupedDifficultyIcon(group.ToList(), group.Last().BeatmapInfo.Ruleset))
                : beatmaps.Select(b => new FilterableDifficultyIcon(b));
        }
    }
}
