// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuLogo : OsuTestScene
    {
        private OsuLogo? logo;

        [Test]
        public void TestBasic()
        {
            AddStep("Add logo", () =>
            {
                Child = logo = new OsuLogo
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

            AddSliderStep("scale", 0.1, 2, 1, scale =>
            {
                if (logo != null)
                    Child.Scale = new Vector2((float)scale);
            });
        }
    }
}
