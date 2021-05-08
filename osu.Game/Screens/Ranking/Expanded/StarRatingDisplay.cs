// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// A pill that displays the star rating of a <see cref="BeatmapInfo"/>.
    /// </summary>
    public class StarRatingDisplay : CompositeDrawable, IHasCurrentValue<StarDifficulty?>
    {
        private Box background;
        private OsuSpriteText wholePart;
        private OsuSpriteText fractionPart;

        private double displayedStarRating;

        protected double DisplayedStarRating
        {
            get => displayedStarRating;
            set
            {
                displayedStarRating = value;

                var starRatingParts = value.ToString("0.00", CultureInfo.InvariantCulture).Split('.');
                wholePart.Text = starRatingParts[0];
                fractionPart.Text = starRatingParts[1];
            }
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly BindableWithCurrent<StarDifficulty?> current = new BindableWithCurrent<StarDifficulty?>();

        public Bindable<StarDifficulty?> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        /// <summary>
        /// Creates a new <see cref="StarRatingDisplay"/> without any <see cref="StarDifficulty"/> set, displaying a placeholder until <see cref="Current"/> is changed.
        /// </summary>
        public StarRatingDisplay()
        {
        }

        /// <summary>
        /// Creates a new <see cref="StarRatingDisplay"/> using an already computed <see cref="StarDifficulty"/>.
        /// </summary>
        /// <param name="starDifficulty">The already computed <see cref="StarDifficulty"/> to display the star difficulty of.</param>
        public StarRatingDisplay(StarDifficulty starDifficulty)
        {
            Current.Value = starDifficulty;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDifficultyCache difficultyCache)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new Box
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
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new[]
                            {
                                wholePart = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Colour = Color4.Black,
                                    Font = OsuFont.Numeric.With(size: 14, weight: FontWeight.Black),
                                    UseFullGlyphHeight = false,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Colour = Color4.Black,
                                    Text = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator,
                                    Font = OsuFont.Numeric.With(size: 7, weight: FontWeight.Black),
                                    UseFullGlyphHeight = false,
                                },
                                fractionPart = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Colour = Color4.Black,
                                    Font = OsuFont.Numeric.With(size: 7, weight: FontWeight.Black),
                                    UseFullGlyphHeight = false,
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

            Current.BindValueChanged(_ => updateDisplay(), true);
            FinishTransforms(true);
        }

        private void updateDisplay()
        {
            const double duration = 400;
            const Easing easing = Easing.OutQuint;

            ColourInfo backgroundColour;

            if (Current.Value == null)
                backgroundColour = Color4.SlateGray.Opacity(0.3f);
            else
            {
                var rating = Current.Value.Value.DifficultyRating;

                backgroundColour = rating == DifficultyRating.ExpertPlus
                    ? ColourInfo.GradientVertical(Color4Extensions.FromHex("#C1C1C1"), Color4Extensions.FromHex("#595959"))
                    : (ColourInfo)colours.ForDifficultyRating(rating);
            }

            background.FadeColour(backgroundColour, duration, easing);
            this.TransformTo(nameof(DisplayedStarRating), Current.Value?.Stars ?? 0.0f, duration, easing);
        }
    }
}
