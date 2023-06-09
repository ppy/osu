// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public abstract partial class LabelledDrawable<T> : CompositeDrawable
        where T : Drawable
    {
        private float? fixedLabelWidth;

        /// <summary>
        /// The fixed width of the label of this <see cref="LabelledDrawable{T}"/>.
        /// If <c>null</c>, the label portion will auto-size to its content.
        /// Can be used in layout scenarios where several labels must match in length for the components to be aligned properly.
        /// </summary>
        public float? FixedLabelWidth
        {
            get => fixedLabelWidth;
            set
            {
                if (fixedLabelWidth == value)
                    return;

                fixedLabelWidth = value;

                updateLabelWidth();
            }
        }

        protected const float CONTENT_PADDING_VERTICAL = 10;
        protected const float CONTENT_PADDING_HORIZONTAL = 15;

        public const float CORNER_RADIUS = 15;

        /// <summary>
        /// The component that is being displayed.
        /// </summary>
        protected readonly T Component;

        private readonly Box background;
        private readonly GridContainer grid;
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
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
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
                        grid = new GridContainer
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
                                        Padding = new MarginPadding
                                        {
                                            Right = 20,
                                            // ensure that the label is always vertically padded even if the component itself isn't.
                                            // this may become an issue if the label is taller than the component.
                                            Vertical = padded ? 0 : CONTENT_PADDING_VERTICAL
                                        }
                                    },
                                    new Container
                                    {
                                        // top right works better when the vertical height of the component changes smoothly (avoids weird layout animations).
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
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

            updateLabelWidth();
        }

        private void updateLabelWidth()
        {
            if (fixedLabelWidth == null)
            {
                grid.ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize) };
                labelText.RelativeSizeAxes = Axes.None;
                labelText.AutoSizeAxes = Axes.Both;
            }
            else
            {
                grid.ColumnDimensions = new[] { new Dimension(GridSizeMode.Absolute, fixedLabelWidth.Value) };
                labelText.AutoSizeAxes = Axes.Y;
                labelText.RelativeSizeAxes = Axes.X;
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour osuColour)
        {
            background.Colour = colourProvider?.Background4 ?? Color4Extensions.FromHex(@"1c2125");
            descriptionText.Colour = osuColour.Yellow;
        }

        public LocalisableString Label
        {
            set => labelText.Text = value;
        }

        public LocalisableString Description
        {
            set
            {
                descriptionText.Text = value;

                if (!string.IsNullOrEmpty(value.ToString()))
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
