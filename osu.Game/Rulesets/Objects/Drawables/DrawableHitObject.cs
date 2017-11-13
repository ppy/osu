// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Game.Rulesets.Judgements;
using Container = osu.Framework.Graphics.Containers.Container;
using osu.Game.Rulesets.Objects.Types;
using OpenTK.Graphics;
using osu.Game.Audio;
using System.Linq;
using osu.Game.Graphics;
using osu.Framework.Configuration;

namespace osu.Game.Rulesets.Objects.Drawables
{
    public abstract class DrawableHitObject : Container, IHasAccentColour
    {
        public readonly HitObject HitObject;

        /// <summary>
        /// The colour used for various elements of this DrawableHitObject.
        /// </summary>
        public virtual Color4 AccentColour { get; set; } = Color4.Gray;

        /// <summary>
        /// Whether a visible judgement should be displayed when this representation is hit.
        /// </summary>
        public virtual bool DisplayJudgement => true;

        public override bool RemoveCompletedTransforms => false;
        public override bool RemoveWhenNotAlive => false;

        protected DrawableHitObject(HitObject hitObject)
        {
            HitObject = hitObject;
        }
    }

    public abstract class DrawableHitObject<TObject> : DrawableHitObject
        where TObject : HitObject
    {
        public event Action<DrawableHitObject, Judgement> OnJudgement;
        public event Action<DrawableHitObject, Judgement> OnJudgementRemoved;

        public new readonly TObject HitObject;

        public override bool HandleInput => Interactive;
        public bool Interactive = true;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> can be judged.
        /// </summary>
        protected virtual bool ProvidesJudgement => true;

        private readonly List<Judgement> judgements = new List<Judgement>();
        public IReadOnlyList<Judgement> Judgements => judgements;

        protected List<SampleChannel> Samples = new List<SampleChannel>();

        public readonly Bindable<ArmedState> State = new Bindable<ArmedState>();

        protected DrawableHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            foreach (SampleInfo sample in HitObject.Samples)
            {
                SampleChannel channel = audio.Sample.Get($@"Gameplay/{sample.Bank}-{sample.Name}");

                if (channel == null)
                    continue;

                channel.Volume.Value = sample.Volume;
                Samples.Add(channel);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.ValueChanged += state =>
            {
                UpdateState(state);

                if (State == ArmedState.Hit)
                    PlaySamples();
            };

            State.TriggerChange();
        }

        protected void PlaySamples()
        {
            Samples.ForEach(s => s?.Play());
        }

        private bool judgementOccurred;
        private bool judgementFinalized => judgements.LastOrDefault()?.Final == true;

        /// <summary>
        /// Whether this <see cref="DrawableHitObject"/> and all of its nested <see cref="DrawableHitObject"/>s have been judged.
        /// </summary>
        public bool AllJudged => (!ProvidesJudgement || judgementFinalized) && (NestedHitObjects?.All(h => h.AllJudged) ?? true);

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

            if (AllJudged || State != ArmedState.Idle)
                return false;

            if (NestedHitObjects != null)
            {
                foreach (var d in NestedHitObjects)
                    judgementOccurred |= d.UpdateJudgement(userTriggered);
            }

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
        protected virtual void CheckForJudgements(bool userTriggered, double timeOffset) { }

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

        private List<DrawableHitObject<TObject>> nestedHitObjects;
        protected IEnumerable<DrawableHitObject<TObject>> NestedHitObjects => nestedHitObjects;

        protected virtual void AddNested(DrawableHitObject<TObject> h)
        {
            if (nestedHitObjects == null)
                nestedHitObjects = new List<DrawableHitObject<TObject>>();

            h.OnJudgement += (d, j) => OnJudgement?.Invoke(d, j);
            h.OnJudgementRemoved += (d, j) => OnJudgementRemoved?.Invoke(d, j);
            nestedHitObjects.Add(h);
        }

        protected abstract void UpdateState(ArmedState state);
    }
}
