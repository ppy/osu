// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Overlays.SearchableList
{
    public abstract class SearchableListOverlay : FullscreenOverlay
    {
        public const float WIDTH_PADDING = 80;

        protected SearchableListOverlay(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
        }
    }

    public abstract class SearchableListOverlay<THeader, TTab, TCategory> : SearchableListOverlay
        where THeader : struct, Enum
        where TTab : struct, Enum
        where TCategory : struct, Enum
    {
        private readonly Container scrollContainer;

        protected readonly SearchableListHeader<THeader> Header;
        protected readonly SearchableListFilterControl<TTab, TCategory> Filter;
        protected readonly FillFlowContainer ScrollFlow;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract Color4 TrianglesColourLight { get; }
        protected abstract Color4 TrianglesColourDark { get; }
        protected abstract SearchableListHeader<THeader> CreateHeader();
        protected abstract SearchableListFilterControl<TTab, TCategory> CreateFilterControl();

        protected SearchableListOverlay(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
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
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Child = new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollbarVisible = false,
                            Child = ScrollFlow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Horizontal = 10, Bottom = 50 },
                                Direction = FillDirection.Vertical,
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
        }

        protected override void Update()
        {
            base.Update();

            scrollContainer.Padding = new MarginPadding { Top = Header.Height + Filter.Height };
        }

        protected override void OnFocus(FocusEvent e)
        {
            Filter.Search.TakeFocus();
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
