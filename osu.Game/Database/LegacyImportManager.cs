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
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.IO;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Maintenance;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Database
{
    /// <summary>
    /// Handles migration of legacy user data from osu-stable.
    /// </summary>
    public class LegacyImportManager : Component
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

        [Resolved]
        private DialogOverlay dialogOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private DesktopGameHost desktopGameHost { get; set; }

        private StableStorage cachedStorage;

        public bool SupportsImportFromStable => RuntimeInfo.IsDesktop;

        public async Task ImportFromStableAsync(StableContent content)
        {
            var stableStorage = await getStableStorage().ConfigureAwait(false);
            var importTasks = new List<Task>();

            Task beatmapImportTask = Task.CompletedTask;
            if (content.HasFlagFast(StableContent.Beatmaps))
                importTasks.Add(beatmapImportTask = new LegacyBeatmapImporter(beatmaps).ImportFromStableAsync(stableStorage));

            if (content.HasFlagFast(StableContent.Skins))
                importTasks.Add(new LegacySkinImporter(skins).ImportFromStableAsync(stableStorage));

            if (content.HasFlagFast(StableContent.Collections))
                importTasks.Add(beatmapImportTask.ContinueWith(_ => collections.ImportFromStableAsync(stableStorage), TaskContinuationOptions.OnlyOnRanToCompletion));

            if (content.HasFlagFast(StableContent.Scores))
                importTasks.Add(beatmapImportTask.ContinueWith(_ => new LegacyScoreImporter(scores).ImportFromStableAsync(stableStorage), TaskContinuationOptions.OnlyOnRanToCompletion));

            await Task.WhenAll(importTasks.ToArray()).ConfigureAwait(false);
        }

        private async Task<StableStorage> getStableStorage()
        {
            if (cachedStorage != null)
                return cachedStorage;

            var stableStorage = game.GetStorageForStableInstall();
            if (stableStorage != null)
                return cachedStorage = stableStorage;

            var taskCompletionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            Schedule(() => dialogOverlay.Push(new StableDirectoryLocationDialog(taskCompletionSource)));
            string stablePath = await taskCompletionSource.Task.ConfigureAwait(false);

            return cachedStorage = new StableStorage(stablePath, desktopGameHost);
        }
    }

    [Flags]
    public enum StableContent
    {
        Beatmaps = 1 << 0,
        Scores = 1 << 1,
        Skins = 1 << 2,
        Collections = 1 << 3,
        All = Beatmaps | Scores | Skins | Collections
    }
}
