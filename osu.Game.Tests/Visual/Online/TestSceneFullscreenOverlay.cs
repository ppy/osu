// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneFullscreenOverlay : OsuTestScene
    {
        private FullscreenOverlay overlay;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(overlay = new TestFullscreenOverlay());
            AddStep(@"toggle", overlay.ToggleVisibility);
        }

        private class TestFullscreenOverlay : FullscreenOverlay
        {
            public TestFullscreenOverlay()
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }
        }
    }
}
