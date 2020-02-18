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
using osu.Game.Online;
using osu.Framework.Bindables;
using osu.Game.Online.API;

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
        private readonly Bindable<bool> disabled = new Bindable<bool>();

        protected readonly SearchableListHeader<THeader> Header;
        protected readonly SearchableListFilterControl<TTab, TCategory> Filter;
        protected readonly FillFlowContainer ScrollFlow;

        protected override Container<Drawable> Content => scrollContainer;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract Color4 TrianglesColourLight { get; }
        protected abstract Color4 TrianglesColourDark { get; }
        protected abstract SearchableListHeader<THeader> CreateHeader();
        protected abstract SearchableListFilterControl<TTab, TCategory> CreateFilterControl();

        protected abstract string LoginPlaceholder { get; }

        protected SearchableListOverlay(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
            base.Content.Children = new Drawable[]
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
                scrollContainer = new PanelContainer(LoginPlaceholder)
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
                                Padding = new MarginPadding { Horizontal = WIDTH_PADDING, Bottom = 50 },
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

            Header.Disabled.BindTo(disabled);
            Filter.Disabled.BindTo(disabled);
        }

        public override void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                case APIState.Failing:
                case APIState.Connecting:
                    disabled.Value = true;
                    break;

                case APIState.Online:
                    disabled.Value = false;
                    break;
            }
            base.APIStateChanged(api, state);   
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

        private class PanelContainer : OnlineViewContainer
        {
            public PanelContainer(string placeholderMessage)
                : base(placeholderMessage)
            {
            }
        }
    }
}