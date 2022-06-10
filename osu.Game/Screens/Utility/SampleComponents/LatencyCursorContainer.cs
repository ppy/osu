// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility.SampleComponents
{
    public class LatencyCursorContainer : CompositeDrawable
    {
        private Circle cursor = null!;
        private InputManager inputManager = null!;

        private readonly BindableBool isActive;

        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        public LatencyCursorContainer(BindableBool isActive)
        {
            this.isActive = isActive;
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

            inputManager = GetContainingInputManager();
        }

        protected override bool OnHover(HoverEvent e) => false;

        protected override void Update()
        {
            cursor.Colour = inputManager.CurrentState.Mouse.IsPressed(MouseButton.Left) ? overlayColourProvider.Content1 : overlayColourProvider.Colour2;

            if (isActive.Value)
            {
                cursor.Position = ToLocalSpace(inputManager.CurrentState.Mouse.Position);
                cursor.Alpha = 1;
            }
            else
            {
                cursor.Alpha = 0;
            }

            base.Update();
        }
    }
}
