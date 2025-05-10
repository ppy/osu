// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneLocallyModifyingOnlineBeatmaps : EditorSavingTestScene
    {
        public override void SetUpSteps()
        {
            CreateInitialBeatmap = () =>
            {
                var importedSet = Game.BeatmapManager.Import(new ImportTask(TestResources.GetTestBeatmapForImport())).GetResultSafely();
                return Game.BeatmapManager.GetWorkingBeatmap(importedSet!.Value.Beatmaps.First());
            };

            base.SetUpSteps();
        }

        [Test]
        public void TestLocallyModifyingOnlineBeatmap()
        {
            string initialHash = string.Empty;
            AddAssert("editor beatmap has online ID", () => EditorBeatmap.BeatmapInfo.OnlineID, () => Is.GreaterThan(0));
            AddStep("store hash for later", () => initialHash = EditorBeatmap.BeatmapInfo.MD5Hash);

            AddStep("delete first hitobject", () => EditorBeatmap.RemoveAt(0));
            SaveEditor();

            ReloadEditorToSameBeatmap();
            AddAssert("beatmap marked as locally modified", () => EditorBeatmap.BeatmapInfo.Status, () => Is.EqualTo(BeatmapOnlineStatus.LocallyModified));
            AddAssert("beatmap hash changed", () => EditorBeatmap.BeatmapInfo.MD5Hash, () => Is.Not.EqualTo(initialHash));
        }
    }
}
