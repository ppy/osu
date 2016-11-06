﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Objects.Osu.Drawable;

namespace osu.Game.Beatmaps.Objects
{
    public abstract class DrawableHitObject : Container, IStateful<ArmedState>
    {
        //todo: move to a more central implementation. this logic should not be at a drawable level.
        public Action<DrawableHitObject> OnHit;
        public Action<DrawableHitObject> OnMiss;

        public Func<DrawableHitObject, bool> AllowHit;

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

        protected double? HitTime;

        protected virtual bool Hit()
        {
            if (State != ArmedState.Disarmed)
                return false;

            if (AllowHit?.Invoke(this) == false)
                return false;

            HitTime = Time.Current;

            State = ArmedState.Armed;
            return true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UpdateState(state);
        }

        private bool counted;

        protected override void Update()
        {
            base.Update();

            if (Time.Current >= HitObject.EndTime && !counted)
            {
                counted = true;
                if (state == ArmedState.Armed)
                    OnHit?.Invoke(this);
                else
                    OnMiss?.Invoke(this);
            }
        }

        protected abstract void UpdateState(ArmedState state);
    }

    public enum ArmedState
    {
        Disarmed,
        Armed
    }
}
