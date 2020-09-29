// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneComposeSelectBox : OsuTestScene
    {
        private Container selectionArea;

        public TestSceneComposeSelectBox()
        {
            ComposeSelectionBox selectionBox = null;

            AddStep("create box", () =>
                Child = selectionArea = new Container
                {
                    Size = new Vector2(300),
                    Position = -new Vector2(150),
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        selectionBox = new ComposeSelectionBox
                        {
                            CanRotate = true,
                            CanScaleX = true,
                            CanScaleY = true,

                            OnRotation = handleRotation,
                            OnScaleX = handleScaleX,
                            OnScaleY = handleScaleY,
                        }
                    }
                });

            AddToggleStep("toggle rotation", state => selectionBox.CanRotate = state);
            AddToggleStep("toggle x", state => selectionBox.CanScaleX = state);
            AddToggleStep("toggle y", state => selectionBox.CanScaleY = state);
        }

        private void handleScaleY(DragEvent e, Anchor reference)
        {
            int direction = (reference & Anchor.y0) > 0 ? -1 : 1;
            if (direction < 0)
                selectionArea.Y += e.Delta.Y;
            selectionArea.Height += direction * e.Delta.Y;
        }

        private void handleScaleX(DragEvent e, Anchor reference)
        {
            int direction = (reference & Anchor.x0) > 0 ? -1 : 1;
            if (direction < 0)
                selectionArea.X += e.Delta.X;
            selectionArea.Width += direction * e.Delta.X;
        }

        private void handleRotation(DragEvent e)
        {
            selectionArea.Rotation += e.Delta.X;
        }
    }
}
