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
using osu.Framework.Bindables;
using osu.Framework.Lists;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A component which performs and acts as a central cache for difficulty calculations of beatmap/ruleset/mod combinations.
    /// Currently not persisted between game sessions.
    /// </summary>
    public class BeatmapDifficultyCache : MemoryCachingComponent<BeatmapDifficultyCache.DifficultyCacheLookup, StarDifficulty>
    {
        // Too many simultaneous updates can lead to stutters. One thread seems to work fine for song select display purposes.
        private readonly ThreadedTaskScheduler updateScheduler = new ThreadedTaskScheduler(1, nameof(BeatmapDifficultyCache));

        /// <summary>
        /// All bindables that should be updated along with the current ruleset + mods.
        /// </summary>
        private readonly WeakList<BindableStarDifficulty> trackedBindables = new WeakList<BindableStarDifficulty>();

        /// <summary>
        /// Cancellation sources used by tracked bindables.
        /// </summary>
        private readonly List<CancellationTokenSource> linkedCancellationSources = new List<CancellationTokenSource>();

        /// <summary>
        /// Lock to be held when operating on <see cref="trackedBindables"/> or <see cref="linkedCancellationSources"/>.
        /// </summary>
        private readonly object bindableUpdateLock = new object();

        private CancellationTokenSource trackedUpdateCancellationSource;

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
        /// Retrieves a bindable containing the star difficulty of a <see cref="BeatmapInfo"/> that follows the currently-selected ruleset and mods.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops updating the star difficulty for the given <see cref="BeatmapInfo"/>.</param>
        /// <returns>A bindable that is updated to contain the star difficulty when it becomes available.</returns>
        public IBindable<StarDifficulty> GetBindableDifficulty([NotNull] BeatmapInfo beatmapInfo, CancellationToken cancellationToken = default)
        {
            var bindable = createBindable(beatmapInfo, currentRuleset.Value, currentMods.Value, cancellationToken);

            lock (bindableUpdateLock)
                trackedBindables.Add(bindable);

            return bindable;
        }

        /// <summary>
        /// Retrieves a bindable containing the star difficulty of a <see cref="BeatmapInfo"/> with a given <see cref="RulesetInfo"/> and <see cref="Mod"/> combination.
        /// </summary>
        /// <remarks>
        /// The bindable will not update to follow the currently-selected ruleset and mods.
        /// </remarks>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to get the difficulty with. If <c>null</c>, the <paramref name="beatmapInfo"/>'s ruleset is used.</param>
        /// <param name="mods">The <see cref="Mod"/>s to get the difficulty with. If <c>null</c>, no mods will be assumed.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops updating the star difficulty for the given <see cref="BeatmapInfo"/>.</param>
        /// <returns>A bindable that is updated to contain the star difficulty when it becomes available.</returns>
        public IBindable<StarDifficulty> GetBindableDifficulty([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo, [CanBeNull] IEnumerable<Mod> mods,
                                                               CancellationToken cancellationToken = default)
            => createBindable(beatmapInfo, rulesetInfo, mods, cancellationToken);

        /// <summary>
        /// Retrieves the difficulty of a <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the difficulty of.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to get the difficulty with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to get the difficulty with.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops computing the star difficulty.</param>
        /// <returns>The <see cref="StarDifficulty"/>.</returns>
        public Task<StarDifficulty> GetDifficultyAsync([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo rulesetInfo = null, [CanBeNull] IEnumerable<Mod> mods = null,
                                                       CancellationToken cancellationToken = default)
        {
            // In the case that the user hasn't given us a ruleset, use the beatmap's default ruleset.
            rulesetInfo ??= beatmapInfo.Ruleset;

            // Difficulty can only be computed if the beatmap and ruleset are locally available.
            if (beatmapInfo.ID == 0 || rulesetInfo.ID == null)
            {
                // If not, fall back to the existing star difficulty (e.g. from an online source).
                return Task.FromResult(new StarDifficulty(beatmapInfo.StarDifficulty, beatmapInfo.MaxCombo ?? 0));
            }

            return GetAsync(new DifficultyCacheLookup(beatmapInfo, rulesetInfo, mods), cancellationToken);
        }

        protected override Task<StarDifficulty> ComputeValueAsync(DifficultyCacheLookup lookup, CancellationToken token = default)
        {
            return Task.Factory.StartNew(() =>
            {
                if (CheckExists(lookup, out var existing))
                    return existing;

                return computeDifficulty(lookup);
            }, token, TaskCreationOptions.HideScheduler | TaskCreationOptions.RunContinuationsAsynchronously, updateScheduler);
        }

        /// <summary>
        /// Retrieves the <see cref="DifficultyRating"/> that describes a star rating.
        /// </summary>
        /// <remarks>
        /// For more information, see: https://osu.ppy.sh/help/wiki/Difficulties
        /// </remarks>
        /// <param name="starRating">The star rating.</param>
        /// <returns>The <see cref="DifficultyRating"/> that best describes <paramref name="starRating"/>.</returns>
        public static DifficultyRating GetDifficultyRating(double starRating)
        {
            if (Precision.AlmostBigger(starRating, 6.5, 0.005))
                return DifficultyRating.ExpertPlus;

            if (Precision.AlmostBigger(starRating, 5.3, 0.005))
                return DifficultyRating.Expert;

            if (Precision.AlmostBigger(starRating, 4.0, 0.005))
                return DifficultyRating.Insane;

            if (Precision.AlmostBigger(starRating, 2.7, 0.005))
                return DifficultyRating.Hard;

            if (Precision.AlmostBigger(starRating, 2.0, 0.005))
                return DifficultyRating.Normal;

            return DifficultyRating.Easy;
        }

        /// <summary>
        /// Updates all tracked <see cref="BindableStarDifficulty"/> using the current ruleset and mods.
        /// </summary>
        private void updateTrackedBindables()
        {
            lock (bindableUpdateLock)
            {
                cancelTrackedBindableUpdate();
                trackedUpdateCancellationSource = new CancellationTokenSource();

                foreach (var b in trackedBindables)
                {
                    var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(trackedUpdateCancellationSource.Token, b.CancellationToken);
                    linkedCancellationSources.Add(linkedSource);

                    updateBindable(b, currentRuleset.Value, currentMods.Value, linkedSource.Token);
                }
            }
        }

        /// <summary>
        /// Cancels the existing update of all tracked <see cref="BindableStarDifficulty"/> via <see cref="updateTrackedBindables"/>.
        /// </summary>
        private void cancelTrackedBindableUpdate()
        {
            lock (bindableUpdateLock)
            {
                trackedUpdateCancellationSource?.Cancel();
                trackedUpdateCancellationSource = null;

                if (linkedCancellationSources != null)
                {
                    foreach (var c in linkedCancellationSources)
                        c.Dispose();

                    linkedCancellationSources.Clear();
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="BindableStarDifficulty"/> and triggers an initial value update.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> that star difficulty should correspond to.</param>
        /// <param name="initialRulesetInfo">The initial <see cref="RulesetInfo"/> to get the difficulty with.</param>
        /// <param name="initialMods">The initial <see cref="Mod"/>s to get the difficulty with.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> which stops updating the star difficulty for the given <see cref="BeatmapInfo"/>.</param>
        /// <returns>The <see cref="BindableStarDifficulty"/>.</returns>
        private BindableStarDifficulty createBindable([NotNull] BeatmapInfo beatmapInfo, [CanBeNull] RulesetInfo initialRulesetInfo, [CanBeNull] IEnumerable<Mod> initialMods,
                                                      CancellationToken cancellationToken)
        {
            var bindable = new BindableStarDifficulty(beatmapInfo, cancellationToken);
            updateBindable(bindable, initialRulesetInfo, initialMods, cancellationToken);
            return bindable;
        }

        /// <summary>
        /// Updates the value of a <see cref="BindableStarDifficulty"/> with a given ruleset + mods.
        /// </summary>
        /// <param name="bindable">The <see cref="BindableStarDifficulty"/> to update.</param>
        /// <param name="rulesetInfo">The <see cref="RulesetInfo"/> to update with.</param>
        /// <param name="mods">The <see cref="Mod"/>s to update with.</param>
        /// <param name="cancellationToken">A token that may be used to cancel this update.</param>
        private void updateBindable([NotNull] BindableStarDifficulty bindable, [CanBeNull] RulesetInfo rulesetInfo, [CanBeNull] IEnumerable<Mod> mods, CancellationToken cancellationToken = default)
        {
            // GetDifficultyAsync will fall back to existing data from BeatmapInfo if not locally available
            // (contrary to GetAsync)
            GetDifficultyAsync(bindable.Beatmap, rulesetInfo, mods, cancellationToken)
                .ContinueWith(t =>
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
        /// Computes the difficulty defined by a <see cref="DifficultyCacheLookup"/> key, and stores it to the timed cache.
        /// </summary>
        /// <param name="key">The <see cref="DifficultyCacheLookup"/> that defines the computation parameters.</param>
        /// <returns>The <see cref="StarDifficulty"/>.</returns>
        private StarDifficulty computeDifficulty(in DifficultyCacheLookup key)
        {
            // In the case that the user hasn't given us a ruleset, use the beatmap's default ruleset.
            var beatmapInfo = key.Beatmap;
            var rulesetInfo = key.Ruleset;

            try
            {
                var ruleset = rulesetInfo.CreateInstance();
                Debug.Assert(ruleset != null);

                var calculator = ruleset.CreateDifficultyCalculator(beatmapManager.GetWorkingBeatmap(key.Beatmap));
                var attributes = calculator.Calculate(key.OrderedMods);

                return new StarDifficulty(attributes);
            }
            catch (BeatmapInvalidForRulesetException e)
            {
                // Conversion has failed for the given ruleset, so return the difficulty in the beatmap's default ruleset.

                // Ensure the beatmap's default ruleset isn't the one already being converted to.
                // This shouldn't happen as it means something went seriously wrong, but if it does an endless loop should be avoided.
                if (rulesetInfo.Equals(beatmapInfo.Ruleset))
                {
                    Logger.Error(e, $"Failed to convert {beatmapInfo.OnlineBeatmapID} to the beatmap's default ruleset ({beatmapInfo.Ruleset}).");
                    return new StarDifficulty();
                }

                return GetAsync(new DifficultyCacheLookup(key.Beatmap, key.Beatmap.Ruleset, key.OrderedMods)).Result;
            }
            catch
            {
                return new StarDifficulty();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancelTrackedBindableUpdate();
            updateScheduler?.Dispose();
        }

        public readonly struct DifficultyCacheLookup : IEquatable<DifficultyCacheLookup>
        {
            public readonly BeatmapInfo Beatmap;
            public readonly RulesetInfo Ruleset;

            public readonly Mod[] OrderedMods;

            public DifficultyCacheLookup([NotNull] BeatmapInfo beatmap, [CanBeNull] RulesetInfo ruleset, IEnumerable<Mod> mods)
            {
                Beatmap = beatmap;
                // In the case that the user hasn't given us a ruleset, use the beatmap's default ruleset.
                Ruleset = ruleset ?? Beatmap.Ruleset;
                OrderedMods = mods?.OrderBy(m => m.Acronym).ToArray() ?? Array.Empty<Mod>();
            }

            public bool Equals(DifficultyCacheLookup other)
                => Beatmap.ID == other.Beatmap.ID
                   && Ruleset.ID == other.Ruleset.ID
                   && OrderedMods.Select(m => m.Acronym).SequenceEqual(other.OrderedMods.Select(m => m.Acronym));

            public override int GetHashCode()
            {
                var hashCode = new HashCode();

                hashCode.Add(Beatmap.ID);
                hashCode.Add(Ruleset.ID);

                foreach (var mod in OrderedMods)
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
}
