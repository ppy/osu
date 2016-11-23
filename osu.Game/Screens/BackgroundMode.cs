//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using System.Threading;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Screens
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

        private GameMode pendingLoad;

        public override bool Push(GameMode mode)
        {
            Debug.Assert(pendingLoad == null);

            // When trying to push a non-loaded GameMode, load it asynchronously and re-invoke Push
            // once it's done.
            if (mode.LoadState == LoadState.NotLoaded)
            {
                pendingLoad = mode;
                mode.Preload(Game, d => Push((BackgroundMode)d));
                return true;
            }

            // Make sure the in-progress loading is complete before pushing the GameMode.
            while (mode.LoadState < LoadState.Loaded && !mode.HasExited)
                Thread.Sleep(1);

            if (mode.HasExited)
                return false;

            pendingLoad = null;
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
            if (base.OnExiting(next)) return true;

            Content.FadeOut(transition_length, EasingTypes.OutExpo);
            Content.MoveToX(x_movement_amount, transition_length, EasingTypes.OutExpo);

            stopPendingLoad();
            return false;
        }

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            stopPendingLoad();
        }

        private void stopPendingLoad()
        {
            pendingLoad?.Exit();
            pendingLoad = null;
        }

        protected override void OnResuming(GameMode last)
        {
            Content.MoveToX(0, transition_length, EasingTypes.OutExpo);
            base.OnResuming(last);
        }
    }
}
