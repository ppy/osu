// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Objects.Drawables
{
    [Cached(typeof(DrawableHitObject))]
    public abstract partial class DrawableHitObject : PoolableDrawableWithLifetime<HitObjectLifetimeEntry>, IAnimationTimeReference
    {
        /// <summary>
        /// Invoked after this <see cref="DrawableHitObject"/>'s applied <see cref="HitObject"/> has had its defaults applied.
        /// </summary>
        public event Action<DrawableHitObject> DefaultsApplied;

        /// <summary>
        /// Invoked after a <see cref="HitObject"/> has been applied to this <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<DrawableHitObject> HitObjectApplied;

        /// <summary>
        /// The <see cref="HitObject"/> currently represented by this <see cref="DrawableHitObject"/>.
        /// </summary>
        public HitObject HitObject => Entry?.HitObject;

        /// <summary>
        /// The parenting <see cref="DrawableHitObject"/>, if any.
        /// </summary>
        [CanBeNull]
        protected internal DrawableHitObject ParentHitObject { get; internal set; }

        /// <summary>
        /// The colour used for various elements of this DrawableHitObject.
        /// </summary>
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>(Color4.Gray);

        protected PausableSkinnableSound Samples { get; private set; }

        public virtual IEnumerable<HitSampleInfo> GetSamples() => HitObject.Samples;

        private readonly List<DrawableHitObject> nestedHitObjects = new List<DrawableHitObject>();
        public IReadOnlyList<DrawableHitObject> NestedHitObjects => nestedHitObjects;

        /// <summary>
        /// Whether this object should handle any user input events.
        /// </summary>
        public bool HandleUserInput { get; set; } = true;

        public override bool PropagatePositionalInputSubTree => HandleUserInput;

        public override bool PropagateNonPositionalInputSubTree => HandleUserInput;

        /// <summary>
        /// Invoked by this or a nested <see cref="DrawableHitObject"/> after a <see cref="JudgementResult"/> has been applied.
        /// </summary>
        public event Action<DrawableHitObject, JudgementResult> OnNewResult;

        /// <summary>
        /// Invoked by this or a nested <see cref="DrawableHitObject"/> prior to a <see cref="JudgementResult"/> being reverted.
        /// </summary>
        /// <remarks>
        /// This is only invoked if this <see cref="DrawableHitObject"/> is alive when the result is reverted.
        /// </remarks>
        public event Action<DrawableHitObject, JudgementResult> OnRevertResult;

        /// <summary>
        /// Invoked when a new nested hit object is created by <see cref="CreateNestedHitObject" />.
        /// </summary>
        internal event Action<DrawableHitObject> OnNestedDrawableCreated;

        /// <summary>
        /// Whether a visual indicator should be displayed when a scoring result occurs.
        /// </summary>
        public virtual bool DisplayResult => true;

        /// <summary>
        /// The scoring result of this <see cref="DrawableHitObject"/>.
        /// </summary>
        public JudgementResult Result => Entry?.Result;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> has been hit. This occurs if <see cref="Result"/> is hit.
        /// Note: This does NOT include nested hitobjects.
        /// </summary>
        public bool IsHit => Result?.IsHit ?? false;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> has been judged.
        /// Note: This does NOT include nested hitobjects.
        /// </summary>
        public bool Judged => Entry?.Judged ?? false;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> and all of its nested <see cref="DrawableHitObject"/>s have been judged.
        /// </summary>
        public bool AllJudged => Entry?.AllJudged ?? false;

        /// <summary>
        /// The relative X position of this hit object for sample playback balance adjustment.
        /// </summary>
        /// <remarks>
        /// This is a range of 0..1 (0 for far-left, 0.5 for centre, 1 for far-right).
        /// Dampening is post-applied to ensure the effect is not too intense.
        /// </remarks>
        protected virtual float SamplePlaybackPosition => 0.5f;

        public readonly Bindable<double> StartTimeBindable = new Bindable<double>();
        private readonly BindableList<HitSampleInfo> samplesBindable = new BindableList<HitSampleInfo>();
        private readonly Bindable<int> comboIndexBindable = new Bindable<int>();

        private readonly IBindable<float> positionalHitsoundsLevel = new Bindable<float>();
        private readonly IBindable<float> comboColourBrightness = new Bindable<float>();
        private readonly Bindable<int> comboIndexWithOffsetsBindable = new Bindable<int>();

        protected override bool RequiresChildrenUpdate => true;

        public override bool IsPresent => base.IsPresent || (State.Value == ArmedState.Idle && Clock?.CurrentTime >= LifetimeStart);

        private readonly Bindable<ArmedState> state = new Bindable<ArmedState>();

        /// <summary>
        /// The state of this <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// For pooled hitobjects, <see cref="ApplyCustomUpdateState"/> is recommended to be used instead for better editor/rewinding support.
        /// </remarks>
        public IBindable<ArmedState> State => state;

        [Resolved(CanBeNull = true)]
        private IPooledHitObjectProvider pooledObjectProvider { get; set; }

        /// <summary>
        /// Whether the initialization logic in <see cref="Playfield" /> has applied.
        /// </summary>
        internal bool IsInitialized;

        /// <summary>
        /// The minimum allowable volume for sample playback.
        /// <see cref="Samples"/> quieter than that will be forcibly played at this volume instead.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Drawable hitobjects adding their own custom samples, or other sample playback sources
        /// (i.e. <see cref="GameplaySampleTriggerSource"/>) must enforce this themselves.
        /// </para>
        /// <para>
        /// This sample volume floor is present in stable, although it is set at 8% rather than 5%.
        /// See: https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/Audio/AudioEngine.cs#L1070,
        /// https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/Audio/AudioEngine.cs#L1404-L1405.
        /// The reason why it is 5% here is that the 8% cap was enforced in a silent manner
        /// (i.e. the minimum selectable volume in the editor was 5%, but it would be played at 8% anyways),
        /// which is confusing and arbitrary, so we're just doing 5% here at the cost of sacrificing strict parity.
        /// </para>
        /// </remarks>
        public const int MINIMUM_SAMPLE_VOLUME = 5;

        /// <summary>
        /// Creates a new <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="initialHitObject">
        /// The <see cref="HitObject"/> to be initially applied to this <see cref="DrawableHitObject"/>.
        /// If <c>null</c>, a hitobject is expected to be later applied via <see cref="PoolableDrawableWithLifetime{TEntry}.Apply"/> (or automatically via pooling).
        /// </param>
        protected DrawableHitObject([CanBeNull] HitObject initialHitObject = null)
        {
            if (initialHitObject == null) return;

            Entry = new SyntheticHitObjectEntry(initialHitObject);
            ensureEntryHasResult();
        }

        [BackgroundDependencyLoader]
        private void load(IGameplaySettings gameplaySettings, ISkinSource skinSource)
        {
            positionalHitsoundsLevel.BindTo(gameplaySettings.PositionalHitsoundsLevel);
            comboColourBrightness.BindTo(gameplaySettings.ComboColourNormalisationAmount);

            // Explicit non-virtual function call in case a DrawableHitObject overrides AddInternal.
            base.AddInternal(Samples = new PausableSkinnableSound
            {
                MinimumSampleVolume = MINIMUM_SAMPLE_VOLUME
            });

            CurrentSkin = skinSource;
            CurrentSkin.SourceChanged += skinSourceChanged;
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            skinChanged();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            comboIndexBindable.BindValueChanged(_ => UpdateComboColour());
            comboIndexWithOffsetsBindable.BindValueChanged(_ => UpdateComboColour(), true);

            comboColourBrightness.BindValueChanged(_ => UpdateComboColour());

            // Apply transforms
            updateStateFromResult();
        }

        /// <summary>
        /// Applies a new <see cref="HitObject"/> to be represented by this <see cref="DrawableHitObject"/>.
        /// A new <see cref="HitObjectLifetimeEntry"/> is automatically created and applied to this <see cref="DrawableHitObject"/>.
        /// </summary>
        public void Apply([NotNull] HitObject hitObject)
        {
            ArgumentNullException.ThrowIfNull(hitObject);

            Apply(new SyntheticHitObjectEntry(hitObject));
        }

        protected sealed override void OnApply(HitObjectLifetimeEntry entry)
        {
            Debug.Assert(Entry != null);

            // LifetimeStart is already computed using HitObjectLifetimeEntry's InitialLifetimeOffset.
            // We override this with DHO's InitialLifetimeOffset for a non-pooled DHO.
            if (entry is SyntheticHitObjectEntry)
                LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;

            ensureEntryHasResult();

            entry.RevertResult += onRevertResult;

            foreach (var h in HitObject.NestedHitObjects)
            {
                var pooledDrawableNested = pooledObjectProvider?.GetPooledDrawableRepresentation(h, this);
                var drawableNested = pooledDrawableNested
                                     ?? CreateNestedHitObject(h)
                                     ?? throw new InvalidOperationException($"{nameof(CreateNestedHitObject)} returned null for {h.GetType().ReadableName()}.");

                // Only invoke the event for non-pooled DHOs, otherwise the event will be fired by the playfield.
                if (pooledDrawableNested == null)
                    OnNestedDrawableCreated?.Invoke(drawableNested);

                drawableNested.OnNewResult += onNewResult;
                drawableNested.OnRevertResult += onNestedRevertResult;
                drawableNested.ApplyCustomUpdateState += onApplyCustomUpdateState;

                // This is only necessary for non-pooled DHOs. For pooled DHOs, this is handled inside GetPooledDrawableRepresentation().
                // Must be done before the nested DHO is added to occur before the nested Apply()!
                drawableNested.ParentHitObject = this;

                nestedHitObjects.Add(drawableNested);

                // assume that synthetic entries are not pooled and therefore need to be managed from within the DHO.
                // this is important for the correctness of value of flags such as `AllJudged`.
                if (drawableNested.Entry is SyntheticHitObjectEntry syntheticNestedEntry)
                    Entry.NestedEntries.Add(syntheticNestedEntry);

                AddNestedHitObject(drawableNested);
            }

            StartTimeBindable.BindTo(HitObject.StartTimeBindable);

            if (HitObject is IHasComboInformation combo)
            {
                comboIndexBindable.BindTo(combo.ComboIndexBindable);
                comboIndexWithOffsetsBindable.BindTo(combo.ComboIndexWithOffsetsBindable);
            }

            samplesBindable.BindTo(HitObject.SamplesBindable);
            samplesBindable.BindCollectionChanged(onSamplesChanged, true);

            HitObject.DefaultsApplied += onDefaultsApplied;

            OnApply();
            HitObjectApplied?.Invoke(this);

            // If not loaded, the state update happens in LoadComplete().
            if (IsLoaded)
            {
                updateStateFromResult();

                // Combo colour may have been applied via a bindable flow while no object entry was attached.
                // Update here to ensure we're in a good state.
                UpdateComboColour();
            }
        }

        private void updateStateFromResult()
        {
            if (Result.IsHit)
                updateState(ArmedState.Hit, true);
            else if (Result.HasResult)
                updateState(ArmedState.Miss, true);
            else
                updateState(ArmedState.Idle, true);
        }

        protected sealed override void OnFree(HitObjectLifetimeEntry entry)
        {
            Debug.Assert(Entry != null);

            StartTimeBindable.UnbindFrom(HitObject.StartTimeBindable);

            if (HitObject is IHasComboInformation combo)
            {
                comboIndexBindable.UnbindFrom(combo.ComboIndexBindable);
                comboIndexWithOffsetsBindable.UnbindFrom(combo.ComboIndexWithOffsetsBindable);
            }

            samplesBindable.UnbindFrom(HitObject.SamplesBindable);

            // When a new hitobject is applied, the samples will be cleared before re-populating.
            // In order to stop this needless update, the event is unbound and re-bound as late as possible in Apply().
            samplesBindable.CollectionChanged -= onSamplesChanged;

            // Release the samples for other hitobjects to use.
            Samples?.ClearSamples();

            foreach (var obj in nestedHitObjects)
            {
                obj.OnNewResult -= onNewResult;
                obj.OnRevertResult -= onNestedRevertResult;
                obj.ApplyCustomUpdateState -= onApplyCustomUpdateState;
            }

            nestedHitObjects.Clear();
            // clean up synthetic entries manually added in `Apply()`.
            Entry.NestedEntries.RemoveAll(nestedEntry => nestedEntry is SyntheticHitObjectEntry);
            ClearNestedHitObjects();

            // Changes to `HitObject` properties trigger default application, which triggers `State` updates.
            // When a new hitobject is applied, `OnApply()` automatically performs a state update.
            HitObject.DefaultsApplied -= onDefaultsApplied;

            entry.RevertResult -= onRevertResult;

            OnFree();

            ParentHitObject = null;

            clearExistingStateTransforms();
        }

        /// <summary>
        /// Invoked for this <see cref="DrawableHitObject"/> to take on any values from a newly-applied <see cref="HitObject"/>.
        /// This is also fired after any changes which occurred via an <see cref="osu.Game.Rulesets.Objects.HitObject.ApplyDefaults"/> call.
        /// </summary>
        protected virtual void OnApply()
        {
        }

        /// <summary>
        /// Invoked for this <see cref="DrawableHitObject"/> to revert any values previously taken on from the currently-applied <see cref="HitObject"/>.
        /// This is also fired after any changes which occurred via an <see cref="osu.Game.Rulesets.Objects.HitObject.ApplyDefaults"/> call.
        /// </summary>
        protected virtual void OnFree()
        {
        }

        /// <summary>
        /// Invoked by the base <see cref="DrawableHitObject"/> to populate samples, once on initial load and potentially again on any change to the samples collection.
        /// </summary>
        protected virtual void LoadSamples()
        {
            var samples = GetSamples().ToArray();

            if (samples.Length <= 0)
                return;

            Samples.Samples = samples.Cast<ISampleInfo>().ToArray();
        }

        private void onSamplesChanged(object sender, NotifyCollectionChangedEventArgs e) => LoadSamples();

        private void onNewResult(DrawableHitObject drawableHitObject, JudgementResult result) => OnNewResult?.Invoke(drawableHitObject, result);

        private void onRevertResult()
        {
            updateState(ArmedState.Idle);
            OnRevertResult?.Invoke(this, Result);
        }

        private void onNestedRevertResult(DrawableHitObject drawableHitObject, JudgementResult result) => OnRevertResult?.Invoke(drawableHitObject, result);

        private void onApplyCustomUpdateState(DrawableHitObject drawableHitObject, ArmedState state) => ApplyCustomUpdateState?.Invoke(drawableHitObject, state);

        private void onDefaultsApplied(HitObject hitObject)
        {
            Debug.Assert(Entry != null);
            Apply(Entry);

            // Applied defaults indicate a change in hit object state.
            // We need to update the judgement result time to the new end time
            // and update state to ensure the hit object fades out at the correct time.
            if (Result is not null)
            {
                Result.TimeOffset = 0;
                updateState(State.Value, true);
            }

            DefaultsApplied?.Invoke(this);
        }

        /// <summary>
        /// Invoked by the base <see cref="DrawableHitObject"/> to add nested <see cref="DrawableHitObject"/>s to the hierarchy.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to be added.</param>
        protected virtual void AddNestedHitObject(DrawableHitObject hitObject)
        {
        }

        /// <summary>
        /// Invoked by the base <see cref="DrawableHitObject"/> to remove all previously-added nested <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual void ClearNestedHitObjects()
        {
        }

        /// <summary>
        /// Creates the drawable representation for a nested <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/>.</param>
        /// <returns>The drawable representation for <paramref name="hitObject"/>.</returns>
        protected virtual DrawableHitObject CreateNestedHitObject(HitObject hitObject) => null;

        #region State / Transform Management

        /// <summary>
        /// Invoked by this or a nested <see cref="DrawableHitObject"/> to apply a custom state that can override the default implementation.
        /// </summary>
        public event Action<DrawableHitObject, ArmedState> ApplyCustomUpdateState;

        protected override void ClearInternal(bool disposeChildren = true) =>
            // See sample addition in load method.
            throw new InvalidOperationException(
                $"Should never clear a {nameof(DrawableHitObject)} as the base implementation adds components. If attempting to use {nameof(InternalChild)} or {nameof(InternalChildren)}, using {nameof(AddInternal)} or {nameof(AddRangeInternal)} instead.");

        private void updateState(ArmedState newState, bool force = false)
        {
            if (State.Value == newState && !force)
                return;

            LifetimeEnd = double.MaxValue;

            clearExistingStateTransforms();

            double initialTransformsTime = HitObject.StartTime - InitialLifetimeOffset;

            AnimationStartTime.Value = initialTransformsTime;

            using (BeginAbsoluteSequence(initialTransformsTime))
                UpdateInitialTransforms();

            using (BeginAbsoluteSequence(StateUpdateTime))
                UpdateStartTimeStateTransforms();

            using (BeginAbsoluteSequence(HitStateUpdateTime))
                UpdateHitStateTransforms(newState);

            state.Value = newState;

            if (LifetimeEnd == double.MaxValue && (state.Value != ArmedState.Idle || HitObject.HitWindows == null))
                LifetimeEnd = Math.Max(LatestTransformEndTime, HitStateUpdateTime + (Samples?.Length ?? 0));

            // apply any custom state overrides
            ApplyCustomUpdateState?.Invoke(this, newState);

            if (!force && newState == ArmedState.Hit)
                PlaySamples();
        }

        private void clearExistingStateTransforms()
        {
            base.ApplyTransformsAt(double.MinValue, true);

            // has to call this method directly (not ClearTransforms) to bypass the local ClearTransformsAfter override.
            base.ClearTransformsAfter(double.MinValue, true);
        }

        /// <summary>
        /// Reapplies the current <see cref="ArmedState"/>.
        /// </summary>
        public void RefreshStateTransforms() => updateState(State.Value, true);

        /// <summary>
        /// Apply (generally fade-in) transforms leading into the <see cref="HitObject"/> start time.
        /// By default, this will fade in the object from zero with no duration.
        /// </summary>
        /// <remarks>
        /// This is called once before every <see cref="UpdateHitStateTransforms"/>. This is to ensure a good state in the case
        /// the <see cref="JudgementResult.TimeOffset"/> was negative and potentially altered the pre-hit transforms.
        /// </remarks>
        protected virtual void UpdateInitialTransforms()
        {
            this.FadeInFromZero();
        }

        /// <summary>
        /// Apply passive transforms at the <see cref="HitObject"/>'s StartTime.
        /// This is called each time <see cref="State"/> changes.
        /// Previous states are automatically cleared.
        /// </summary>
        protected virtual void UpdateStartTimeStateTransforms()
        {
        }

        /// <summary>
        /// Apply transforms based on the current <see cref="ArmedState"/>. This call is offset by <see cref="HitStateUpdateTime"/> (HitObject.EndTime + Result.Offset), equivalent to when the user hit the object.
        /// If <see cref="Drawable.LifetimeEnd"/> was not set during this call, <see cref="Drawable.Expire"/> will be invoked.
        /// Previous states are automatically cleared.
        /// </summary>
        /// <param name="state">The new armed state.</param>
        protected virtual void UpdateHitStateTransforms(ArmedState state)
        {
        }

        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null)
        {
            // Parent calls to this should be blocked for safety, as we are manually handling this in updateState.
        }

        public override void ApplyTransformsAt(double time, bool propagateChildren = false)
        {
            // Parent calls to this should be blocked for safety, as we are manually handling this in updateState.
        }

        #endregion

        #region Skinning

        protected ISkinSource CurrentSkin { get; private set; }

        private void skinSourceChanged() => Scheduler.AddOnce(skinChanged);

        private void skinChanged()
        {
            UpdateComboColour();

            ApplySkin(CurrentSkin, true);

            if (IsLoaded)
                updateState(State.Value, true);
        }

        protected void UpdateComboColour()
        {
            if (!(HitObject is IHasComboInformation combo)) return;

            Color4 colour = combo.GetComboColour(CurrentSkin);

            // Normalise the combo colour to the given brightness level.
            if (comboColourBrightness.Value != 0)
            {
                colour = Interpolation.ValueAt(Math.Abs(comboColourBrightness.Value), colour, new HSPAColour(colour) { P = 0.6f }.ToColor4(), 0, 1);
            }

            AccentColour.Value = colour;
        }

        /// <summary>
        /// Called when a change is made to the skin.
        /// </summary>
        /// <param name="skin">The new skin.</param>
        /// <param name="allowFallback">Whether fallback to default skin should be allowed if the custom skin is missing this resource.</param>
        protected virtual void ApplySkin(ISkinSource skin, bool allowFallback)
        {
        }

        /// <summary>
        /// Calculate the position to be used for sample playback at a specified X position (0..1).
        /// </summary>
        /// <param name="position">The lookup X position. Generally should be <see cref="SamplePlaybackPosition"/>.</param>
        protected double CalculateSamplePlaybackBalance(double position)
        {
            float balanceAdjustAmount = positionalHitsoundsLevel.Value * 2;
            double returnedValue = balanceAdjustAmount * (position - 0.5f);

            return returnedValue;
        }

        /// <summary>
        /// Plays all the hit sounds for this <see cref="DrawableHitObject"/>.
        /// This is invoked automatically when this <see cref="DrawableHitObject"/> is hit.
        /// </summary>
        public virtual void PlaySamples()
        {
            if (Samples != null)
            {
                Samples.Balance.Value = CalculateSamplePlaybackBalance(SamplePlaybackPosition);
                Samples.Play();
            }
        }

        /// <summary>
        /// Stops playback of all relevant samples. Generally only looping samples should be stopped by this, and the rest let to play out.
        /// Automatically called when <see cref="DrawableHitObject{TObject}"/>'s lifetime has been exceeded.
        /// </summary>
        public virtual void StopAllSamples()
        {
            if (Samples?.Looping == true)
                Samples.Stop();
        }

        #endregion

        public override bool UpdateSubTreeMasking(Drawable source, RectangleF maskingBounds) => false;

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

        /// <summary>
        /// An offset prior to the start time of <see cref="HitObject"/> at which this <see cref="DrawableHitObject"/> may begin displaying contents.
        /// By default, <see cref="DrawableHitObject"/>s are assumed to display their contents within 10 seconds prior to the start time of <see cref="HitObject"/>.
        /// </summary>
        /// <remarks>
        /// The initial transformation (<see cref="UpdateInitialTransforms"/>) starts at this offset before the start time of <see cref="HitObject"/>.
        /// </remarks>
        protected virtual double InitialLifetimeOffset => 10000;

        /// <summary>
        /// The time at which state transforms should be applied that line up to <see cref="HitObject"/>'s StartTime.
        /// This is used to offset calls to <see cref="UpdateStartTimeStateTransforms"/>.
        /// </summary>
        public double StateUpdateTime => HitObject.StartTime;

        /// <summary>
        /// The time at which judgement dependent state transforms should be applied. This is equivalent of the (end) time of the object, in addition to any judgement offset.
        /// This is used to offset calls to <see cref="UpdateHitStateTransforms"/>.
        /// </summary>
        public double HitStateUpdateTime => Result?.TimeAbsolute ?? HitObject.GetEndTime();

        /// <summary>
        /// Will be called at least once after this <see cref="DrawableHitObject"/> has become not alive.
        /// </summary>
        public virtual void OnKilled()
        {
            foreach (var nested in NestedHitObjects)
                nested.OnKilled();

            // failsafe to ensure looping samples don't get stuck in a playing state.
            // this could occur in a non-frame-stable context where DrawableHitObjects get killed before a SkinnableSound has the chance to be stopped.
            StopAllSamples();

            UpdateResult(false);
        }

        /// <summary>
        /// Applies the <see cref="Result"/> of this <see cref="DrawableHitObject"/>, notifying responders such as
        /// the <see cref="ScoreProcessor"/> of the <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="application">The callback that applies changes to the <see cref="JudgementResult"/>.</param>
        protected void ApplyResult(Action<JudgementResult> application)
        {
            if (Result.HasResult)
                throw new InvalidOperationException("Cannot apply result on a hitobject that already has a result.");

            application?.Invoke(Result);

            if (!Result.HasResult)
                throw new InvalidOperationException($"{GetType().ReadableName()} applied a {nameof(JudgementResult)} but did not update {nameof(JudgementResult.Type)}.");

            HitResultExtensions.ValidateHitResultPair(Result.Judgement.MaxResult, Result.Judgement.MinResult);

            if (!Result.Type.IsValidHitResult(Result.Judgement.MinResult, Result.Judgement.MaxResult))
            {
                throw new InvalidOperationException(
                    $"{GetType().ReadableName()} applied an invalid hit result (was: {Result.Type}, expected: [{Result.Judgement.MinResult} ... {Result.Judgement.MaxResult}]).");
            }

            Result.RawTime = Time.Current;
            Result.GameplayRate = Clock.Rate;

            if (Result.HasResult)
                updateState(Result.IsHit ? ArmedState.Hit : ArmedState.Miss);

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
            if ((Clock as IGameplayClock)?.IsRewinding == true)
                return false;

            if (Judged)
                return false;

            CheckForResult(userTriggered, Time.Current - HitObject.GetEndTime());

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

        private void ensureEntryHasResult()
        {
            Debug.Assert(Entry != null);
            Entry.Result ??= CreateResult(HitObject.CreateJudgement())
                             ?? throw new InvalidOperationException($"{GetType().ReadableName()} must provide a {nameof(JudgementResult)} through {nameof(CreateResult)}.");
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (HitObject != null)
                HitObject.DefaultsApplied -= onDefaultsApplied;

            if (CurrentSkin != null)
                CurrentSkin.SourceChanged -= skinSourceChanged;
        }

        public Bindable<double> AnimationStartTime { get; } = new BindableDouble();
    }

    public abstract partial class DrawableHitObject<TObject> : DrawableHitObject
        where TObject : HitObject
    {
        public new TObject HitObject => (TObject)base.HitObject;

        protected DrawableHitObject([CanBeNull] TObject hitObject)
            : base(hitObject)
        {
        }
    }
}
