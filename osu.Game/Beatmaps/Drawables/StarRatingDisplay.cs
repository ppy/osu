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
        /// <param name="size">The size of the star rating display.</param>
        public StarRatingDisplay(StarDifficulty starDifficulty, StarRatingDisplaySize size = StarRatingDisplaySize.Regular)
        {
            Current.Value = starDifficulty;

            AutoSizeAxes = Axes.Both;

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
                        Margin = size == StarRatingDisplaySize.Small
                            ? new MarginPadding { Horizontal = 7f }
                            : new MarginPadding { Horizontal = 8f, Vertical = 2f },
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
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding { Bottom = 1.5f },
                                    // todo: this should be size: 12f, but to match up with the design, it needs to be 14.4f
                                    // see https://github.com/ppy/osu-framework/issues/3271.
                                    Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                                    Shadow = false,
                                }
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
                starsText.Text = c.NewValue.Stars.ToString("0.00");

                background.Colour = colours.ForStarDifficulty(c.NewValue.Stars);

                starIcon.Colour = c.NewValue.Stars >= 6.5 ? colours.Orange1 : colourProvider?.Background5 ?? Color4Extensions.FromHex("303d47");
                starsText.Colour = c.NewValue.Stars >= 6.5 ? colours.Orange1 : colourProvider?.Background5 ?? Color4.Black.Opacity(0.75f);
            }, true);
        }
    }

    public enum StarRatingDisplaySize
    {
        Small,
        Regular,
    }
}
