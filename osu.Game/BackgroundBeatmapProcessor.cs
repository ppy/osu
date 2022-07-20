// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Task.Run(() =>
            {
                Logger.Log("Beginning background beatmap processing..");
                checkForOutdatedStarRatings();
                processBeatmapSetsWithMissingMetrics();
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
                                b.StarRating = 0;
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
            // TODO: rate limit and pause processing during gameplay.

            HashSet<Guid> beatmapSetIds = new HashSet<Guid>();

            realmAccess.Run(r =>
            {
                foreach (var b in r.All<BeatmapInfo>().Where(b => b.StarRating == 0 || (b.OnlineID > 0 && b.LastOnlineUpdate == null)))
                {
                    Debug.Assert(b.BeatmapSet != null);
                    beatmapSetIds.Add(b.BeatmapSet.ID);
                }
            });

            foreach (var id in beatmapSetIds)
            {
                realmAccess.Run(r =>
                {
                    var set = r.Find<BeatmapSetInfo>(id);

                    if (set != null)
                    {
                        Logger.Log($"Background processing {set}");
                        beatmapUpdater.Process(set);
                    }
                });
            }
        }
    }
}
