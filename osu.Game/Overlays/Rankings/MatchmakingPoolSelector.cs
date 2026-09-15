// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Rankings
{
    public partial class MatchmakingPoolSelector : CompositeDrawable, IHasCurrentValue<APIMatchmakingPool>
    {
        private readonly BindableWithCurrent<APIMatchmakingPool> current = new BindableWithCurrent<APIMatchmakingPool>();

        public Bindable<APIMatchmakingPool> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public IEnumerable<APIMatchmakingPool> Pools
        {
            get => dropdown.Items;
            set => dropdown.Items = value;
        }

        private readonly Box background;
        private readonly RankingSelectorDropdown<APIMatchmakingPool> dropdown;

        public MatchmakingPoolSelector()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING },
                    Child = new Container
                    {
                        Margin = new MarginPadding { Vertical = 20 },
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Depth = -float.MaxValue,
                        Child = dropdown = new RankingSelectorDropdown<APIMatchmakingPool>
                        {
                            RelativeSizeAxes = Axes.X,
                            Current = Current
                        }
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Dark3;
        }
    }
}
