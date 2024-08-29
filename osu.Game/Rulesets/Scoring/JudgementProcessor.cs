// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Scoring
{
    public abstract partial class JudgementProcessor : Component
    {
        /// <summary>
        /// Invoked when a new judgement has occurred. This occurs after the judgement has been processed by this <see cref="JudgementProcessor"/>.
        /// </summary>
        public event Action<JudgementResult>? NewJudgement;

        /// <summary>
        /// Invoked when a judgement is reverted, usually due to rewinding gameplay.
        /// </summary>
        public event Action<JudgementResult>? JudgementReverted;

        /// <summary>
        /// Invoked when this <see cref="JudgementProcessor"/> is in a failed state.
        /// </summary>
        public event FailedDelegate? Failed;

        /// <summary>
        /// The current selected mods.
        /// </summary>
        public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// The maximum number of hits that can be judged.
        /// </summary>
        protected int MaxHits { get; private set; }

        /// <summary>
        /// Whether <see cref="SimulateAutoplay"/> is currently running.
        /// </summary>
        protected bool IsSimulating { get; private set; }

        /// <summary>
        /// The total number of judged <see cref="HitObject"/>s at the current point in time.
        /// </summary>
        public int JudgedHits { get; private set; }

        /// <summary>
        /// Whether this <see cref="JudgementProcessor"/> has already triggered the failed state.
        /// </summary>
        public bool HasFailed { get; private set; }

        private JudgementResult? lastAppliedResult;

        private readonly BindableBool hasCompleted = new BindableBool();

        /// <summary>
        /// Whether all <see cref="Judgement"/>s have been processed.
        /// </summary>
        public IBindable<bool> HasCompleted => hasCompleted;

        /// <summary>
        /// Applies a <see cref="IBeatmap"/> to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to read properties from.</param>
        public virtual void ApplyBeatmap(IBeatmap beatmap)
        {
            Reset(false);
            SimulateAutoplay(beatmap);
            Reset(true);
        }

        /// <summary>
        /// Applies the score change of a <see cref="JudgementResult"/> to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to apply.</param>
        public void ApplyResult(JudgementResult result)
        {
#pragma warning disable CS0618
            if (result.Type == HitResult.LegacyComboIncrease)
                throw new ArgumentException(@$"A {nameof(HitResult.LegacyComboIncrease)} hit result cannot be applied.");
#pragma warning restore CS0618

            result.FailedAtJudgement |= HasFailed;

            JudgedHits++;
            lastAppliedResult = result;

            ApplyResultInternal(result);
            checkFailConditions(result);

            NewJudgement?.Invoke(result);
        }

        /// <summary>
        /// Reverts the score change of a <see cref="JudgementResult"/> that was applied to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="result">The judgement scoring result.</param>
        public void RevertResult(JudgementResult result)
        {
            JudgedHits--;
            RevertResultInternal(result);

            JudgementReverted?.Invoke(result);
        }

        /// <summary>
        /// Applies the score change of a <see cref="JudgementResult"/> to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <remarks>
        /// Any changes applied via this method can be reverted via <see cref="RevertResultInternal"/>.
        /// </remarks>
        /// <param name="result">The <see cref="JudgementResult"/> to apply.</param>
        protected abstract void ApplyResultInternal(JudgementResult result);

        /// <summary>
        /// Reverts the score change of a <see cref="JudgementResult"/> that was applied to this <see cref="ScoreProcessor"/> via <see cref="ApplyResultInternal"/>.
        /// </summary>
        /// <param name="result">The judgement scoring result.</param>
        protected abstract void RevertResultInternal(JudgementResult result);

        private void checkFailConditions(JudgementResult result)
        {
            if (HasFailed)
                return;

            if (!meetsAnyFailCondition(result, out bool restart))
                return;

            if (Failed?.Invoke(restart) != false)
                HasFailed = true;
        }

        /// <summary>
        /// Whether the current state of <see cref="HealthProcessor"/> or the provided <paramref name="result"/> meets any fail condition.
        /// </summary>
        /// <param name="result">The judgement result.</param>
        /// <param name="restart">Whether a restart should be triggered as a result of the fail.</param>
        private bool meetsAnyFailCondition(JudgementResult result, out bool restart)
        {
            bool hasDefaultFail = CheckDefaultFailCondition(result);
            bool allowDefaultFail = false;

            bool hasForcedFail = false;
            bool forcedRestartOnFail = false;

            for (int i = 0; i < Mods.Value.Count; i++)
            {
                switch (Mods.Value[i])
                {
                    case IBlockFail blockFailMod:
                        // This is intentionally not de-duping so that all mods have a chance to update internal states (e.g. ModEasyWithExtraLives).
                        if (hasDefaultFail)
                            allowDefaultFail |= blockFailMod.AllowFail();
                        break;

                    case IForceFail failMod:
                        if (failMod.ShouldFail(result))
                        {
                            hasForcedFail = true;
                            forcedRestartOnFail = failMod.RestartOnFail;
                        }

                        break;
                }
            }

            restart = forcedRestartOnFail;
            return hasForcedFail || (hasDefaultFail && allowDefaultFail);
        }

        /// <summary>
        /// Resets this <see cref="JudgementProcessor"/> to a default state.
        /// </summary>
        /// <param name="storeResults">Whether to store the current state of the <see cref="JudgementProcessor"/> for future use.</param>
        protected virtual void Reset(bool storeResults)
        {
            if (storeResults)
                MaxHits = JudgedHits;

            JudgedHits = 0;
            HasFailed = false;
        }

        /// <summary>
        /// Reset all statistics based on header information contained within a replay frame.
        /// </summary>
        /// <remarks>
        /// If the provided replay frame does not have any header information, this will be a noop.
        /// </remarks>
        /// <param name="frame">The replay frame to read header statistics from.</param>
        public virtual void ResetFromReplayFrame(ReplayFrame frame)
        {
            if (frame.Header == null)
                return;

            JudgedHits = 0;

            foreach ((_, int count) in frame.Header.Statistics)
                JudgedHits += count;
        }

        /// <summary>
        /// Simulates an autoplay of the <see cref="IBeatmap"/> to determine scoring values.
        /// </summary>
        /// <remarks>This provided temporarily. DO NOT USE.</remarks>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to simulate.</param>
        protected void SimulateAutoplay(IBeatmap beatmap)
        {
            IsSimulating = true;

            foreach (var obj in EnumerateHitObjects(beatmap))
            {
                var judgement = obj.Judgement;

                var result = CreateResult(obj, judgement);
                if (result == null)
                    throw new InvalidOperationException($"{GetType().ReadableName()} must provide a {nameof(JudgementResult)} through {nameof(CreateResult)}.");

                result.Type = GetSimulatedHitResult(judgement);
                ApplyResult(result);
            }

            IsSimulating = false;
        }

        /// <summary>
        /// Enumerates all <see cref="HitObject"/>s in the given <paramref name="beatmap"/> in the order in which they are to be judged.
        /// Used in <see cref="SimulateAutoplay"/>.
        /// </summary>
        /// <remarks>
        /// In Score V2, the score awarded for each object includes a component based on the combo value after the judgement of that object.
        /// This means that the score is dependent on the order of evaluation of judgements.
        /// This method is provided so that rulesets can specify custom ordering that is correct for them and matches processing order during actual gameplay.
        /// </remarks>
        protected virtual IEnumerable<HitObject> EnumerateHitObjects(IBeatmap beatmap)
            => enumerateRecursively(beatmap.HitObjects);

        private IEnumerable<HitObject> enumerateRecursively(IEnumerable<HitObject> hitObjects)
        {
            foreach (var hitObject in hitObjects)
            {
                foreach (var nested in enumerateRecursively(hitObject.NestedHitObjects))
                    yield return nested;

                yield return hitObject;
            }
        }

        /// <summary>
        /// Creates the <see cref="JudgementResult"/> that represents the scoring result for a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which was judged.</param>
        /// <param name="judgement">The <see cref="Judgement"/> that provides the scoring information.</param>
        protected virtual JudgementResult CreateResult(HitObject hitObject, Judgement judgement) => new JudgementResult(hitObject, judgement);

        /// <summary>
        /// Gets a simulated <see cref="HitResult"/> for a judgement. Used during <see cref="SimulateAutoplay"/> to simulate a "perfect" play.
        /// </summary>
        /// <param name="judgement">The judgement to simulate a <see cref="HitResult"/> for.</param>
        /// <returns>The simulated <see cref="HitResult"/> for the judgement.</returns>
        protected virtual HitResult GetSimulatedHitResult(Judgement judgement) => judgement.MaxResult;

        protected override void Update()
        {
            base.Update();

            hasCompleted.Value =
                JudgedHits == MaxHits
                && (JudgedHits == 0
                    // Last applied result is guaranteed to be non-null when JudgedHits > 0.
                    || lastAppliedResult.AsNonNull().TimeAbsolute < Clock.CurrentTime);
        }

        /// <summary>
        /// Checks whether the default conditions for failing are met.
        /// </summary>
        /// <returns><see langword="true"/> if failure should be invoked.</returns>
        protected virtual bool CheckDefaultFailCondition(JudgementResult result) => false;
    }

    /// <summary>
    /// Handles a failing state.
    /// </summary>
    /// <param name="restart">Whether gameplay is expected to be restarted as a result of the fail.</param>
    /// <returns><c>true</c> if the fail was allowed to occur.</returns>
    public delegate bool FailedDelegate(bool restart);
}
