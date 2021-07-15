// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.Ranking.Expanded;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class StarRatingRangeDisplay : OnlinePlayComposite
    {
        [Resolved]
        private OsuColour colours { get; set; }

        private StarRatingDisplay minDisplay;
        private Drawable minBackground;
        private StarRatingDisplay maxDisplay;
        private Drawable maxBackground;

        public StarRatingRangeDisplay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 1,
                    Children = new[]
                    {
                        minBackground = new Box
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.5f),
                        },
                        maxBackground = new Box
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.5f),
                        },
                    }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        minDisplay = new StarRatingDisplay(default),
                        maxDisplay = new StarRatingDisplay(default)
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.BindCollectionChanged(updateRange, true);
        }

        private void updateRange(object sender, NotifyCollectionChangedEventArgs e)
        {
            var orderedDifficulties = Playlist.Select(p => p.Beatmap.Value).OrderBy(b => b.StarDifficulty).ToArray();

            StarDifficulty minDifficulty = new StarDifficulty(orderedDifficulties.Length > 0 ? orderedDifficulties[0].StarDifficulty : 0, 0);
            StarDifficulty maxDifficulty = new StarDifficulty(orderedDifficulties.Length > 0 ? orderedDifficulties[^1].StarDifficulty : 0, 0);

            minDisplay.Current.Value = minDifficulty;
            maxDisplay.Current.Value = maxDifficulty;

            minBackground.Colour = colours.ForDifficultyRating(minDifficulty.DifficultyRating, true);
            maxBackground.Colour = colours.ForDifficultyRating(maxDifficulty.DifficultyRating, true);
        }
    }
}
