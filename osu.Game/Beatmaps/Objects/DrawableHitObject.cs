//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
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
        public Action<DrawableHitObject> OnHit;
        public Action<DrawableHitObject> OnMiss;

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
                state = value;

                UpdateState(state);
            }
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            UpdateState(state);
        }

        private bool counted;

        protected override void Update()
        {
            base.Update();

            if (Time >= HitObject.EndTime && !counted)
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
