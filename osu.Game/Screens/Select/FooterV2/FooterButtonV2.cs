// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonV2 : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        private const int button_height = 120;
        private const int button_width = 140;
        private const int corner_radius = 10;

        public const float SHEAR_WIDTH = 16;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        protected static readonly Vector2 SHEAR = new Vector2(SHEAR_WIDTH / button_height, 0);

        [BackgroundDependencyLoader]
        private void load()
        {
            Shear = SHEAR;
            Size = new Vector2(button_width, button_height);
            Masking = true;
            CornerRadius = corner_radius;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Background3,
                    RelativeSizeAxes = Axes.Both
                },

                //For elements that should not be sheared.
                new Container
                {
                    Shear = -SHEAR
                }
            };
        }

        public Action Hovered = null!;
        public Action HoverLost = null!;
        public GlobalAction? Hotkey;

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) { }
    }
}
