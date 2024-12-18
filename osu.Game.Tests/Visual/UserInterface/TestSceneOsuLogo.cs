// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuLogo : OsuTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("Add logo", () =>
            {
                Child = new OsuLogo
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });
        }
    }
}
