// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneModSelectScreen : OsuTestScene
    {
        [Test]
        public void TestModSelectScreen()
        {
            ModSelectScreen modSelectScreen = null;

            AddStep("create screen", () => Child = modSelectScreen = new ModSelectScreen
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible }
            });

            AddToggleStep("toggle state", visible => modSelectScreen.State.Value = visible ? Visibility.Visible : Visibility.Hidden);
        }
    }
}
