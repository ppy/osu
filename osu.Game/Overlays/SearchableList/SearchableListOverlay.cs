﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.SearchableList
{
    public abstract class SearchableListOverlay : WaveOverlayContainer
    {
        public static readonly float WIDTH_PADDING = 80;
    }

    public abstract class SearchableListOverlay<T, U, S> : SearchableListOverlay
    {
        private readonly Container scrollContainer;

        protected readonly SearchableListHeader<T> Header;
        protected readonly SearchableListFilterControl<U, S> Filter;
        protected readonly FillFlowContainer ScrollFlow;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract Color4 TrianglesColourLight { get; }
        protected abstract Color4 TrianglesColourDark { get; }
        protected abstract SearchableListHeader<T> CreateHeader();
        protected abstract SearchableListFilterControl<U, S> CreateFilterControl();

        protected SearchableListOverlay()
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
                scrollContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollbarVisible = false,
                            Children = new[]
                            {
                                ScrollFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = WIDTH_PADDING, Bottom = 50 },
                                    Direction = FillDirection.Vertical,
                                },
                            },
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
            };

            Filter.Search.Exit = Hide;
        }

        protected override void Update()
        {
            base.Update();

            scrollContainer.Padding = new MarginPadding { Top = Header.Height + Filter.Height };
        }

        protected override void OnFocus(InputState state)
        {
            GetContainingInputManager().ChangeFocus(Filter.Search);
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
