// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialogDangerousButton : PopupDialogButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ButtonColour = colours.Red3;

            ColourContainer.Add(new ConfirmFillBox
            {
                Action = () => Action(),
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
            });
        }

        private class ConfirmFillBox : HoldToConfirmContainer
        {
            private Box box;

            protected override double? HoldActivationDelay => 500;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Child = box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                };

                Progress.BindValueChanged(progress => box.Width = (float)progress.NewValue, true);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                BeginConfirm();
                return true;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (!e.HasAnyButtonPressed)
                    AbortConfirm();
            }
        }
    }
}
