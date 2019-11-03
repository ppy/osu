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

            int fireCount = 0;

            Add(overlay = new TestFullscreenOverlay());

            overlay.State.ValueChanged += _ => fireCount++;

            AddStep(@"show", overlay.Show);

            AddAssert("fire count 1", () => fireCount == 1);

            AddStep(@"show again", overlay.Show);

            // this logic is specific to FullscreenOverlay
            AddAssert("fire count 2", () => fireCount == 2);

            AddStep(@"hide", overlay.Hide);

            AddAssert("fire count 3", () => fireCount == 3);
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
