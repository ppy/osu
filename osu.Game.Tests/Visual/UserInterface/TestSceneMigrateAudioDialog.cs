// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneMigrateAudioDialog : OsuManualInputManagerTestScene
    {
        private DialogOverlay overlay = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create dialog overlay", () => Child = overlay = new DialogOverlay());
        }

        [Test]
        public void TestWasUsing()
        {
            AddStep("create dialog", () =>
            {
                overlay.Push(new MigrateNewAudioDialog(true));
            });
        }

        [Test]
        public void TestNotUsing()
        {
            AddStep("create dialog", () =>
            {
                overlay.Push(new MigrateNewAudioDialog(false));
            });
        }
    }
}
