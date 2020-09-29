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
        public TestSceneComposeSelectBox()
        {
            ComposeSelectionBox selectionBox = null;

            AddStep("create box", () =>
                Child = new Container
                {
                    Size = new Vector2(300),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        selectionBox = new ComposeSelectionBox
                        {
                            CanRotate = true,
                            CanScaleX = true,
                            CanScaleY = true
                        }
                    }
                });

            AddToggleStep("toggle rotation", state => selectionBox.CanRotate = state);
            AddToggleStep("toggle x", state => selectionBox.CanScaleX = state);
            AddToggleStep("toggle y", state => selectionBox.CanScaleY = state);
        }
    }
}
