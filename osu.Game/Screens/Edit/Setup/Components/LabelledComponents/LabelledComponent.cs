// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Setup.Components.LabelledComponents
{
    public abstract class LabelledComponent : CompositeDrawable
    {
        protected const float CONTENT_PADDING_VERTICAL = 10;
        protected const float CONTENT_PADDING_HORIZONTAL = 15;
        protected const float CORNER_RADIUS = 15;

        protected readonly Drawable Component;

        private readonly OsuTextFlowContainer label;
        private readonly OsuTextFlowContainer bottomText;

        public string LabelText
        {
            set => label.Text = value;
        }

        public string BottomLabelText
        {
            set => bottomText.Text = value;
        }

        protected LabelledComponent()
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
                    Colour = OsuColour.FromHex("1c2125"),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = CONTENT_PADDING_HORIZONTAL, Vertical = CONTENT_PADDING_VERTICAL },
                    Direction = FillDirection.Vertical,
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
                                    label = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(weight: FontWeight.Bold))
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding
                                        {
                                            Right = ExpandComponent ? 0 : -CONTENT_PADDING_HORIZONTAL,
                                            Vertical = ExpandComponent ? 0 : -CONTENT_PADDING_VERTICAL
                                        },
                                        Child = Component = CreateComponent().With(d =>
                                        {
                                            d.Anchor = Anchor.TopRight;
                                            d.Origin = Anchor.TopRight;
                                        })
                                    }
                                },
                            },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                        },
                        bottomText = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold, italics: true))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            bottomText.Colour = osuColour.Yellow;
        }

        /// <summary>
        /// Whether to expand the component to fill the entire available extents.
        /// </summary>
        protected virtual bool ExpandComponent => true;

        protected abstract Drawable CreateComponent();
    }
}
