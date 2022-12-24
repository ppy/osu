// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Overlays.Notifications;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneStartupImport : OsuGameTestScene
    {
        private string? importFilename;

        protected override TestOsuGame CreateTestGame() => new TestOsuGame(LocalStorage, API, new[] { importFilename });

        public override void SetUpSteps()
        {
            AddStep("Prepare import beatmap", () => importFilename = TestResources.GetTestBeatmapForImport());

            base.SetUpSteps();
        }

        [Test]
        public void TestImportCreatedNotification()
        {
            AddUntilStep("Import notification was presented", () => Game.Notifications.ChildrenOfType<ProgressCompletionNotification>().Count() == 1);
        }
    }
}
