// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectRemoveHandling : SongSelectTestScene
    {
        private BeatmapInfo? selectedBeatmap => (BeatmapInfo?)Carousel.CurrentSelection;
        private BeatmapSetInfo? selectedBeatmapSet => selectedBeatmap?.BeatmapSet;

        [Test]
        public void TestRemoveBeatmapSet()
        {
            LoadSongSelect();
            importMaps();

            checkPanelsArrived<PanelBeatmapSet>();
            clickVisiblePanel<PanelBeatmapSet>(2);
            AddUntilStep("wait for beatmap to be selected", () => selectedBeatmapSet != null);

            BeatmapSetInfo deletedBeatmap = null!;
            AddStep("remove beatmap", () => Beatmaps.Delete(deletedBeatmap = selectedBeatmapSet!));
            waitForFiltering();

            AddAssert("selected different beatmap", () => selectedBeatmapSet,
                () => Is.Not.EqualTo(deletedBeatmap));
            checkPanelSelected<PanelBeatmapSet>(2);
        }

        [Test]
        public void TestHideBeatmap()
        {
            LoadSongSelect();
            importMaps();

            checkPanelsArrived<PanelBeatmapSet>();
            clickVisiblePanel<PanelBeatmapSet>(2);
            AddUntilStep("wait for beatmap to be selected", () => selectedBeatmapSet != null);

            checkPanelsArrived<PanelBeatmap>();
            clickVisiblePanel<PanelBeatmap>(2);

            BeatmapInfo hiddenBeatmap = null!;
            AddStep("hide selected", () => Beatmaps.Hide(hiddenBeatmap = selectedBeatmap!));
            waitForFiltering();

            AddAssert("selected different beatmap", () => selectedBeatmap,
                () => Is.Not.EqualTo(hiddenBeatmap));
            checkPanelSelected<PanelBeatmap>(2);
        }

        private void importMaps()
        {
            var importedMaps = new List<BeatmapSetInfo>();
            AddStep("import test maps", () =>
            {
                importedMaps.Clear();

                for (int i = 0; i < 5; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(5);
                    importedMaps.Add(set);
                    Beatmaps.Import(set);
                }
            });
            AddUntilStep("wait for beatmap import", () =>
            {
                var usableMaps = Beatmaps.GetAllUsableBeatmapSets();
                return importedMaps.All(b => usableMaps.Contains(b));
            });
        }

        private void waitForFiltering()
            => AddUntilStep("filtering finished", () => Carousel.IsFiltering, () => Is.False);

        private void checkPanelsArrived<T>()
            where T : Drawable
            => AddUntilStep($"wait for {typeof(T).Name} panels to appear", () => Carousel.ChildrenOfType<T>().Any());

        private void clickVisiblePanel<T>(int index)
            where T : Drawable
            => AddStep($"click panel at index {index}", () => allPanels<T>().ElementAt(index).ChildrenOfType<Panel>().Single().TriggerClick());

        private void checkPanelSelected<T>(int index)
            where T : Drawable
            => AddAssert($"selected panel at index {index}", getActivePanelIndex<T>, () => Is.EqualTo(index));

        private IEnumerable<T> allPanels<T>()
            where T : Drawable
            => Carousel.ChildrenOfType<T>().OrderBy(p => p.Y);

        private int getActivePanelIndex<T>()
            where T : Drawable
            => allPanels<T>().ToList().FindIndex(p => p switch
            {
                PanelBeatmap pb => pb.Selected.Value,
                Panel pbs => pbs.Expanded.Value,
                _ => throw new InvalidOperationException(),
            });
    }
}
