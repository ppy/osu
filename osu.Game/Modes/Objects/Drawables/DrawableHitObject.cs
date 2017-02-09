﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Samples;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Modes.Objects.Drawables
{
    public abstract class DrawableHitObject : Container, IStateful<ArmedState>
    {
        public event Action<DrawableHitObject, JudgementInfo> OnJudgement;

        public Container<DrawableHitObject> ChildObjects;

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
                if (state == value)
                    return;
                state = value;

                UpdateState(state);

                Expire();

                if (State == ArmedState.Hit)
                    PlaySample();
            }
        }

        private List<AudioSample> samples = new List<AudioSample>();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            // TODO: Use HitObject.Sample.Bank to load custom sample overrides,
            //       Use HitObject.Sample.File to load extra samples,
            //       Check options to make sure the user wants custom sample overrides,
            //       Check beatmap settings and enabled mods to make sure samples are playing at the correct speed
            // NOTE: Sample overrides without a prefix apply to both Soft and Normal sets

            foreach (SampleType type in Enum.GetValues(typeof(SampleType)))
            {
                if (type == SampleType.None)
                    continue;
                if (HitObject.Sample.Type.HasFlag(type))
                {
                    if (type == SampleType.Normal)
                        samples.Add(audio.Sample.Get($@"Gameplay/{HitObject.Sample.Set.ToString().ToLower()}-hitnormal"));
                    else
                        samples.Add(audio.Sample.Get($@"Gameplay/{HitObject.Sample.AdditionSet.ToString().ToLower()}-hit{type.ToString().ToLower()}"));
                }
            }
        }

        protected void PlaySample()
        {
            for (int i = 0; i < samples.Count; i++)
                samples[i].Play();
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
            //todo: consider making abstract.
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
        Hit
    }
}
