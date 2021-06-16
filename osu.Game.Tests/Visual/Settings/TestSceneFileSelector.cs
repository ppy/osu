// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneFileSelector : OsuTestScene
    {
        [Test]
        public void TestAllFiles()
        {
            AddStep("create", () => Child = new FileSelector { RelativeSizeAxes = Axes.Both });
        }

        [Test]
        public void TestJpgFilesOnly()
        {
            AddStep("create", () => Child = new FileSelector(validFileExtensions: new[] { ".jpg" }) { RelativeSizeAxes = Axes.Both });
        }
    }
}
