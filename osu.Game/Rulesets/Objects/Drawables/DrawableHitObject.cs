// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Configuration;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Objects.Drawables
{
    [Cached(typeof(DrawableHitObject))]
    public abstract class DrawableHitObject : SkinReloadableDrawable
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
        public HitObject HitObject { get; private set; }

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

        private readonly Lazy<List<DrawableHitObject>> nestedHitObjects = new Lazy<List<DrawableHitObject>>();
        public IReadOnlyList<DrawableHitObject> NestedHitObjects => nestedHitObjects.IsValueCreated ? nestedHitObjects.Value : (IReadOnlyList<DrawableHitObject>)Array.Empty<DrawableHitObject>();

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
        private readonly Bindable<bool> userPositionalHitSounds = new Bindable<bool>();
        private readonly Bindable<int> comboIndexBindable = new Bindable<int>();

        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;
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

        /// <summary>
        /// Whether <see cref="HitObject"/> is currently applied.
        /// </summary>
        private bool hasHitObjectApplied;

        /// <summary>
        /// The <see cref="HitObjectLifetimeEntry"/> controlling the lifetime of the currently-attached <see cref="HitObject"/>.
        /// </summary>
        [CanBeNull]
        private HitObjectLifetimeEntry lifetimeEntry;

        [Resolved(CanBeNull = true)]
        private IPooledHitObjectProvider pooledObjectProvider { get; set; }

        /// <summary>
        /// Whether the initialization logic in <see cref="Playfield" /> has applied.
        /// </summary>
        internal bool IsInitialized;

        /// <summary>
        /// Creates a new <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="initialHitObject">
        /// The <see cref="HitObject"/> to be initially applied to this <see cref="DrawableHitObject"/>.
        /// If <c>null</c>, a hitobject is expected to be later applied via <see cref="Apply"/> (or automatically via pooling).
        /// </param>
        protected DrawableHitObject([CanBeNull] HitObject initialHitObject = null)
        {
            HitObject = initialHitObject;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.PositionalHitSounds, userPositionalHitSounds);

            // Explicit non-virtual function call.
            base.AddInternal(Samples = new PausableSkinnableSound());
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();

            if (HitObject != null)
                Apply(HitObject, lifetimeEntry);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            comboIndexBindable.BindValueChanged(_ => UpdateComboColour(), true);

            updateState(ArmedState.Idle, true);
        }

        /// <summary>
        /// Applies a new <see cref="HitObject"/> to be represented by this <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to apply.</param>
        /// <param name="lifetimeEntry">The <see cref="HitObjectLifetimeEntry"/> controlling the lifetime of <paramref name="hitObject"/>.</param>
        public void Apply([NotNull] HitObject hitObject, [CanBeNull] HitObjectLifetimeEntry lifetimeEntry)
        {
            free();

            HitObject = hitObject ?? throw new InvalidOperationException($"Cannot apply a null {nameof(HitObject)}.");

            this.lifetimeEntry = lifetimeEntry;

            if (lifetimeEntry != null)
            {
                // Transfer lifetime from the entry.
                LifetimeStart = lifetimeEntry.LifetimeStart;
                LifetimeEnd = lifetimeEntry.LifetimeEnd;

                // Copy any existing result from the entry (required for rewind / judgement revert).
                Result = lifetimeEntry.Result;
            }
            else
                LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;

            // Ensure this DHO has a result.
            Result ??= CreateResult(HitObject.CreateJudgement())
                       ?? throw new InvalidOperationException($"{GetType().ReadableName()} must provide a {nameof(JudgementResult)} through {nameof(CreateResult)}.");

            // Copy back the result to the entry for potential future retrieval.
            if (lifetimeEntry != null)
                lifetimeEntry.Result = Result;

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
                drawableNested.OnRevertResult += onRevertResult;
                drawableNested.ApplyCustomUpdateState += onApplyCustomUpdateState;

                // This is only necessary for non-pooled DHOs. For pooled DHOs, this is handled inside GetPooledDrawableRepresentation().
                // Must be done before the nested DHO is added to occur before the nested Apply()!
                drawableNested.ParentHitObject = this;

                nestedHitObjects.Value.Add(drawableNested);
                AddNestedHitObject(drawableNested);
            }

            StartTimeBindable.BindTo(HitObject.StartTimeBindable);
            StartTimeBindable.BindValueChanged(onStartTimeChanged);

            if (HitObject is IHasComboInformation combo)
                comboIndexBindable.BindTo(combo.ComboIndexBindable);

            samplesBindable.BindTo(HitObject.SamplesBindable);
            samplesBindable.BindCollectionChanged(onSamplesChanged, true);

            HitObject.DefaultsApplied += onDefaultsApplied;

            OnApply();
            HitObjectApplied?.Invoke(this);

            // If not loaded, the state update happens in LoadComplete().
            if (IsLoaded)
            {
                if (Result.IsHit)
                    updateState(ArmedState.Hit, true);
                else if (Result.HasResult)
                    updateState(ArmedState.Miss, true);
                else
                    updateState(ArmedState.Idle, true);
            }

            hasHitObjectApplied = true;
        }

        /// <summary>
        /// Removes the currently applied <see cref="HitObject"/>
        /// </summary>
        private void free()
        {
            if (!hasHitObjectApplied)
                return;

            StartTimeBindable.UnbindFrom(HitObject.StartTimeBindable);
            if (HitObject is IHasComboInformation combo)
                comboIndexBindable.UnbindFrom(combo.ComboIndexBindable);
            samplesBindable.UnbindFrom(HitObject.SamplesBindable);

            // Changes in start time trigger state updates. When a new hitobject is applied, OnApply() automatically performs a state update anyway.
            StartTimeBindable.ValueChanged -= onStartTimeChanged;

            // When a new hitobject is applied, the samples will be cleared before re-populating.
            // In order to stop this needless update, the event is unbound and re-bound as late as possible in Apply().
            samplesBindable.CollectionChanged -= onSamplesChanged;

            // Release the samples for other hitobjects to use.
            if (Samples != null)
                Samples.Samples = null;

            if (nestedHitObjects.IsValueCreated)
            {
                foreach (var obj in nestedHitObjects.Value)
                {
                    obj.OnNewResult -= onNewResult;
                    obj.OnRevertResult -= onRevertResult;
                    obj.ApplyCustomUpdateState -= onApplyCustomUpdateState;
                }

                nestedHitObjects.Value.Clear();
                ClearNestedHitObjects();
            }

            HitObject.DefaultsApplied -= onDefaultsApplied;

            OnFree();

            HitObject = null;
            ParentHitObject = null;
            Result = null;
            lifetimeEntry = null;

            clearExistingStateTransforms();

            hasHitObjectApplied = false;
        }

        protected sealed override void FreeAfterUse()
        {
            base.FreeAfterUse();

            // Freeing while not in a pool would cause the DHO to not be usable elsewhere in the hierarchy without being re-applied.
            if (!IsInPool)
                return;

            free();
        }

        /// <summary>
        /// Invoked for this <see cref="DrawableHitObject"/> to take on any values from a newly-applied <see cref="HitObject"/>.
        /// </summary>
        protected virtual void OnApply()
        {
        }

        /// <summary>
        /// Invoked for this <see cref="DrawableHitObject"/> to revert any values previously taken on from the currently-applied <see cref="HitObject"/>.
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

            if (HitObject.SampleControlPoint == null)
            {
                throw new InvalidOperationException($"{nameof(HitObject)}s must always have an attached {nameof(HitObject.SampleControlPoint)}."
                                                    + $" This is an indication that {nameof(HitObject.ApplyDefaults)} has not been invoked on {this}.");
            }

            Samples.Samples = samples.Select(s => HitObject.SampleControlPoint.ApplyTo(s)).Cast<ISampleInfo>().ToArray();
        }

        private void onSamplesChanged(object sender, NotifyCollectionChangedEventArgs e) => LoadSamples();

        private void onStartTimeChanged(ValueChangedEvent<double> startTime) => updateState(State.Value, true);

        private void onNewResult(DrawableHitObject drawableHitObject, JudgementResult result) => OnNewResult?.Invoke(drawableHitObject, result);

        private void onRevertResult(DrawableHitObject drawableHitObject, JudgementResult result) => OnRevertResult?.Invoke(drawableHitObject, result);

        private void onApplyCustomUpdateState(DrawableHitObject drawableHitObject, ArmedState state) => ApplyCustomUpdateState?.Invoke(drawableHitObject, state);

        private void onDefaultsApplied(HitObject hitObject)
        {
            Apply(hitObject, lifetimeEntry);
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

        protected override void ClearInternal(bool disposeChildren = true) => throw new InvalidOperationException($"Should never clear a {nameof(DrawableHitObject)}");

        private void updateState(ArmedState newState, bool force = false)
        {
            if (State.Value == newState && !force)
                return;

            LifetimeEnd = double.MaxValue;

            double transformTime = HitObject.StartTime - InitialLifetimeOffset;

            clearExistingStateTransforms();

            using (BeginAbsoluteSequence(transformTime, true))
                UpdateInitialTransforms();

            using (BeginAbsoluteSequence(StateUpdateTime, true))
                UpdateStartTimeStateTransforms();

#pragma warning disable 618
            using (BeginAbsoluteSequence(StateUpdateTime + (Result?.TimeOffset ?? 0), true))
                UpdateStateTransforms(newState);
#pragma warning restore 618

            using (BeginAbsoluteSequence(HitStateUpdateTime, true))
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
        [Obsolete("Use UpdateStartTimeStateTransforms and UpdateHitStateTransforms instead")] // Can be removed 20210504
        protected virtual void UpdateStateTransforms(ArmedState state)
        {
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

        protected sealed override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            UpdateComboColour();

            ApplySkin(skin, allowFallback);

            if (IsLoaded)
                updateState(State.Value, true);
        }

        protected void UpdateComboColour()
        {
            if (!(HitObject is IHasComboInformation combo)) return;

            var comboColours = CurrentSkin.GetConfig<GlobalSkinColours, IReadOnlyList<Color4>>(GlobalSkinColours.ComboColours)?.Value ?? Array.Empty<Color4>();
            AccentColour.Value = combo.GetComboColour(comboColours);
        }

        /// <summary>
        /// Called to retrieve the combo colour. Automatically assigned to <see cref="AccentColour"/>.
        /// Defaults to using <see cref="IHasComboInformation.ComboIndex"/> to decide on a colour.
        /// </summary>
        /// <remarks>
        /// This will only be called if the <see cref="HitObject"/> implements <see cref="IHasComboInformation"/>.
        /// </remarks>
        /// <param name="comboColours">A list of combo colours provided by the beatmap or skin. Can be null if not available.</param>
        [Obsolete("Unused. Implement IHasComboInformation and IHasComboInformation.GetComboColour() on the HitObject model instead.")] // Can be removed 20210527
        protected virtual Color4 GetComboColour(IReadOnlyList<Color4> comboColours)
        {
            if (!(HitObject is IHasComboInformation combo))
                throw new InvalidOperationException($"{nameof(HitObject)} must implement {nameof(IHasComboInformation)}");

            return comboColours?.Count > 0 ? comboColours[combo.ComboIndex % comboColours.Count] : Color4.White;
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
        /// <returns></returns>
        protected double CalculateSamplePlaybackBalance(double position)
        {
            const float balance_adjust_amount = 0.4f;

            return balance_adjust_amount * (userPositionalHitSounds.Value ? position - 0.5f : 0);
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

        protected override void Update()
        {
            base.Update();

            if (Result != null && Result.HasResult)
            {
                var endTime = HitObject.GetEndTime();

                if (Result.TimeOffset + endTime > Time.Current)
                {
                    OnRevertResult?.Invoke(this, Result);

                    Result.TimeOffset = 0;
                    Result.Type = HitResult.None;

                    updateState(ArmedState.Idle);
                }
            }
        }

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

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set => setLifetime(value, LifetimeEnd);
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set => setLifetime(LifetimeStart, value);
        }

        private void setLifetime(double lifetimeStart, double lifetimeEnd)
        {
            base.LifetimeStart = lifetimeStart;
            base.LifetimeEnd = lifetimeEnd;

            if (lifetimeEntry != null)
            {
                lifetimeEntry.LifetimeStart = lifetimeStart;
                lifetimeEntry.LifetimeEnd = lifetimeEnd;
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
        /// <para>
        /// Only has an effect if this <see cref="DrawableHitObject"/> is not being pooled.
        /// For pooled <see cref="DrawableHitObject"/>s, use <see cref="HitObjectLifetimeEntry.InitialLifetimeOffset"/> instead.
        /// </para>
        /// </remarks>
        protected virtual double InitialLifetimeOffset => 10000;

        /// <summary>
        /// The time at which state transforms should be applied that line up to <see cref="HitObject"/>'s StartTime.
        /// This is used to offset calls to <see cref="UpdateStateTransforms"/>.
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
        /// The maximum offset from the end time of <see cref="HitObject"/> at which this <see cref="DrawableHitObject"/> can be judged.
        /// The time offset of <see cref="Result"/> will be clamped to this value during <see cref="ApplyResult"/>.
        /// <para>
        /// Defaults to the miss window of <see cref="HitObject"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This does not affect the time offset provided to invocations of <see cref="CheckForResult"/>.
        /// </remarks>
        protected virtual double MaximumJudgementOffset => HitObject.HitWindows?.WindowFor(HitResult.Miss) ?? 0;

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

            // Some (especially older) rulesets use scorable judgements instead of the newer ignorehit/ignoremiss judgements.
            // Can be removed 20210328
            if (Result.Judgement.MaxResult == HitResult.IgnoreHit)
            {
                HitResult originalType = Result.Type;

                if (Result.Type == HitResult.Miss)
                    Result.Type = HitResult.IgnoreMiss;
                else if (Result.Type >= HitResult.Meh && Result.Type <= HitResult.Perfect)
                    Result.Type = HitResult.IgnoreHit;

                if (Result.Type != originalType)
                {
                    Logger.Log($"{GetType().ReadableName()} applied an invalid hit result ({originalType}) when {nameof(HitResult.IgnoreMiss)} or {nameof(HitResult.IgnoreHit)} is expected.\n"
                               + $"This has been automatically adjusted to {Result.Type}, and support will be removed from 2020-03-28 onwards.", level: LogLevel.Important);
                }
            }

            if (!Result.Type.IsValidHitResult(Result.Judgement.MinResult, Result.Judgement.MaxResult))
            {
                throw new InvalidOperationException(
                    $"{GetType().ReadableName()} applied an invalid hit result (was: {Result.Type}, expected: [{Result.Judgement.MinResult} ... {Result.Judgement.MaxResult}]).");
            }

            Result.TimeOffset = Math.Min(MaximumJudgementOffset, Time.Current - HitObject.GetEndTime());

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
            if (Time.Elapsed < 0)
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (HitObject != null)
                HitObject.DefaultsApplied -= onDefaultsApplied;
        }
    }

    public abstract class DrawableHitObject<TObject> : DrawableHitObject
        where TObject : HitObject
    {
        public new TObject HitObject => (TObject)base.HitObject;

        protected DrawableHitObject(TObject hitObject)
            : base(hitObject)
        {
        }
    }
}
