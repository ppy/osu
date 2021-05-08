// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        public async Task ImportFromStableAsync(StableContent content)
        {
            //var stableStorage = await getStableStorage().ConfigureAwait(false);
            var importTasks = new List<Task>();

            if (content.HasFlagFast(StableContent.Beatmaps))
                importTasks.Add(beatmaps.ImportFromStableAsync());

            if (content.HasFlagFast(StableContent.Collections))
                importTasks.Add(collections.ImportFromStableAsync());

            if (content.HasFlagFast(StableContent.Scores))
                importTasks.Add(scores.ImportFromStableAsync());

            if (content.HasFlagFast(StableContent.Skins))
                importTasks.Add(skins.ImportFromStableAsync());

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
        Beatmaps = 0x1,
        Scores = 0x2,
        Skins = 0x3,
        Collections = 0x4,
        All = Beatmaps | Scores | Skins | Collections
    }
}
