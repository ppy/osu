﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
        public event Action<DrawableHitObject, JudgementInfo> OnJudgement;

        public override bool HandleInput => Interactive;

        public bool Interactive = true;

        public JudgementInfo Judgement;

        public abstract JudgementInfo CreateJudgementInfo();

        public HitObject HitObject;

        public DrawableHitObject(HitObject hitObject)
        {
            HitObject = hitObject;
        }

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

        SampleChannel sample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            string hitType = ((HitObject.Sample?.Type ?? SampleType.None) == SampleType.None ? SampleType.Normal : HitObject.Sample.Type).ToString().ToLower();
            string sampleSet = (HitObject.Sample?.Set ?? SampleSet.Normal).ToString().ToLower();

            sample = audio.Sample.Get($@"Gameplay/{sampleSet}-hit{hitType}");
        }

        protected virtual void PlaySample()
        {
            sample?.Play();
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

        private List<DrawableHitObject> nestedHitObjects;

        protected IEnumerable<DrawableHitObject> NestedHitObjects => nestedHitObjects;

        protected void AddNested(DrawableHitObject h)
        {
            if (nestedHitObjects == null)
                nestedHitObjects = new List<DrawableHitObject>();

            h.OnJudgement += (d, j) => { OnJudgement?.Invoke(d, j); } ;
            nestedHitObjects.Add(h);
        }

        /// <summary>
        /// Process a hit of this hitobject. Carries out judgement.
        /// </summary>
        /// <param name="judgement">Preliminary judgement information provided by the hit source.</param>
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

        protected abstract void UpdateState(ArmedState state);
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
