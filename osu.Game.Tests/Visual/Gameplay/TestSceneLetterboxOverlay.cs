// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Play.Break;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneLetterboxOverlay : OsuTestScene
    {
        public TestSceneLetterboxOverlay()
        {
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new LetterboxOverlay()
            });
        }
    }
}
