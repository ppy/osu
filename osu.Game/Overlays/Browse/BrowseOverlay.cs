// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Overlays.Browse
{
    public abstract class BrowseOverlay : WaveOverlayContainer
    {
        public static readonly float WIDTH_PADDING = 80;
    }

    //todo: naming
    public abstract class BrowseOverlay<T,U> : BrowseOverlay
    {
        private readonly Container scrollContainer;

        protected readonly BrowseHeader<T> Header;
        protected readonly BrowseFilterControl<U> Filter;
        protected readonly FillFlowContainer ScrollFlow;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract Color4 TrianglesColourLight { get; }
        protected abstract Color4 TrianglesColourDark { get; }
        protected abstract BrowseHeader<T> CreateHeader();
        protected abstract BrowseFilterControl<U> CreateFilterControl();

        public BrowseOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = BackgroundColour,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new[]
                    {
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            TriangleScale = 5,
                            ColourLight = TrianglesColourLight,
                            ColourDark = TrianglesColourDark,
                        },
                    },
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        Header = CreateHeader(),
                        Filter = CreateFilterControl(),
                    },
                },
                scrollContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollDraggerVisible = false,
                            Children = new[]
                            {
                                ScrollFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = BrowseOverlay.WIDTH_PADDING },
                                    Direction = FillDirection.Vertical,
                                },
                            },
                        },
                    },
                },
            };

            Filter.Search.Exit = Hide;
        }

        protected override void Update()
        {
            base.Update();

            scrollContainer.Padding = new MarginPadding { Top = Header.Height + Filter.Height };
        }

        protected override bool OnFocus(InputState state)
        {
            Filter.Search.TriggerFocus();
            return false;
        }

        protected override void PopIn()
        {
            base.PopIn();

            Filter.Search.HoldFocus = true;
        }

        protected override void PopOut()
        {
            base.PopOut();

            Filter.Search.HoldFocus = false;
        }
    }
}
