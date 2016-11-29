//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using System.Diagnostics;
using osu.Framework;
using osu.Framework.Graphics.Containers;
using OpenTK;
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Judgement = CreateJudgementInfo();
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

        protected override void Update()
        {
            base.Update();

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
