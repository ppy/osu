// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>
    /// A sidebar area that can be attached to the left or right edge of the screen.
    /// Houses scrolling sectionised content.
    /// </summary>
    internal partial class EditorSidebar : Container<EditorSidebarSection>
    {
        public const float WIDTH = 250;

        public const float PADDING = 3;

        private readonly Box background;

        protected override Container<EditorSidebarSection> Content { get; }

        public EditorSidebar()
        {
            Width = WIDTH;
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuScrollContainer
                {
                    ScrollbarOverlapsContent = false,
                    RelativeSizeAxes = Axes.Both,
                    Child = Content = new FillFlowContainer<EditorSidebarSection>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(PADDING),
                        Direction = FillDirection.Vertical,
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background5;
        }
    }
}
