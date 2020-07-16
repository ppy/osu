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

        public async Task<double> GetDifficultyAsync([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IReadOnlyList<Mod> mods = null,
                                                     CancellationToken cancellationToken = default)
        {
            if (tryGetGetExisting(beatmapInfo, rulesetInfo, mods, out var existing, out var key))
                return existing;

            return await Task.Factory.StartNew(() => getDifficulty(key), cancellationToken, TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously, updateScheduler);
        }

        public double GetDifficulty([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IReadOnlyList<Mod> mods = null)
        {
            if (tryGetGetExisting(beatmapInfo, rulesetInfo, mods, out var existing, out var key))
                return existing;

            return getDifficulty(key);
        }

        private double getDifficulty(in DifficultyCacheLookup key)
        {
            try
            {
                var ruleset = key.RulesetInfo.CreateInstance();
                Debug.Assert(ruleset != null);

                var calculator = ruleset.CreateDifficultyCalculator(beatmapManager.GetWorkingBeatmap(key.BeatmapInfo));
                var attributes = calculator.Calculate(key.Mods);

                difficultyCache.Add(key, attributes.StarRating);
                return attributes.StarRating;
            }
            catch
            {
                difficultyCache.Add(key, 0);
                return 0;
            }
        }

        /// <summary>
        /// Attempts to retrieve an existing difficulty for the combination.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/>.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/>.</param>
        /// <param name="mods">The <see cref="Mod"/>s.</param>
        /// <param name="existingDifficulty">The existing difficulty value, if present.</param>
        /// <param name="key">The <see cref="DifficultyCacheLookup"/> key that was used to perform this lookup. This can be further used to query <see cref="getDifficulty"/>.</param>
        /// <returns>Whether an existing difficulty was found.</returns>
        private bool tryGetGetExisting(BeatmapInfo beatmapInfo, RulesetInfo rulesetInfo, IReadOnlyList<Mod> mods, out double existingDifficulty, out DifficultyCacheLookup key)
        {
            // In the case that the user hasn't given us a ruleset, use the beatmap's default ruleset.
            rulesetInfo ??= beatmapInfo.Ruleset;

            // Difficulty can only be computed if the beatmap is locally available.
            if (beatmapInfo.ID == 0)
            {
                existingDifficulty = 0;
                key = default;

                return true;
            }

            key = new DifficultyCacheLookup(beatmapInfo, rulesetInfo, mods);
            return difficultyCache.TryGetValue(key, out existingDifficulty);
        }

        private readonly struct DifficultyCacheLookup : IEquatable<DifficultyCacheLookup>
        {
            public readonly BeatmapInfo BeatmapInfo;
            public readonly RulesetInfo RulesetInfo;
            public readonly Mod[] Mods;

            public DifficultyCacheLookup(BeatmapInfo beatmapInfo, RulesetInfo rulesetInfo, IEnumerable<Mod> mods)
            {
                BeatmapInfo = beatmapInfo;
                RulesetInfo = rulesetInfo;
                Mods = mods?.OrderBy(m => m.Acronym).ToArray() ?? Array.Empty<Mod>();
            }

            public bool Equals(DifficultyCacheLookup other)
                => BeatmapInfo.Equals(other.BeatmapInfo)
                   && Mods.SequenceEqual(other.Mods);

            public override int GetHashCode()
            {
                var hashCode = new HashCode();

                hashCode.Add(BeatmapInfo.Hash);
                hashCode.Add(RulesetInfo.GetHashCode());
                foreach (var mod in Mods)
                    hashCode.Add(mod.Acronym);

                return hashCode.ToHashCode();
            }
        }
    }
}
