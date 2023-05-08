﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
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
    public partial class LegacyImportManager : Component
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private ScoreManager scores { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        private StableStorage? cachedStorage;

        public bool SupportsImportFromStable => RuntimeInfo.IsDesktop;

        public void UpdateStorage(string stablePath) => cachedStorage = new StableStorage(stablePath, gameHost as DesktopGameHost);

        public bool CheckSongsFolderHardLinkAvailability()
        {
            var stableStorage = GetCurrentStableStorage();

            if (stableStorage == null || gameHost is not DesktopGameHost desktopGameHost)
                return false;

            string testExistingPath = stableStorage.GetSongStorage().GetFullPath(string.Empty);
            string testDestinationPath = desktopGameHost.Storage.GetFullPath(string.Empty);

            return HardLinkHelper.CheckAvailability(testDestinationPath, testExistingPath);
        }

        public virtual async Task<int> GetImportCount(StableContent content, CancellationToken cancellationToken)
        {
            var stableStorage = GetCurrentStableStorage();

            if (stableStorage == null)
                return 0;

            cancellationToken.ThrowIfCancellationRequested();

            switch (content)
            {
                case StableContent.Beatmaps:
                    return await new LegacyBeatmapImporter(beatmaps).GetAvailableCount(stableStorage).ConfigureAwait(false);

                case StableContent.Skins:
                    return await new LegacySkinImporter(skins).GetAvailableCount(stableStorage).ConfigureAwait(false);

                case StableContent.Collections:
                    return await new LegacyCollectionImporter(realmAccess).GetAvailableCount(stableStorage).ConfigureAwait(false);

                case StableContent.Scores:
                    return await new LegacyScoreImporter(scores).GetAvailableCount(stableStorage).ConfigureAwait(false);

                default:
                    throw new ArgumentException($"Only one {nameof(StableContent)} flag should be specified.");
            }
        }

        public async Task ImportFromStableAsync(StableContent content, bool interactiveLocateIfNotFound = true)
        {
            var stableStorage = GetCurrentStableStorage();

            if (stableStorage == null)
            {
                if (!interactiveLocateIfNotFound)
                    return;

                var taskCompletionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                Schedule(() => dialogOverlay.Push(new StableDirectoryLocationDialog(taskCompletionSource)));
                string stablePath = await taskCompletionSource.Task.ConfigureAwait(false);

                UpdateStorage(stablePath);
                stableStorage = GetCurrentStableStorage();
            }

            if (stableStorage == null)
                return;

            var importTasks = new List<Task>();

            Task beatmapImportTask = Task.CompletedTask;
            if (content.HasFlagFast(StableContent.Beatmaps))
                importTasks.Add(beatmapImportTask = new LegacyBeatmapImporter(beatmaps).ImportFromStableAsync(stableStorage));

            if (content.HasFlagFast(StableContent.Skins))
                importTasks.Add(new LegacySkinImporter(skins).ImportFromStableAsync(stableStorage));

            if (content.HasFlagFast(StableContent.Collections))
            {
                importTasks.Add(beatmapImportTask.ContinueWith(_ => new LegacyCollectionImporter(realmAccess)
                {
                    // Other legacy importers import via model managers which handle the posting of notifications.
                    // Collections are an exception.
                    PostNotification = n => notifications?.Post(n)
                }.ImportFromStorage(stableStorage), TaskContinuationOptions.OnlyOnRanToCompletion));
            }

            if (content.HasFlagFast(StableContent.Scores))
                importTasks.Add(beatmapImportTask.ContinueWith(_ => new LegacyScoreImporter(scores).ImportFromStableAsync(stableStorage), TaskContinuationOptions.OnlyOnRanToCompletion));

            await Task.WhenAll(importTasks.ToArray()).ConfigureAwait(false);
        }

        public StableStorage? GetCurrentStableStorage()
        {
            if (cachedStorage != null)
                return cachedStorage;

            var stableStorage = game?.GetStorageForStableInstall();
            if (stableStorage != null)
                return cachedStorage = stableStorage;

            return null;
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
