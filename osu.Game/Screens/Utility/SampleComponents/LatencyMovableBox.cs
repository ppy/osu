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
    public class LatencyMovableBox : CompositeDrawable
    {
        private Box box = null!;
        private InputManager inputManager = null!;

        private readonly BindableBool isActive;

        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        public LatencyMovableBox(BindableBool isActive)
        {
            this.isActive = isActive;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            InternalChild = box = new Box
            {
                Size = new Vector2(40),
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(0.5f),
                Origin = Anchor.Centre,
                Colour = overlayColourProvider.Colour1,
            };
        }

        protected override bool OnHover(HoverEvent e) => false;

        private double? lastFrameTime;

        protected override void Update()
        {
            base.Update();

            if (!isActive.Value)
            {
                lastFrameTime = null;
                box.Colour = overlayColourProvider.Colour1;
                return;
            }

            if (lastFrameTime != null)
            {
                float movementAmount = (float)(Clock.CurrentTime - lastFrameTime) / 400;

                var buttons = inputManager.CurrentState.Keyboard.Keys;

                box.Colour = buttons.HasAnyButtonPressed ? overlayColourProvider.Content1 : overlayColourProvider.Colour1;

                foreach (var key in buttons)
                {
                    switch (key)
                    {
                        case Key.F:
                        case Key.Up:
                            box.Y = MathHelper.Clamp(box.Y - movementAmount, 0.1f, 0.9f);
                            break;

                        case Key.K:
                        case Key.Down:
                            box.Y = MathHelper.Clamp(box.Y + movementAmount, 0.1f, 0.9f);
                            break;

                        case Key.Z:
                        case Key.Left:
                            box.X = MathHelper.Clamp(box.X - movementAmount, 0.1f, 0.9f);
                            break;

                        case Key.X:
                        case Key.Right:
                            box.X = MathHelper.Clamp(box.X + movementAmount, 0.1f, 0.9f);
                            break;
                    }
                }
            }

            lastFrameTime = Clock.CurrentTime;
        }
    }
}
