// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.Primitives;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Objects.Drawables
{
    public abstract class DrawableHitObject : SkinReloadableDrawable, IHasAccentColour
    {
        public readonly HitObject HitObject;

        /// <summary>
        /// The colour used for various elements of this DrawableHitObject.
        /// </summary>
        public virtual Color4 AccentColour { get; set; } = Color4.Gray;

        // Todo: Rulesets should be overriding the resources instead, but we need to figure out where/when to apply overrides first
        protected virtual string SampleNamespace => null;

        protected SkinnableSound Samples;

        protected virtual IEnumerable<SampleInfo> GetSamples() => HitObject.Samples;

        private readonly Lazy<List<DrawableHitObject>> nestedHitObjects = new Lazy<List<DrawableHitObject>>();
        public IEnumerable<DrawableHitObject> NestedHitObjects => nestedHitObjects.IsValueCreated ? nestedHitObjects.Value : Enumerable.Empty<DrawableHitObject>();

        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> has been applied by this <see cref="DrawableHitObject"/> or a nested <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<DrawableHitObject, JudgementResult> OnNewResult;

        /// <summary>
        /// Invoked when a <see cref="JudgementResult"/> is being reverted by this <see cref="DrawableHitObject"/> or a nested <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<DrawableHitObject, JudgementResult> OnRevertResult;

        /// <summary>
        /// Whether a visual indicator should be displayed when a scoring result occurs.
        /// </summary>
        public virtual bool DisplayResult => true;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> and all of its nested <see cref="DrawableHitObject"/>s have been judged.
        /// </summary>
        public bool AllJudged => Judged && NestedHitObjects.All(h => h.AllJudged);

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> has been hit. This occurs if <see cref="Result.IsHit"/> is <see cref="true"/>.
        /// Note: This does NOT include nested hitobjects.
        /// </summary>
        public bool IsHit => Result?.IsHit ?? false;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> has been judged.
        /// Note: This does NOT include nested hitobjects.
        /// </summary>
        public bool Judged => Result?.HasResult ?? true;

        /// <summary>
        /// The scoring result of this <see cref="DrawableHitObject"/>.
        /// </summary>
        public JudgementResult Result { get; private set; }

        private bool judgementOccurred;

        public bool Interactive = true;
        public override bool HandleNonPositionalInput => Interactive;
        public override bool HandlePositionalInput => Interactive;

        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;
        protected override bool RequiresChildrenUpdate => true;

        public override bool IsPresent => base.IsPresent || State.Value == ArmedState.Idle && Clock?.CurrentTime >= LifetimeStart;

        public readonly Bindable<ArmedState> State = new Bindable<ArmedState>();

        protected DrawableHitObject(HitObject hitObject)
        {
            HitObject = hitObject;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var judgement = HitObject.CreateJudgement();
            if (judgement != null)
            {
                Result = CreateResult(judgement);
                if (Result == null)
                    throw new InvalidOperationException($"{GetType().ReadableName()} must provide a {nameof(JudgementResult)} through {nameof(CreateResult)}.");
            }

            var samples = GetSamples().ToArray();

            if (samples.Any())
            {
                if (HitObject.SampleControlPoint == null)
                    throw new ArgumentNullException(nameof(HitObject.SampleControlPoint), $"{nameof(HitObject)}s must always have an attached {nameof(HitObject.SampleControlPoint)}."
                                                                                          + $" This is an indication that {nameof(HitObject.ApplyDefaults)} has not been invoked on {this}.");

                samples = samples.Select(s => HitObject.SampleControlPoint.ApplyTo(s)).ToArray();
                foreach (var s in samples)
                    s.Namespace = SampleNamespace;

                AddInternal(Samples = new SkinnableSound(samples));
            }
        }

        protected override void ClearInternal(bool disposeChildren = true) => throw new InvalidOperationException($"Should never clear a {nameof(DrawableHitObject)}");

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.ValueChanged += armed =>
            {
                UpdateState(armed.NewValue);

                // apply any custom state overrides
                ApplyCustomUpdateState?.Invoke(this, armed.NewValue);

                if (armed.NewValue == ArmedState.Hit)
                    PlaySamples();
            };

            State.TriggerChange();
        }

        protected abstract void UpdateState(ArmedState state);

        /// <summary>
        /// Bind to apply a custom state which can override the default implementation.
        /// </summary>
        public event Action<DrawableHitObject, ArmedState> ApplyCustomUpdateState;

        /// <summary>
        /// Plays all the hit sounds for this <see cref="DrawableHitObject"/>.
        /// This is invoked automatically when this <see cref="DrawableHitObject"/> is hit.
        /// </summary>
        public void PlaySamples() => Samples?.Play();

        protected override void Update()
        {
            base.Update();

            if (Result != null && Result.HasResult)
            {
                var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;

                if (Result.TimeOffset + endTime > Time.Current)
                {
                    OnRevertResult?.Invoke(this, Result);

                    Result.Type = HitResult.None;
                    State.Value = ArmedState.Idle;
                }
            }
        }

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => AllJudged && base.ComputeIsMaskedAway(maskingBounds);

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            UpdateResult(false);
        }

        protected virtual void AddNested(DrawableHitObject h)
        {
            h.OnNewResult += (d, r) => OnNewResult?.Invoke(d, r);
            h.OnRevertResult += (d, r) => OnRevertResult?.Invoke(d, r);
            h.ApplyCustomUpdateState += (d, j) => ApplyCustomUpdateState?.Invoke(d, j);

            nestedHitObjects.Value.Add(h);
        }

        /// <summary>
        /// Applies the <see cref="Result"/> of this <see cref="DrawableHitObject"/>, notifying responders such as
        /// the <see cref="ScoreProcessor"/> of the <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="application">The callback that applies changes to the <see cref="JudgementResult"/>.</param>
        protected void ApplyResult(Action<JudgementResult> application)
        {
            application?.Invoke(Result);

            if (!Result.HasResult)
                throw new InvalidOperationException($"{GetType().ReadableName()} applied a {nameof(JudgementResult)} but did not update {nameof(JudgementResult.Type)}.");

            judgementOccurred = true;

            // Ensure that the judgement is given a valid time offset, because this may not get set by the caller
            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
            Result.TimeOffset = Time.Current - endTime;

            switch (Result.Type)
            {
                case HitResult.None:
                    break;
                case HitResult.Miss:
                    State.Value = ArmedState.Miss;
                    break;
                default:
                    State.Value = ArmedState.Hit;
                    break;
            }

            OnNewResult?.Invoke(this, Result);
        }

        /// <summary>
        /// Will called at least once after the <see cref="LifetimeEnd"/> of this <see cref="DrawableHitObject"/> has been passed.
        /// </summary>
        internal void OnLifetimeEnd()
        {
            foreach (var nested in NestedHitObjects)
                nested.OnLifetimeEnd();
            UpdateResult(false);
        }

        /// <summary>
        /// Processes this <see cref="DrawableHitObject"/>, checking if a scoring result has occurred.
        /// </summary>
        /// <param name="userTriggered">Whether the user triggered this process.</param>
        /// <returns>Whether a scoring result has occurred from this <see cref="DrawableHitObject"/> or any nested <see cref="DrawableHitObject"/>.</returns>
        protected bool UpdateResult(bool userTriggered)
        {
            judgementOccurred = false;

            if (AllJudged)
                return false;

            foreach (var d in NestedHitObjects)
                judgementOccurred |= d.UpdateResult(userTriggered);

            if (judgementOccurred || Judged)
                return judgementOccurred;

            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
            CheckForResult(userTriggered, Time.Current - endTime);

            return judgementOccurred;
        }

        /// <summary>
        /// Checks if a scoring result has occurred for this <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// If a scoring result has occurred, this method must invoke <see cref="ApplyResult"/> to update the result and notify responders.
        /// </remarks>
        /// <param name="userTriggered">Whether the user triggered this check.</param>
        /// <param name="timeOffset">The offset from the end time of the <see cref="HitObject"/> at which this check occurred.
        /// A <paramref name="timeOffset"/> &gt; 0 implies that this check occurred after the end time of the <see cref="HitObject"/>. </param>
        protected virtual void CheckForResult(bool userTriggered, double timeOffset)
        {
        }

        /// <summary>
        /// Creates the <see cref="JudgementResult"/> that represents the scoring result for this <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="judgement">The <see cref="Judgement"/> that provides the scoring information.</param>
        protected virtual JudgementResult CreateResult(Judgement judgement) => new JudgementResult(judgement);
    }

    public abstract class DrawableHitObject<TObject> : DrawableHitObject
        where TObject : HitObject
    {
        public new readonly TObject HitObject;

        protected DrawableHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;
        }
    }
}
