// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Beatmaps
{
    public class BeatmapDifficultyManager : CompositeDrawable
    {
        // Too many simultaneous updates can lead to stutters. One thread seems to work fine for song select display purposes.
        private readonly ThreadedTaskScheduler updateScheduler = new ThreadedTaskScheduler(1, nameof(BeatmapDifficultyManager));

        private readonly TimedExpiryCache<DifficultyCacheLookup, double> difficultyCache = new TimedExpiryCache<DifficultyCacheLookup, double> { ExpiryTime = 60000 };
        private readonly BeatmapManager beatmapManager;

        public BeatmapDifficultyManager(BeatmapManager beatmapManager)
        {
            this.beatmapManager = beatmapManager;
        }

        public Task<double> GetDifficultyAsync([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IReadOnlyList<Mod> mods = null,
                                               CancellationToken cancellationToken = default)
            => Task.Factory.StartNew(() => GetDifficulty(beatmapInfo, rulesetInfo, mods), cancellationToken,
                TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously,
                updateScheduler);

        public double GetDifficulty([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IReadOnlyList<Mod> mods = null)
        {
            // Difficulty can only be computed if the beatmap is locally available.
            if (beatmapInfo.ID == 0)
                return 0;

            // In the case that the user hasn't given us a ruleset, use the beatmap's default ruleset.
            rulesetInfo ??= beatmapInfo.Ruleset;

            var key = new DifficultyCacheLookup(beatmapInfo, rulesetInfo, mods);
            if (difficultyCache.TryGetValue(key, out var existing))
                return existing;

            try
            {
                var ruleset = rulesetInfo.CreateInstance();
                Debug.Assert(ruleset != null);

                var calculator = ruleset.CreateDifficultyCalculator(beatmapManager.GetWorkingBeatmap(beatmapInfo));
                var attributes = calculator.Calculate(mods?.ToArray() ?? Array.Empty<Mod>());

                difficultyCache.Add(key, attributes.StarRating);
                return attributes.StarRating;
            }
            catch
            {
                difficultyCache.Add(key, 0);
                return 0;
            }
        }

        private readonly struct DifficultyCacheLookup : IEquatable<DifficultyCacheLookup>
        {
            private readonly BeatmapInfo beatmapInfo;
            private readonly RulesetInfo rulesetInfo;
            private readonly IReadOnlyList<Mod> mods;

            public DifficultyCacheLookup(BeatmapInfo beatmapInfo, RulesetInfo rulesetInfo, IEnumerable<Mod> mods)
            {
                this.beatmapInfo = beatmapInfo;
                this.rulesetInfo = rulesetInfo;
                this.mods = mods?.OrderBy(m => m.Acronym).ToArray() ?? Array.Empty<Mod>();
            }

            public bool Equals(DifficultyCacheLookup other)
                => beatmapInfo.Equals(other.beatmapInfo)
                   && mods.SequenceEqual(other.mods);

            public override int GetHashCode()
            {
                var hashCode = new HashCode();

                hashCode.Add(beatmapInfo.Hash);
                hashCode.Add(rulesetInfo.GetHashCode());
                foreach (var mod in mods)
                    hashCode.Add(mod.Acronym);

                return hashCode.ToHashCode();
            }
        }
    }
}
