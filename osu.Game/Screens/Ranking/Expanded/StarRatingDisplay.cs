// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
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
    public class StarRatingDisplay : CompositeDrawable, IHasCurrentValue<StarDifficulty>
    {
        private Box background;
        private OsuTextFlowContainer textFlow;

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly BindableWithCurrent<StarDifficulty> current = new BindableWithCurrent<StarDifficulty>();

        public Bindable<StarDifficulty> Current
        {
            get => current.Current;
            set => current.Current = value;
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
                        textFlow = new OsuTextFlowContainer(s => s.Font = OsuFont.Numeric.With(weight: FontWeight.Black))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            TextAnchor = Anchor.BottomLeft,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            var starRatingParts = Current.Value.Stars.ToString("0.00", CultureInfo.InvariantCulture).Split('.');
            string wholePart = starRatingParts[0];
            string fractionPart = starRatingParts[1];
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            background.Colour = colours.ForStarDifficulty(Current.Value.Stars);

            textFlow.Clear();
            textFlow.AddText($"{wholePart}", s =>
            {
                s.Colour = Color4.Black;
                s.Font = s.Font.With(size: 14);
                s.UseFullGlyphHeight = false;
            });

            textFlow.AddText($"{separator}{fractionPart}", s =>
            {
                s.Colour = Color4.Black;
                s.Font = s.Font.With(size: 7);
                s.UseFullGlyphHeight = false;
            });
        }
    }
}
