// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public abstract class LabelledDrawable<T> : CompositeDrawable
        where T : Drawable
    {
        protected const float CONTENT_PADDING_VERTICAL = 10;
        protected const float CONTENT_PADDING_HORIZONTAL = 15;
        protected const float CORNER_RADIUS = 15;

        /// <summary>
        /// The component that is being displayed.
        /// </summary>
        protected readonly T Component;

        private readonly OsuTextFlowContainer labelText;
        private readonly OsuTextFlowContainer descriptionText;

        /// <summary>
        /// Creates a new <see cref="LabelledComponent{T, U}"/>.
        /// </summary>
        /// <param name="padded">Whether the component should be padded or should be expanded to the bounds of this <see cref="LabelledComponent{T, U}"/>.</param>
        protected LabelledDrawable(bool padded)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            CornerRadius = CORNER_RADIUS;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("1c2125"),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = padded
                        ? new MarginPadding { Horizontal = CONTENT_PADDING_HORIZONTAL, Vertical = CONTENT_PADDING_VERTICAL }
                        : new MarginPadding { Left = CONTENT_PADDING_HORIZONTAL },
                    Spacing = new Vector2(0, 12),
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    labelText = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Bold))
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Right = 20 }
                                    },
                                    new Container
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = Component = CreateComponent().With(d =>
                                        {
                                            d.Anchor = Anchor.CentreRight;
                                            d.Origin = Anchor.CentreRight;
                                        })
                                    }
                                },
                            },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                        },
                        descriptionText = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold, italics: true))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Bottom = padded ? 0 : CONTENT_PADDING_VERTICAL },
                            Alpha = 0,
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            descriptionText.Colour = osuColour.Yellow;
        }

        public string Label
        {
            set => labelText.Text = value;
        }

        public string Description
        {
            set
            {
                descriptionText.Text = value;

                if (!string.IsNullOrEmpty(value))
                    descriptionText.Show();
                else
                    descriptionText.Hide();
            }
        }

        /// <summary>
        /// Creates the component that should be displayed.
        /// </summary>
        /// <returns>The component.</returns>
        protected abstract T CreateComponent();
    }
}
