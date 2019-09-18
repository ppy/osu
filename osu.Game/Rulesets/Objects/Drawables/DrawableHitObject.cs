﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Objects.Drawables
{
    [Cached(typeof(DrawableHitObject))]
    public abstract class DrawableHitObject : SkinReloadableDrawable
    {
        public readonly HitObject HitObject;

        /// <summary>
        /// The colour used for various elements of this DrawableHitObject.
        /// </summary>
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>(Color4.Gray);

        // Todo: Rulesets should be overriding the resources instead, but we need to figure out where/when to apply overrides first
        protected virtual string SampleNamespace => null;

        protected SkinnableSound Samples;

        protected virtual IEnumerable<HitSampleInfo> GetSamples() => HitObject.Samples;

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
        /// Whether this <see cref="DrawableHitObject"/> has been hit. This occurs if <see cref="Result"/> is hit.
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

        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;
        protected override bool RequiresChildrenUpdate => true;

        public override bool IsPresent => base.IsPresent || (State.Value == ArmedState.Idle && Clock?.CurrentTime >= LifetimeStart);

        private readonly Bindable<ArmedState> state = new Bindable<ArmedState>();

        public IBindable<ArmedState> State => state;

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

            if (samples.Length > 0)
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState(ArmedState.Idle, true);
        }

        #region State / Transform Management

        /// <summary>
        /// Bind to apply a custom state which can override the default implementation.
        /// </summary>
        public event Action<DrawableHitObject, ArmedState> ApplyCustomUpdateState;

#pragma warning disable 618 // (legacy state management) - can be removed 20200227

        /// <summary>
        /// Enables automatic transform management of this hitobject. Implementation of transforms should be done in <see cref="UpdateInitialTransforms"/> and <see cref="UpdateStateTransforms"/> only. Rewinding and removing previous states is done automatically.
        /// </summary>
        /// <remarks>
        /// Going forward, this is the preferred way of implementing <see cref="DrawableHitObject"/>s. Previous functionality
        /// is offered as a compatibility layer until all rulesets have been migrated across.
        /// </remarks>
        [Obsolete("Use UpdateInitialTransforms()/UpdateStateTransforms() instead")] // can be removed 20200227
        protected virtual bool UseTransformStateManagement => true;

        protected override void ClearInternal(bool disposeChildren = true) => throw new InvalidOperationException($"Should never clear a {nameof(DrawableHitObject)}");

        private void updateState(ArmedState newState, bool force = false)
        {
            if (State.Value == newState && !force)
                return;

            if (UseTransformStateManagement)
            {
                LifetimeEnd = double.MaxValue;

                double transformTime = HitObject.StartTime - InitialLifetimeOffset;

                base.ApplyTransformsAt(transformTime, true);
                base.ClearTransformsAfter(transformTime, true);

                using (BeginAbsoluteSequence(transformTime, true))
                {
                    UpdateInitialTransforms();

                    var judgementOffset = Math.Min(HitObject.HitWindows?.WindowFor(HitResult.Miss) ?? double.MaxValue, Result?.TimeOffset ?? 0);

                    using (BeginDelayedSequence(InitialLifetimeOffset + judgementOffset, true))
                    {
                        UpdateStateTransforms(newState);
                        state.Value = newState;
                    }
                }

                if (state.Value != ArmedState.Idle && LifetimeEnd == double.MaxValue)
                    Expire();
            }
            else
                state.Value = newState;

            UpdateState(newState);

            // apply any custom state overrides
            ApplyCustomUpdateState?.Invoke(this, newState);

            if (newState == ArmedState.Hit)
                PlaySamples();
        }

        /// <summary>
        /// Apply (generally fade-in) transforms leading into the <see cref="HitObject"/> start time.
        /// The local drawable hierarchy is recursively delayed to <see cref="LifetimeStart"/> for convenience.
        ///
        /// By default this will fade in the object from zero with no duration.
        /// </summary>
        /// <remarks>
        /// This is called once before every <see cref="UpdateStateTransforms"/>. This is to ensure a good state in the case
        /// the <see cref="JudgementResult.TimeOffset"/> was negative and potentially altered the pre-hit transforms.
        /// </remarks>
        protected virtual void UpdateInitialTransforms()
        {
            this.FadeInFromZero();
        }

        /// <summary>
        /// Apply transforms based on the current <see cref="ArmedState"/>. Previous states are automatically cleared.
        /// In the case of a non-idle <see cref="ArmedState"/>, and if <see cref="Drawable.LifetimeEnd"/> was not set during this call, <see cref="Drawable.Expire"/> will be invoked.
        /// </summary>
        /// <param name="state">The new armed state.</param>
        protected virtual void UpdateStateTransforms(ArmedState state)
        {
        }

        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null)
        {
            // When we are using automatic state management, parent calls to this should be blocked for safety.
            if (!UseTransformStateManagement)
                base.ClearTransformsAfter(time, propagateChildren, targetMember);
        }

        public override void ApplyTransformsAt(double time, bool propagateChildren = false)
        {
            // When we are using automatic state management, parent calls to this should be blocked for safety.
            if (!UseTransformStateManagement)
                base.ApplyTransformsAt(time, propagateChildren);
        }

        /// <summary>
        /// Legacy method to handle state changes.
        /// Should generally not be used when <see cref="UseTransformStateManagement"/> is true; use <see cref="UpdateStateTransforms"/> instead.
        /// </summary>
        /// <param name="state">The new armed state.</param>
        [Obsolete("Use UpdateInitialTransforms()/UpdateStateTransforms() instead")] // can be removed 20200227
        protected virtual void UpdateState(ArmedState state)
        {
        }

#pragma warning restore 618

        #endregion

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            if (HitObject is IHasComboInformation combo)
            {
                var comboColours = skin.GetConfig<GlobalSkinConfiguration, List<Color4>>(GlobalSkinConfiguration.ComboColours)?.Value;

                AccentColour.Value = comboColours?.Count > 0 ? comboColours[combo.ComboIndex % comboColours.Count] : Color4.White;
            }
        }

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

                    Result.TimeOffset = 0;
                    Result.Type = HitResult.None;

                    updateState(ArmedState.Idle);
                }
            }
        }

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => AllJudged && base.ComputeIsMaskedAway(maskingBounds);

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            UpdateResult(false);
        }

        /// <summary>
        /// Schedules an <see cref="Action"/> to this <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// Only provided temporarily until hitobject pooling is implemented.
        /// </remarks>
        protected internal new ScheduledDelegate Schedule(Action action) => base.Schedule(action);

        private double? lifetimeStart;

        public override double LifetimeStart
        {
            get => lifetimeStart ?? (HitObject.StartTime - InitialLifetimeOffset);
            set
            {
                base.LifetimeStart = value;
                lifetimeStart = value;
            }
        }

        /// <summary>
        /// A safe offset prior to the start time of <see cref="HitObject"/> at which this <see cref="DrawableHitObject"/> may begin displaying contents.
        /// By default, <see cref="DrawableHitObject"/>s are assumed to display their contents within 10 seconds prior to the start time of <see cref="HitObject"/>.
        /// </summary>
        /// <remarks>
        /// This is only used as an optimisation to delay the initial update of this <see cref="DrawableHitObject"/> and may be tuned more aggressively if required.
        /// It is indirectly used to decide the automatic transform offset provided to <see cref="UpdateInitialTransforms"/>.
        /// A more accurate <see cref="LifetimeStart"/> should be set for further optimisation (in <see cref="LoadComplete"/>, for example).
        /// </remarks>
        protected virtual double InitialLifetimeOffset => 10000;

        /// <summary>
        /// Will be called at least once after this <see cref="DrawableHitObject"/> has become not alive.
        /// </summary>
        public virtual void OnKilled()
        {
            foreach (var nested in NestedHitObjects)
                nested.OnKilled();

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

            // Ensure that the judgement is given a valid time offset, because this may not get set by the caller
            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
            Result.TimeOffset = Time.Current - endTime;

            switch (Result.Type)
            {
                case HitResult.None:
                    break;

                case HitResult.Miss:
                    updateState(ArmedState.Miss);
                    break;

                default:
                    updateState(ArmedState.Hit);
                    break;
            }

            OnNewResult?.Invoke(this, Result);
        }

        /// <summary>
        /// Processes this <see cref="DrawableHitObject"/>, checking if a scoring result has occurred.
        /// </summary>
        /// <param name="userTriggered">Whether the user triggered this process.</param>
        /// <returns>Whether a scoring result has occurred from this <see cref="DrawableHitObject"/> or any nested <see cref="DrawableHitObject"/>.</returns>
        protected bool UpdateResult(bool userTriggered)
        {
            // It's possible for input to get into a bad state when rewinding gameplay, so results should not be processed
            if (Time.Elapsed < 0)
                return false;

            if (Judged)
                return false;

            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
            CheckForResult(userTriggered, Time.Current - endTime);

            return Judged;
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
        protected virtual JudgementResult CreateResult(Judgement judgement) => new JudgementResult(HitObject, judgement);
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
