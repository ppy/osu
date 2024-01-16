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
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Scoring
{
    public abstract partial class JudgementProcessor : Component
    {
        /// <summary>
        /// Invoked when a new judgement has occurred. This occurs after the judgement has been processed by this <see cref="JudgementProcessor"/>.
        /// </summary>
        public event Action<Judgement>? NewJudgement;

        /// <summary>
        /// Invoked when a judgement is reverted, usually due to rewinding gameplay.
        /// </summary>
        public event Action<Judgement>? JudgementReverted;

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

        private Judgement? lastAppliedResult;

        private readonly BindableBool hasCompleted = new BindableBool();

        /// <summary>
        /// Whether all <see cref="JudgementInfo"/>s have been processed.
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
        /// Applies the score change of a <see cref="Judgement"/> to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="result">The <see cref="Judgement"/> to apply.</param>
        public void ApplyResult(Judgement result)
        {
#pragma warning disable CS0618
            if (result.Type == HitResult.LegacyComboIncrease)
                throw new ArgumentException(@$"A {nameof(HitResult.LegacyComboIncrease)} hit result cannot be applied.");
#pragma warning restore CS0618

            JudgedHits++;
            lastAppliedResult = result;

            ApplyResultInternal(result);

            NewJudgement?.Invoke(result);
        }

        /// <summary>
        /// Reverts the score change of a <see cref="Judgement"/> that was applied to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <param name="result">The judgement scoring result.</param>
        public void RevertResult(Judgement result)
        {
            JudgedHits--;

            RevertResultInternal(result);

            JudgementReverted?.Invoke(result);
        }

        /// <summary>
        /// Applies the score change of a <see cref="Judgement"/> to this <see cref="ScoreProcessor"/>.
        /// </summary>
        /// <remarks>
        /// Any changes applied via this method can be reverted via <see cref="RevertResultInternal"/>.
        /// </remarks>
        /// <param name="result">The <see cref="Judgement"/> to apply.</param>
        protected abstract void ApplyResultInternal(Judgement result);

        /// <summary>
        /// Reverts the score change of a <see cref="Judgement"/> that was applied to this <see cref="ScoreProcessor"/> via <see cref="ApplyResultInternal"/>.
        /// </summary>
        /// <param name="result">The judgement scoring result.</param>
        protected abstract void RevertResultInternal(Judgement result);

        /// <summary>
        /// Resets this <see cref="JudgementProcessor"/> to a default state.
        /// </summary>
        /// <param name="storeResults">Whether to store the current state of the <see cref="JudgementProcessor"/> for future use.</param>
        protected virtual void Reset(bool storeResults)
        {
            if (storeResults)
                MaxHits = JudgedHits;

            JudgedHits = 0;
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
                var judgement = obj.CreateJudgement();

                var result = CreateResult(obj, judgement);
                if (result == null)
                    throw new InvalidOperationException($"{GetType().ReadableName()} must provide a {nameof(Judgement)} through {nameof(CreateResult)}.");

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
        /// Creates the <see cref="Judgement"/> that represents the scoring result for a <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which was judged.</param>
        /// <param name="judgementInfo">The <see cref="JudgementInfo"/> that provides the scoring information.</param>
        protected virtual Judgement CreateResult(HitObject hitObject, JudgementInfo judgementInfo) => new Judgement(hitObject, judgementInfo);

        /// <summary>
        /// Gets a simulated <see cref="HitResult"/> for a judgement. Used during <see cref="SimulateAutoplay"/> to simulate a "perfect" play.
        /// </summary>
        /// <param name="judgementInfo">The judgement to simulate a <see cref="HitResult"/> for.</param>
        /// <returns>The simulated <see cref="HitResult"/> for the judgement.</returns>
        protected virtual HitResult GetSimulatedHitResult(JudgementInfo judgementInfo) => judgementInfo.MaxResult;

        protected override void Update()
        {
            base.Update();

            hasCompleted.Value =
                JudgedHits == MaxHits
                && (JudgedHits == 0
                    // Last applied result is guaranteed to be non-null when JudgedHits > 0.
                    || lastAppliedResult.AsNonNull().TimeAbsolute < Clock.CurrentTime);
        }
    }
}
