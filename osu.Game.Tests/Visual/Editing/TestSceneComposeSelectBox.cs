// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneComposeSelectBox : OsuTestScene
    {
        private Container selectionArea;

        public TestSceneComposeSelectBox()
        {
            SelectionBox selectionBox = null;

            AddStep("create box", () =>
                Child = selectionArea = new Container
                {
                    Size = new Vector2(400),
                    Position = -new Vector2(150),
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        selectionBox = new SelectionBox
                        {
                            CanRotate = true,
                            CanScaleX = true,
                            CanScaleY = true,

                            OnRotation = handleRotation,
                            OnScale = handleScale
                        }
                    }
                });

            AddToggleStep("toggle rotation", state => selectionBox.CanRotate = state);
            AddToggleStep("toggle x", state => selectionBox.CanScaleX = state);
            AddToggleStep("toggle y", state => selectionBox.CanScaleY = state);
        }

        private bool handleScale(Vector2 amount, Anchor reference)
        {
            if ((reference & Anchor.y1) == 0)
            {
                int directionY = (reference & Anchor.y0) > 0 ? -1 : 1;
                if (directionY < 0)
                    selectionArea.Y += amount.Y;
                selectionArea.Height += directionY * amount.Y;
            }

            if ((reference & Anchor.x1) == 0)
            {
                int directionX = (reference & Anchor.x0) > 0 ? -1 : 1;
                if (directionX < 0)
                    selectionArea.X += amount.X;
                selectionArea.Width += directionX * amount.X;
            }

            return true;
        }

        private bool handleRotation(float angle)
        {
            // kinda silly and wrong, but just showing that the drag handles work.
            selectionArea.Rotation += angle;
            return true;
        }
    }
}
