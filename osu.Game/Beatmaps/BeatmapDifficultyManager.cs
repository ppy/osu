// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Lists;
using osu.Framework.Threading;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Beatmaps
{
    public class BeatmapDifficultyManager : CompositeDrawable
    {
        // Too many simultaneous updates can lead to stutters. One thread seems to work fine for song select display purposes.
        private readonly ThreadedTaskScheduler updateScheduler = new ThreadedTaskScheduler(1, nameof(BeatmapDifficultyManager));

        // A cache that keeps references to BeatmapInfos for 60sec.
        private readonly ConcurrentDictionary<DifficultyCacheLookup, StarDifficulty> difficultyCache = new ConcurrentDictionary<DifficultyCacheLookup, StarDifficulty>();

        // All bindables that should be updated along with the current ruleset + mods.
        private readonly LockedWeakList<BindableStarDifficulty> trackedBindables = new LockedWeakList<BindableStarDifficulty>();

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private Bindable<RulesetInfo> currentRuleset { get; set; }

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> currentMods { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentRuleset.BindValueChanged(_ => updateTrackedBindables());
            currentMods.BindValueChanged(_ => updateTrackedBindables(), true);
        }

        /// <summary>
        /// Retrieves an <see cref="IBindable{StarDifficulty}"/> containing the star difficulty of a <see cref="BeatmapInfo"/> with a given <see cref="RulesetInfo"/> and <see cref="Mod"/> combination.
        /// </summary>
        /// <remarks>
        /// This <see cref="Bindable{StarDifficulty}"/> will not update to follow the currently-selected ruleset and mods.
        /// </remarks>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to get the difficulty with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to get the difficulty with.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops updating the star difficulty for the given <see cref="BeatmapInfo"/>.</param>
        /// <returns>An <see cref="IBindable{StarDifficulty}"/> that is updated to contain the star difficulty when it becomes available.</returns>
        public IBindable<StarDifficulty> GetUntrackedBindable([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IReadOnlyList<Mod> mods = null,
                                                              CancellationToken cancellationToken = default)
            => createBindable(beatmapInfo, rulesetInfo, mods, cancellationToken);

        /// <summary>
        /// Retrieves a <see cref="IBindable{StarDifficulty}"/> containing the star difficulty of a <see cref="BeatmapInfo"/> that follows the user's currently-selected ruleset and mods.
        /// </summary>
        /// <remarks>
        /// Ensure to hold a local reference of the returned <see cref="Bindable{StarDifficulty}"/> in order to receive value-changed events.
        /// </remarks>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops updating the star difficulty for the given <see cref="BeatmapInfo"/>.</param>
        /// <returns>An <see cref="IBindable{StarDifficulty}"/> that is updated to contain the star difficulty when it becomes available, or when the currently-selected ruleset and mods change.</returns>
        public IBindable<StarDifficulty> GetTrackedBindable([NotNull] BeatmapInfo beatmapInfo, CancellationToken cancellationToken = default)
        {
            var bindable = createBindable(beatmapInfo, currentRuleset.Value, currentMods.Value, cancellationToken);
            trackedBindables.Add(bindable);
            return bindable;
        }

        /// <summary>
        /// Retrieves the difficulty of a <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to get the difficulty with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to get the difficulty with.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops computing the star difficulty.</param>
        /// <returns>The <see cref="StarDifficulty"/>.</returns>
        public async Task<StarDifficulty> GetDifficultyAsync([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IReadOnlyList<Mod> mods = null,
                                                             CancellationToken cancellationToken = default)
        {
            if (tryGetGetExisting(beatmapInfo, rulesetInfo, mods, out var existing, out var key))
                return existing;

            return await Task.Factory.StartNew(() => computeDifficulty(key, beatmapInfo, rulesetInfo), cancellationToken,
                TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously, updateScheduler);
        }

        /// <summary>
        /// Retrieves the difficulty of a <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to get the difficulty with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to get the difficulty with.</param>
        /// <returns>The <see cref="StarDifficulty"/>.</returns>
        public StarDifficulty GetDifficulty([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IReadOnlyList<Mod> mods = null)
        {
            if (tryGetGetExisting(beatmapInfo, rulesetInfo, mods, out var existing, out var key))
                return existing;

            return computeDifficulty(key, beatmapInfo, rulesetInfo);
        }

        private CancellationTokenSource trackedUpdateCancellationSource;

        /// <summary>
        /// Updates all tracked <see cref="BindableStarDifficulty"/> using the current ruleset and mods.
        /// </summary>
        private void updateTrackedBindables()
        {
            trackedUpdateCancellationSource?.Cancel();
            trackedUpdateCancellationSource = new CancellationTokenSource();

            foreach (var b in trackedBindables)
            {
                if (trackedUpdateCancellationSource.IsCancellationRequested)
                    break;

                using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(trackedUpdateCancellationSource.Token, b.CancellationToken))
                    updateBindable(b, currentRuleset.Value, currentMods.Value, linkedSource.Token);
            }
        }

        /// <summary>
        /// Updates the value of a <see cref="BindableStarDifficulty"/> with a given ruleset + mods.
        /// </summary>
        /// <param name="bindable">The <see cref="BindableStarDifficulty"/> to update.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to update with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to update with.</param>
        /// <param name="cancellationToken">A token that may be used to cancel this update.</param>
        private void updateBindable([NotNull] BindableStarDifficulty bindable, [CanBeNull] RulesetInfo rulesetInfo, [CanBeNull] IReadOnlyList<Mod> mods, CancellationToken cancellationToken = default)
        {
            GetDifficultyAsync(bindable.Beatmap, rulesetInfo, mods, cancellationToken).ContinueWith(t =>
            {
                // We're on a threadpool thread, but we should exit back to the update thread so consumers can safely handle value-changed events.
                Schedule(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                        bindable.Value = t.Result;
                });
            }, cancellationToken);
        }

        /// <summary>
        /// Creates a new <see cref="BindableStarDifficulty"/> and triggers an initial value update.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> that star difficulty should correspond to.</param>
        /// <param name="initialRulesetInfo">The initial <see cref="RulesetInfo"/> to get the difficulty with.</param>
        /// <param name="initialMods">The initial <see cref="Mod"/>s to get the difficulty with.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops updating the star difficulty for the given <see cref="BeatmapInfo"/>.</param>
        /// <returns>The <see cref="BindableStarDifficulty"/>.</returns>
        private BindableStarDifficulty createBindable([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo initialRulesetInfo, [CanBeNull] IReadOnlyList<Mod> initialMods,
                                                      CancellationToken cancellationToken)
        {
            var bindable = new BindableStarDifficulty(beatmapInfo, cancellationToken);
            updateBindable(bindable, initialRulesetInfo, initialMods, cancellationToken);
            return bindable;
        }

        /// <summary>
        /// Computes the difficulty defined by a <see cref="DifficultyCacheLookup"/> key, and stores it to the timed cache.
        /// </summary>
        /// <param name="key">The <see cref="DifficultyCacheLookup"/> that defines the computation parameters.</param>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to compute the difficulty of.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to compute the difficulty with.</param>
        /// <returns>The <see cref="StarDifficulty"/>.</returns>
        private StarDifficulty computeDifficulty(in DifficultyCacheLookup key, BeatmapInfo beatmapInfo, RulesetInfo rulesetInfo)
        {
            try
            {
                var ruleset = rulesetInfo.CreateInstance();
                Debug.Assert(ruleset != null);

                var calculator = ruleset.CreateDifficultyCalculator(beatmapManager.GetWorkingBeatmap(beatmapInfo));
                var attributes = calculator.Calculate(key.Mods);

                return difficultyCache[key] = new StarDifficulty(attributes.StarRating);
            }
            catch
            {
                return difficultyCache[key] = new StarDifficulty(0);
            }
        }

        /// <summary>
        /// Attempts to retrieve an existing difficulty for the combination.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/>.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/>.</param>
        /// <param name="mods">The <see cref="Mod"/>s.</param>
        /// <param name="existingDifficulty">The existing difficulty value, if present.</param>
        /// <param name="key">The <see cref="DifficultyCacheLookup"/> key that was used to perform this lookup. This can be further used to query <see cref="computeDifficulty"/>.</param>
        /// <returns>Whether an existing difficulty was found.</returns>
        private bool tryGetGetExisting(BeatmapInfo beatmapInfo, RulesetInfo rulesetInfo, IReadOnlyList<Mod> mods, out StarDifficulty existingDifficulty, out DifficultyCacheLookup key)
        {
            // In the case that the user hasn't given us a ruleset, use the beatmap's default ruleset.
            rulesetInfo ??= beatmapInfo.Ruleset;

            // Difficulty can only be computed if the beatmap and ruleset are locally available.
            if (beatmapInfo.ID == 0 || rulesetInfo.ID == null)
            {
                existingDifficulty = new StarDifficulty(0);
                key = default;

                return true;
            }

            key = new DifficultyCacheLookup(beatmapInfo.ID, rulesetInfo.ID.Value, mods);
            return difficultyCache.TryGetValue(key, out existingDifficulty);
        }

        private readonly struct DifficultyCacheLookup : IEquatable<DifficultyCacheLookup>
        {
            public readonly int BeatmapId;
            public readonly int RulesetId;
            public readonly Mod[] Mods;

            public DifficultyCacheLookup(int beatmapId, int rulesetId, IEnumerable<Mod> mods)
            {
                BeatmapId = beatmapId;
                RulesetId = rulesetId;
                Mods = mods?.OrderBy(m => m.Acronym).ToArray() ?? Array.Empty<Mod>();
            }

            public bool Equals(DifficultyCacheLookup other)
                => BeatmapId == other.BeatmapId
                   && RulesetId == other.RulesetId
                   && Mods.SequenceEqual(other.Mods);

            public override int GetHashCode()
            {
                var hashCode = new HashCode();

                hashCode.Add(BeatmapId);
                hashCode.Add(RulesetId);
                foreach (var mod in Mods)
                    hashCode.Add(mod.Acronym);

                return hashCode.ToHashCode();
            }
        }

        private class BindableStarDifficulty : Bindable<StarDifficulty>
        {
            public readonly BeatmapInfo Beatmap;
            public readonly CancellationToken CancellationToken;

            public BindableStarDifficulty(BeatmapInfo beatmap, CancellationToken cancellationToken)
            {
                Beatmap = beatmap;
                CancellationToken = cancellationToken;
            }
        }
    }

    public readonly struct StarDifficulty
    {
        public readonly double Stars;

        public StarDifficulty(double stars)
        {
            Stars = stars;

            // Todo: Add more members (BeatmapInfo.DifficultyRating? Attributes? Etc...)
        }
    }
}
