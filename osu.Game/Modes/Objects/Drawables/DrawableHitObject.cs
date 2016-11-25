//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using osu.Framework;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.Objects.Drawables
{
    public abstract class DrawableHitObject : Container, IStateful<ArmedState>
    {
        //todo: move to a more central implementation. this logic should not be at a drawable level.
        public Action<DrawableHitObject> OnHit;
        public Action<DrawableHitObject> OnMiss;

        public Action<DrawableHitObject, JudgementInfo> CheckJudgement;

        public Container<DrawableHitObject> ChildObjects;

        public JudgementInfo Judgement;

        public HitObject HitObject;

        public DrawableHitObject(HitObject hitObject)
        {
            HitObject = hitObject;
            Depth = -(float)hitObject.StartTime;
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
            }
        }

        /// <summary>
        /// Process a hit of this hitobject. Carries out judgement.
        /// </summary>
        /// <param name="judgement">Preliminary judgement information provided by the hit source.</param>
        /// <returns>Whether a hit was processed.</returns>
        protected bool Hit(JudgementInfo judgement)
        {
            if (State != ArmedState.Idle)
                return false;

            judgement.TimeOffset = Time.Current - HitObject.EndTime;

            CheckJudgement?.Invoke(this, judgement);

            if (judgement.Result == HitResult.Ignore)
                return false;

            Judgement = judgement;

            switch (judgement.Result)
            {
                default:
                    State = ArmedState.Hit;
                    OnHit?.Invoke(this);
                    break;
                case HitResult.Miss:
                    State = ArmedState.Miss;
                    OnMiss?.Invoke(this);
                    break;
            }

            
            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (Time.Current >= HitObject.EndTime && Judgement == null)
                Hit(new JudgementInfo());
        }

        protected abstract void UpdateState(ArmedState state);
    }

    public enum ArmedState
    {
        Idle,
        Hit,
        Miss
    }
}
