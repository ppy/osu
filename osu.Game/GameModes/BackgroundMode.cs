//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.GameModes;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework;
using System.Threading;

namespace osu.Game.GameModes
{
    public abstract class BackgroundMode : GameMode, IEquatable<BackgroundMode>
    {
        public virtual bool Equals(BackgroundMode other)
        {
            return other?.GetType() == GetType();
        }

        const float transition_length = 500;
        const float x_movement_amount = 50;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            //we don't want to handle escape key.
            return false;
        }

        BaseGame game;

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            this.game = game;
        }

        public override bool Push(GameMode mode)
        {
            //don't actually push until we've finished loading.
            if (!mode.IsLoaded)
            {
                mode.Preload(game, d => Push((BackgroundMode)d));
                return true;
            }

            base.Push(mode);

            return true;
        }

        protected override void Update()
        {
            base.Update();
            Content.Scale = new Vector2(1 + (x_movement_amount / DrawSize.X) * 2);
        }

        protected override void OnEntering(GameMode last)
        {
            Content.FadeOut();
            Content.MoveToX(x_movement_amount);

            Content.FadeIn(transition_length, EasingTypes.InOutQuart);
            Content.MoveToX(0, transition_length, EasingTypes.InOutQuart);

            base.OnEntering(last);
        }

        protected override void OnSuspending(GameMode next)
        {
            Content.MoveToX(-x_movement_amount, transition_length, EasingTypes.InOutQuart);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(GameMode next)
        {
            Content.FadeOut(transition_length, EasingTypes.OutExpo);
            Content.MoveToX(x_movement_amount, transition_length, EasingTypes.OutExpo);

            return base.OnExiting(next);
        }

        protected override void OnResuming(GameMode last)
        {
            Content.MoveToX(0, transition_length, EasingTypes.OutExpo);
            base.OnResuming(last);
        }
    }
}
