// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Input.Events;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Graphics.UserInterface
{
    public class GradientLineTabControl<TModel> : PageTabControl<TModel>
    {
        protected override Dropdown<TModel> CreateDropdown() => null;

        protected override TabItem<TModel> CreateTabItem(TModel value) => new ScopeSelectorTabItem(value);

        protected Color4 LineColour
        {
            get => line.MainColour.Value;
            set => line.MainColour.Value = value;
        }

        private readonly GradientLine line;

        public GradientLineTabControl()
        {
            RelativeSizeAxes = Axes.X;

            AddInternal(line = new GradientLine
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            });
        }

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            AutoSizeAxes = Axes.X,
            RelativeSizeAxes = Axes.Y,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(20, 0),
        };

        private class ScopeSelectorTabItem : PageTabItem
        {
            public ScopeSelectorTabItem(TModel value)
                : base(value)
            {
                Text.Font = OsuFont.GetFont(size: 16);
            }

            protected override bool OnHover(HoverEvent e)
            {
                Text.FadeColour(AccentColour);

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                Text.FadeColour(Color4.White);
            }
        }

        private class GradientLine : GridContainer
        {
            public readonly Bindable<Color4> MainColour = new Bindable<Color4>();

            private readonly Box left;
            private readonly Box middle;
            private readonly Box right;

            public GradientLine()
            {
                RelativeSizeAxes = Axes.X;
                Size = new Vector2(0.8f, 1.5f);

                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(mode: GridSizeMode.Relative, size: 0.4f),
                    new Dimension(),
                };

                Content = new[]
                {
                    new Drawable[]
                    {
                        left = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        middle = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        right = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                };
            }

            protected override void LoadComplete()
            {
                MainColour.BindValueChanged(onColourChanged, true);
                base.LoadComplete();
            }

            private void onColourChanged(ValueChangedEvent<Color4> colour)
            {
                left.Colour = ColourInfo.GradientHorizontal(colour.NewValue.Opacity(0), colour.NewValue);
                middle.Colour = colour.NewValue;
                right.Colour = ColourInfo.GradientHorizontal(colour.NewValue, colour.NewValue.Opacity(0));
            }
        }
    }
}
