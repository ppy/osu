// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using OpenTK.Graphics;

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
        public bool HasNestedHitObjects => nestedHitObjects.IsValueCreated;
        public IReadOnlyList<DrawableHitObject> NestedHitObjects => nestedHitObjects.Value;

        public event Action<DrawableHitObject, Judgement> OnJudgement;
        public event Action<DrawableHitObject, Judgement> OnJudgementRemoved;

        public IReadOnlyList<Judgement> Judgements => judgements;
        private readonly List<Judgement> judgements = new List<Judgement>();

        /// <summary>
        /// Whether a visible judgement should be displayed when this representation is hit.
        /// </summary>
        public virtual bool DisplayJudgement => true;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> and all of its nested <see cref="DrawableHitObject"/>s have been hit.
        /// </summary>
        public bool IsHit => Judgements.Any(j => j.Final && j.IsHit) && (!HasNestedHitObjects || NestedHitObjects.All(n => n.IsHit));

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> and all of its nested <see cref="DrawableHitObject"/>s have been judged.
        /// </summary>
        public bool AllJudged => (!ProvidesJudgement || judgementFinalized) && (!HasNestedHitObjects || NestedHitObjects.All(h => h.AllJudged));

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> can be judged.
        /// </summary>
        protected virtual bool ProvidesJudgement => true;

        private bool judgementOccurred;
        private bool judgementFinalized => judgements.LastOrDefault()?.Final == true;

        public bool Interactive = true;
        public override bool HandleKeyboardInput => Interactive;
        public override bool HandleMouseInput => Interactive;

        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;
        protected override bool RequiresChildrenUpdate => true;

        public readonly Bindable<ArmedState> State = new Bindable<ArmedState>();

        protected DrawableHitObject(HitObject hitObject)
        {
            HitObject = hitObject;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var samples = GetSamples().ToArray();

            if (samples.Any())
            {
                if (HitObject.SampleControlPoint == null)
                    throw new ArgumentNullException(nameof(HitObject.SampleControlPoint), $"{nameof(HitObject)}s must always have an attached {nameof(HitObject.SampleControlPoint)}."
                                                                                          + $" This is an indication that {nameof(HitObject.ApplyDefaults)} has not been invoked on {this}.");
                AddInternal(Samples = new SkinnableSound(samples.Select(s => new SampleInfo
                {
                    Bank = s.Bank ?? HitObject.SampleControlPoint.SampleBank,
                    Name = s.Name,
                    Volume = s.Volume > 0 ? s.Volume : HitObject.SampleControlPoint.SampleVolume,
                    Namespace = SampleNamespace
                }).ToArray()));
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.ValueChanged += state =>
            {
                UpdateState(state);

                // apply any custom state overrides
                ApplyCustomUpdateState?.Invoke(this, state);

                if (State == ArmedState.Hit)
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
        /// Plays all the hitsounds for this <see cref="DrawableHitObject"/>.
        /// </summary>
        public void PlaySamples() => Samples?.Play();

        protected override void Update()
        {
            base.Update();

            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;

            while (judgements.Count > 0)
            {
                var lastJudgement = judgements[judgements.Count - 1];
                if (lastJudgement.TimeOffset + endTime <= Time.Current)
                    break;

                judgements.RemoveAt(judgements.Count - 1);
                State.Value = ArmedState.Idle;

                OnJudgementRemoved?.Invoke(this, lastJudgement);
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            UpdateJudgement(false);
        }

        protected virtual void AddNested(DrawableHitObject h)
        {
            h.OnJudgement += (d, j) => OnJudgement?.Invoke(d, j);
            h.OnJudgementRemoved += (d, j) => OnJudgementRemoved?.Invoke(d, j);
            h.ApplyCustomUpdateState += (d, j) => ApplyCustomUpdateState?.Invoke(d, j);

            nestedHitObjects.Value.Add(h);
        }

        /// <summary>
        /// Notifies that a new judgement has occurred for this <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="judgement">The <see cref="Judgement"/>.</param>
        protected void AddJudgement(Judgement judgement)
        {
            judgementOccurred = true;

            // Ensure that the judgement is given a valid time offset, because this may not get set by the caller
            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
            judgement.TimeOffset = Time.Current - endTime;

            judgements.Add(judgement);

            switch (judgement.Result)
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

            OnJudgement?.Invoke(this, judgement);
        }

        /// <summary>
        /// Processes this <see cref="DrawableHitObject"/>, checking if any judgements have occurred.
        /// </summary>
        /// <param name="userTriggered">Whether the user triggered this process.</param>
        /// <returns>Whether a judgement has occurred from this <see cref="DrawableHitObject"/> or any nested <see cref="DrawableHitObject"/>s.</returns>
        protected bool UpdateJudgement(bool userTriggered)
        {
            judgementOccurred = false;

            if (AllJudged)
                return false;

            if (HasNestedHitObjects)
                foreach (var d in NestedHitObjects)
                    judgementOccurred |= d.UpdateJudgement(userTriggered);

            if (!ProvidesJudgement || judgementFinalized || judgementOccurred)
                return judgementOccurred;

            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
            CheckForJudgements(userTriggered, Time.Current - endTime);

            return judgementOccurred;
        }

        /// <summary>
        /// Checks if any judgements have occurred for this <see cref="DrawableHitObject"/>. This method must construct
        /// all <see cref="Judgement"/>s and notify of them through <see cref="AddJudgement"/>.
        /// </summary>
        /// <param name="userTriggered">Whether the user triggered this check.</param>
        /// <param name="timeOffset">The offset from the <see cref="HitObject"/> end time at which this check occurred. A <paramref name="timeOffset"/> &gt; 0
        /// implies that this check occurred after the end time of <see cref="HitObject"/>. </param>
        protected virtual void CheckForJudgements(bool userTriggered, double timeOffset)
        {
        }
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
