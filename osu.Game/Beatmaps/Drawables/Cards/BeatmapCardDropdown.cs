// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardDropdown : CompositeDrawable
    {
        public Drawable Body
        {
            set => bodyContent.Child = value;
        }

        public Drawable Dropdown
        {
            set => dropdownContent.Child = value;
        }

        public Bindable<bool> Expanded { get; } = new BindableBool();

        private readonly Box background;
        private readonly Container bodyContent;
        private readonly Container dropdownContent;

        public BeatmapCardDropdown(float height)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                CornerRadius = BeatmapCard.CORNER_RADIUS,
                Masking = true,

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    bodyContent = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = height,
                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                        Masking = true,
                    },
                    dropdownContent = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Top = height },
                        Alpha = 0
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background2;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Expanded.BindValueChanged(_ => updateState());
        }

        private void updateState()
        {
            background.FadeTo(Expanded.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            dropdownContent.FadeTo(Expanded.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
