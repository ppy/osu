// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// A pill that displays the star rating of a <see cref="BeatmapInfo"/>.
    /// </summary>
    public class StarRatingDisplay : CompositeDrawable
    {
        [Resolved]
        private OsuColour colours { get; set; }

        private CircularContainer colorContainer;
        private OsuTextFlowContainer textContainer;
        private CancellationTokenSource cancellationTokenSource;
        private IBindable<StarDifficulty?> bindableStarDifficulty;

        private readonly StarDifficulty starDifficulty;
        private readonly BeatmapInfo beatmapInfo;

        /// <summary>
        /// Creates a new <see cref="StarRatingDisplay"/> using an already computed <see cref="StarDifficulty"/>.
        /// </summary>
        /// <param name="starDifficulty">The already computed <see cref="StarDifficulty"/> to display the star difficulty of.</param>
        public StarRatingDisplay(StarDifficulty starDifficulty)
        {
            this.starDifficulty = starDifficulty;
        }

        /// <summary>
        /// Creates a new <see cref="StarRatingDisplay"/> using a <see cref="BeatmapInfo"/> to use a bindable for the difficulty. 
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to use to create a bindable for <see cref="StarDifficulty"/></param>
        public StarRatingDisplay(BeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;
        }

        private void setDifficulty(StarDifficulty difficulty)
        {
            var starRatingParts = difficulty.Stars.ToString("0.00", CultureInfo.InvariantCulture).Split('.');
            string wholePart = starRatingParts[0];
            string fractionPart = starRatingParts[1];
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            ColourInfo backgroundColour = difficulty.DifficultyRating == DifficultyRating.ExpertPlus
                ? ColourInfo.GradientVertical(Color4Extensions.FromHex("#C1C1C1"), Color4Extensions.FromHex("#595959"))
                : (ColourInfo)colours.ForDifficultyRating(difficulty.DifficultyRating);

            colorContainer.Colour = backgroundColour;

            textContainer.Text = string.Empty;

            textContainer.With(t =>
            {
                t.AddText($"{wholePart}", s =>
                {
                    s.Colour = Color4.Black;
                    s.Font = s.Font.With(size: 14);
                    s.UseFullGlyphHeight = false;
                });

                t.AddText($"{separator}{fractionPart}", s =>
                {
                    s.Colour = Color4.Black;
                    s.Font = s.Font.With(size: 7);
                    s.UseFullGlyphHeight = false;
                });
            });
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapDifficultyCache difficultyCache)
        {
            if (beatmapInfo != null)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();

                bindableStarDifficulty?.UnbindAll();
                bindableStarDifficulty = difficultyCache.GetBindableDifficulty(beatmapInfo, cancellationTokenSource.Token);
            }

            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                colorContainer = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 8, Vertical = 4 },
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(2, 0),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(7),
                            Icon = FontAwesome.Solid.Star,
                            Colour = Color4.Black
                        },
                        textContainer = new OsuTextFlowContainer(s => s.Font = OsuFont.Numeric.With(weight: FontWeight.Black))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            TextAnchor = Anchor.BottomLeft,
                        },
                    }
                }
            };

            if (bindableStarDifficulty != null)
                bindableStarDifficulty.BindValueChanged(valueChanged => setDifficulty(valueChanged.NewValue ?? new StarDifficulty()), true);
            else
                setDifficulty(starDifficulty);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            cancellationTokenSource?.Cancel();
        }
    }
}
