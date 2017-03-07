// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Game.Beatmaps.Samples;
using OpenTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Modes.Objects.Drawables
{
    public abstract class DrawableHitObject : Container, IStateful<ArmedState>
    {
        public override bool HandleInput => Interactive;

        public bool Interactive = true;

        public JudgementInfo Judgement;

        protected abstract JudgementInfo CreateJudgementInfo();

        protected abstract void UpdateState(ArmedState state);

        private ArmedState state;
        public ArmedState State
        {
            get { return state; }

            set
            {
                if (state == value) return;
                state = value;

                UpdateState(state);
                if (IsLoaded)
                    Expire();

                if (State == ArmedState.Hit)
                    PlaySample();
            }
        }

        protected SampleChannel Sample;

        protected virtual void PlaySample()
        {
            Sample?.Play();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //we may be setting a custom judgement in test cases or what not.
            if (Judgement == null)
                Judgement = CreateJudgementInfo();

            //force application of the state that was set before we loaded.
            UpdateState(State);

            Expire(true);
        }
    }

    public abstract class DrawableHitObject<HitObjectType> : DrawableHitObject
        where HitObjectType : HitObject
    {
        public event Action<DrawableHitObject<HitObjectType>, JudgementInfo> OnJudgement;

        public HitObjectType HitObject;

        public DrawableHitObject(HitObjectType hitObject)
        {
            HitObject = hitObject;
        }

        /// <summary>
        /// Process a hit of this hitobject. Carries out judgement.
        /// </summary>
        /// <returns>Whether a hit was processed.</returns>
        protected bool UpdateJudgement(bool userTriggered)
        {
            if (Judgement.Result != null)
                return false;

            Judgement.TimeOffset = Time.Current - HitObject.EndTime;

            CheckJudgement(userTriggered);

            if (Judgement.Result == null)
                return false;

            switch (Judgement.Result)
            {
                default:
                    State = ArmedState.Hit;
                    break;
                case HitResult.Miss:
                    State = ArmedState.Miss;
                    break;
            }

            OnJudgement?.Invoke(this, Judgement);

            return true;
        }

        protected virtual void CheckJudgement(bool userTriggered)
        {
            if (NestedHitObjects != null)
            {
                foreach (var d in NestedHitObjects)
                    d.CheckJudgement(userTriggered);
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            UpdateJudgement(false);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            string hitType = ((HitObject.Sample?.Type ?? SampleType.None) == SampleType.None ? SampleType.Normal : HitObject.Sample.Type).ToString().ToLower();
            string sampleSet = (HitObject.Sample?.Set ?? SampleSet.Normal).ToString().ToLower();

            Sample = audio.Sample.Get($@"Gameplay/{sampleSet}-hit{hitType}");
        }

        private List<DrawableHitObject<HitObjectType>> nestedHitObjects;

        protected IEnumerable<DrawableHitObject<HitObjectType>> NestedHitObjects => nestedHitObjects;

        protected void AddNested(DrawableHitObject<HitObjectType> h)
        {
            if (nestedHitObjects == null)
                nestedHitObjects = new List<DrawableHitObject<HitObjectType>>();

            h.OnJudgement += (d, j) => { OnJudgement?.Invoke(d, j); } ;
            nestedHitObjects.Add(h);
        }
    }

    public enum ArmedState
    {
        Idle,
        Hit,
        Miss
    }

    public class PositionalJudgementInfo : JudgementInfo
    {
        public Vector2 PositionOffset;
    }

    public class JudgementInfo
    {
        public ulong? ComboAtHit;
        public HitResult? Result;
        public double TimeOffset;
    }

    public enum HitResult
    {
        [Description(@"Miss")]
        Miss,
        [Description(@"Hit")]
        Hit,
    }
}
