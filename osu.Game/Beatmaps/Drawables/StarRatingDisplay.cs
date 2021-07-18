// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// A pill that displays the star rating of a beatmap.
    /// </summary>
    public class StarRatingDisplay : CompositeDrawable, IHasCurrentValue<StarDifficulty>
    {
        private readonly bool useNewDifficultyColours;
        private readonly Box background;
        private readonly SpriteIcon starIcon;
        private readonly OsuSpriteText starsText;

        private readonly BindableWithCurrent<StarDifficulty> current = new BindableWithCurrent<StarDifficulty>();

        public Bindable<StarDifficulty> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved(canBeNull: true)]
        private OverlayColourProvider colourProvider { get; set; }

        /// <summary>
        /// Creates a new <see cref="StarRatingDisplay"/> using an already computed <see cref="StarDifficulty"/>.
        /// </summary>
        /// <param name="starDifficulty">The already computed <see cref="StarDifficulty"/> to display.</param>
        /// <param name="useNewDifficultyColours">Use the new spectrum-based difficulty colours for the display, rather than the old.</param>
        public StarRatingDisplay(StarDifficulty starDifficulty, bool useNewDifficultyColours = false)
        {
            this.useNewDifficultyColours = useNewDifficultyColours;

            Current.Value = starDifficulty;

            Size = new Vector2(52f, 20f);

            InternalChild = new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    starIcon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Right = 26.5f },
                        Icon = FontAwesome.Solid.Star,
                        Size = new Vector2(8f),
                    },
                    starsText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Left = 10f, Bottom = 1.5f },
                        Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.Bold),
                        Shadow = false,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(c =>
            {
                starsText.Text = c.NewValue.Stars.ToString("0.00");

                background.Colour = useNewDifficultyColours
                    ? colours.ForStarDifficulty(c.NewValue.Stars)
                    : colours.ForDifficultyRating(c.NewValue.DifficultyRating);

                starIcon.Colour = c.NewValue.Stars >= 6.5 ? colours.Orange1 : colourProvider?.Background5 ?? Color4Extensions.FromHex("303d47");
                starsText.Colour = c.NewValue.Stars >= 6.5 ? colours.Orange1 : colourProvider?.Background5 ?? Color4.Black.Opacity(0.75f);
            }, true);
        }
    }
}
