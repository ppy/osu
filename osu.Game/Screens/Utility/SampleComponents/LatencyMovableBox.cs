// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility.SampleComponents
{
    public partial class LatencyMovableBox : LatencySampleComponent
    {
        private Box box = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChild = box = new Box
            {
                Size = new Vector2(40),
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(0.5f),
                Origin = Anchor.Centre,
                Colour = OverlayColourProvider.Colour1,
            };
        }

        protected override bool OnHover(HoverEvent e) => false;

        private double? lastFrameTime;

        protected override void UpdateAtLimitedRate(InputState inputState)
        {
            if (!IsActive.Value)
            {
                lastFrameTime = null;
                box.Colour = OverlayColourProvider.Colour1;
                return;
            }

            if (lastFrameTime != null)
            {
                float movementAmount = (float)(Clock.CurrentTime - lastFrameTime) / 400;

                var buttons = inputState.Keyboard.Keys;

                box.Colour = buttons.HasAnyButtonPressed ? OverlayColourProvider.Content1 : OverlayColourProvider.Colour1;

                foreach (var key in buttons)
                {
                    switch (key)
                    {
                        case Key.F:
                        case Key.Up:
                            box.Y = MathHelper.Clamp(box.Y - movementAmount, 0.1f, 0.9f);
                            break;

                        case Key.J:
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
