// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game
{
    public class BackgroundBeatmapProcessor : Component
    {
        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private BeatmapUpdater beatmapUpdater { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> gameBeatmap { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo? localUserPlayInfo { get; set; }

        protected virtual int TimeToSleepDuringGameplay => 30000;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Run(() =>
            {
                Logger.Log("Beginning background beatmap processing..");
                checkForOutdatedStarRatings();
                processBeatmapSetsWithMissingMetrics();
            }).ContinueWith(t =>
            {
                if (t.Exception?.InnerException is ObjectDisposedException)
                {
                    Logger.Log("Finished background aborted during shutdown");
                    return;
                }

                Logger.Log("Finished background beatmap processing!");
            });
        }

        /// <summary>
        /// Check whether the databased difficulty calculation version matches the latest ruleset provided version.
        /// If it doesn't, clear out any existing difficulties so they can be incrementally recalculated.
        /// </summary>
        private void checkForOutdatedStarRatings()
        {
            foreach (var ruleset in rulesetStore.AvailableRulesets)
            {
                // beatmap being passed in is arbitrary here. just needs to be non-null.
                int currentVersion = ruleset.CreateInstance().CreateDifficultyCalculator(gameBeatmap.Value).Version;

                if (ruleset.LastAppliedDifficultyVersion < currentVersion)
                {
                    Logger.Log($"Resetting star ratings for {ruleset.Name} (difficulty calculation version updated from {ruleset.LastAppliedDifficultyVersion} to {currentVersion})");

                    int countReset = 0;

                    realmAccess.Write(r =>
                    {
                        foreach (var b in r.All<BeatmapInfo>())
                        {
                            if (b.Ruleset.ShortName == ruleset.ShortName)
                            {
                                b.StarRating = -1;
                                countReset++;
                            }
                        }

                        r.Find<RulesetInfo>(ruleset.ShortName).LastAppliedDifficultyVersion = currentVersion;
                    });

                    Logger.Log($"Finished resetting {countReset} beatmap sets for {ruleset.Name}");
                }
            }
        }

        private void processBeatmapSetsWithMissingMetrics()
        {
            HashSet<Guid> beatmapSetIds = new HashSet<Guid>();

            Logger.Log("Querying for beatmap sets to reprocess...");

            realmAccess.Run(r =>
            {
                foreach (var b in r.All<BeatmapInfo>().Where(b => b.StarRating < 0 || (b.OnlineID > 0 && b.LastOnlineUpdate == null)))
                {
                    Debug.Assert(b.BeatmapSet != null);
                    beatmapSetIds.Add(b.BeatmapSet.ID);
                }
            });

            Logger.Log($"Found {beatmapSetIds.Count} beatmap sets which require reprocessing.");

            int i = 0;

            foreach (var id in beatmapSetIds)
            {
                while (localUserPlayInfo?.IsPlaying.Value == true)
                {
                    Logger.Log("Background processing sleeping due to active gameplay...");
                    Thread.Sleep(TimeToSleepDuringGameplay);
                }

                realmAccess.Run(r =>
                {
                    var set = r.Find<BeatmapSetInfo>(id);

                    if (set != null)
                    {
                        try
                        {
                            Logger.Log($"Background processing {set} ({++i} / {beatmapSetIds.Count})");
                            beatmapUpdater.Process(set);
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Background processing failed on {set}: {e}");
                        }
                    }
                });
            }
        }
    }
}
