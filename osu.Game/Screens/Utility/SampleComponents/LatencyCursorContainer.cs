// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility.SampleComponents
{
    public class LatencyCursorContainer : LatencySampleComponent
    {
        private Circle cursor = null!;

        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        public LatencyCursorContainer()
        {
            Masking = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChild = cursor = new Circle
            {
                Size = new Vector2(40),
                Origin = Anchor.Centre,
                Colour = overlayColourProvider.Colour2,
            };
        }

        protected override bool OnHover(HoverEvent e) => false;

        protected override void UpdateAtLimitedRate(InputState inputState)
        {
            cursor.Colour = inputState.Mouse.IsPressed(MouseButton.Left) ? overlayColourProvider.Content1 : overlayColourProvider.Colour2;

            if (IsActive.Value)
            {
                cursor.Position = ToLocalSpace(inputState.Mouse.Position);
                cursor.Alpha = 1;
            }
            else
            {
                cursor.Alpha = 0;
            }
        }
    }
}
