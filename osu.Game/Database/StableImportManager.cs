// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.IO;
using osu.Game.Overlays.Settings.Sections.Maintenance;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Database
{    
    public class StableImportManager : Component
    {
        [Resolved]
        private SkinManager skins { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private ScoreManager scores { get; set; }

        [Resolved]
        private CollectionManager collections { get; set; }

        [Resolved]
        private OsuGame game { get; set; }

        [Resolved(CanBeNull = true)]
        private DesktopGameHost desktopGameHost { get; set; }

        private StableStorage cachedStorage;

        public bool SupportsImportFromStable => RuntimeInfo.IsDesktop;

        public async Task ImportFromStableAsync(StableContent content)
        {
            var stableStorage = await getStableStorage().ConfigureAwait(false);
            var importTasks = new List<Task>();

            Task beatmapImportTask = default;
            if (content.HasFlagFast(StableContent.Beatmaps))
                importTasks.Add(beatmapImportTask = beatmaps.ImportFromStableAsync(stableStorage));

            if (content.HasFlagFast(StableContent.Skins))
                importTasks.Add(skins.ImportFromStableAsync(stableStorage));

            if (content.HasFlagFast(StableContent.Collections))
            {
                if (beatmapImportTask != null)
                    importTasks.Add(beatmapImportTask.ContinueWith(_ => collections.ImportFromStableAsync(stableStorage), TaskContinuationOptions.OnlyOnRanToCompletion));
                else
                    importTasks.Add(collections.ImportFromStableAsync(stableStorage));
            }

            if (content.HasFlagFast(StableContent.Scores))
            {
                if (beatmapImportTask != null)
                    importTasks.Add(beatmapImportTask.ContinueWith(_ => scores.ImportFromStableAsync(stableStorage), TaskContinuationOptions.OnlyOnRanToCompletion));
                else
                    importTasks.Add(scores.ImportFromStableAsync(stableStorage));
            }

            await Task.WhenAll(importTasks.ToArray()).ConfigureAwait(false);
        }

        private async Task<StableStorage> getStableStorage()
        {
            var stableStorage = game.GetStorageForStableInstall();
            if (stableStorage != null)
                return stableStorage;

            if (cachedStorage != null)
                return cachedStorage;

            var taskCompletionSource = new TaskCompletionSource<string>();
            Schedule(() => game.PerformFromScreen(t => t.Push(new StableDirectorySelectScreen(taskCompletionSource))));
            var stablePath = await taskCompletionSource.Task.ConfigureAwait(false);

            return cachedStorage = new StableStorage(stablePath, desktopGameHost);
        }

    }

    [Flags]
    public enum StableContent
    {
        Beatmaps = 1,
        Scores = 2,
        Skins = 4,
        Collections = 8,
        All = Beatmaps | Scores | Skins | Collections
    }
}
