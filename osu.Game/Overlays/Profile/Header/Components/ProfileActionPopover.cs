// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public abstract partial class ProfileActionPopover : OsuPopover
    {
        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private FillFlowContainer container = null!;

        protected ProfileActionPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Colour = colourProvider.Background6;

            AllowableAnchors = [Anchor.BottomCentre, Anchor.TopCentre];

            Child = container = new FillFlowContainer
            {
                Width = 160,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
            };
        }

        public ProfilePopoverAction[] Actions { set => container.Children = value; }
    }
}
