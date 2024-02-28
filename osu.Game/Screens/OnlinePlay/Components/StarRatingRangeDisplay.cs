// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class StarRatingRangeDisplay : OnlinePlayComposite
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
                        minDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Range),
                        maxDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Range)
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DifficultyRange.BindValueChanged(_ => updateRange());
            Playlist.BindCollectionChanged((_, _) => updateRange(), true);
        }

        private void updateRange()
        {
            StarDifficulty minDifficulty;
            StarDifficulty maxDifficulty;

            if (DifficultyRange.Value != null && Playlist.Count == 0)
            {
                // When Playlist is empty (in lounge) we take retrieved range
                minDifficulty = new StarDifficulty(DifficultyRange.Value.Min, 0);
                maxDifficulty = new StarDifficulty(DifficultyRange.Value.Max, 0);
            }
            else
            {
                // When Playlist is not empty (in room) we compute actual range
                var orderedDifficulties = Playlist.Select(p => p.Beatmap).OrderBy(b => b.StarRating).ToArray();

                minDifficulty = new StarDifficulty(orderedDifficulties.Length > 0 ? orderedDifficulties[0].StarRating : 0, 0);
                maxDifficulty = new StarDifficulty(orderedDifficulties.Length > 0 ? orderedDifficulties[^1].StarRating : 0, 0);
            }

            minDisplay.Current.Value = minDifficulty;
            maxDisplay.Current.Value = maxDifficulty;
            maxDisplay.Alpha = Precision.AlmostEquals(Math.Round(minDifficulty.Stars, 2), Math.Round(maxDifficulty.Stars, 2)) ? 0 : 1;

            minBackground.Colour = colours.ForStarDifficulty(minDifficulty.Stars);
            maxBackground.Colour = colours.ForStarDifficulty(maxDifficulty.Stars);
        }
    }
}
