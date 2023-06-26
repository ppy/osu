// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
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
    public partial class StarRatingDisplay : CompositeDrawable, IHasCurrentValue<StarDifficulty>
    {
        private readonly bool animated;
        private readonly Box background;
        private readonly SpriteIcon starIcon;
        private readonly OsuSpriteText starsText;

        private readonly BindableWithCurrent<StarDifficulty> current = new BindableWithCurrent<StarDifficulty>();

        public Bindable<StarDifficulty> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly Bindable<double> displayedStars = new BindableDouble();

        /// <summary>
        /// The currently displayed stars of this display wrapped in a bindable.
        /// This bindable gets transformed on change rather than instantaneous, if animation is enabled.
        /// </summary>
        public IBindable<double> DisplayedStars => displayedStars;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider? colourProvider { get; set; }

        /// <summary>
        /// Creates a new <see cref="StarRatingDisplay"/> using an already computed <see cref="StarDifficulty"/>.
        /// </summary>
        /// <param name="starDifficulty">The already computed <see cref="StarDifficulty"/> to display.</param>
        /// <param name="size">The size of the star rating display.</param>
        /// <param name="animated">Whether the star rating display will perform transforms on change rather than updating instantaneously.</param>
        public StarRatingDisplay(StarDifficulty starDifficulty, StarRatingDisplaySize size = StarRatingDisplaySize.Regular, bool animated = false)
        {
            this.animated = animated;

            Current.Value = starDifficulty;

            AutoSizeAxes = Axes.Both;

            MarginPadding margin = default;

            switch (size)
            {
                case StarRatingDisplaySize.Small:
                    margin = new MarginPadding { Horizontal = 7f };
                    break;

                case StarRatingDisplaySize.Range:
                    margin = new MarginPadding { Horizontal = 8f };
                    break;

                case StarRatingDisplaySize.Regular:
                    margin = new MarginPadding { Horizontal = 8f, Vertical = 2f };
                    break;
            }

            InternalChild = new CircularContainer
            {
                Masking = true,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new GridContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Margin = margin,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, 3f),
                            new Dimension(GridSizeMode.AutoSize, minSize: 25f),
                        },
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                        Content = new[]
                        {
                            new[]
                            {
                                starIcon = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.Star,
                                    Size = new Vector2(8f),
                                },
                                Empty(),
                                starsText = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Margin = new MarginPadding { Bottom = 1.5f },
                                    // todo: this should be size: 12f, but to match up with the design, it needs to be 14.4f
                                    // see https://github.com/ppy/osu-framework/issues/3271.
                                    Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                                    Shadow = false,
                                },
                            }
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(c =>
            {
                if (animated)
                    this.TransformBindableTo(displayedStars, c.NewValue.Stars, 750, Easing.OutQuint);
                else
                    displayedStars.Value = c.NewValue.Stars;
            });

            displayedStars.Value = Current.Value.Stars;

            displayedStars.BindValueChanged(s =>
            {
                starsText.Text = s.NewValue < 0 ? "-" : s.NewValue.ToLocalisableString("0.00");

                background.Colour = colours.ForStarDifficulty(s.NewValue);

                starIcon.Colour = s.NewValue >= 6.5 ? colours.Orange1 : colourProvider?.Background5 ?? Color4Extensions.FromHex("303d47");
                starsText.Colour = s.NewValue >= 6.5 ? colours.Orange1 : colourProvider?.Background5 ?? Color4.Black.Opacity(0.75f);
            }, true);
        }
    }

    public enum StarRatingDisplaySize
    {
        Small,
        Range,
        Regular,
    }
}
