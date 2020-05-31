// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.UI
{
    public abstract class ReplayRecorder<T> : ReplayRecorder, IKeyBindingHandler<T>
        where T : struct
    {
        private readonly Replay target;

        private readonly List<T> pressedActions = new List<T>();

        private InputManager inputManager;

        public int RecordFrameRate = 60;

        protected ReplayRecorder(Replay target)
        {
            this.target = target;

            RelativeSizeAxes = Axes.Both;

            Depth = float.MinValue;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            recordFrame(false);
            return base.OnMouseMove(e);
        }

        public bool OnPressed(T action)
        {
            pressedActions.Add(action);
            recordFrame(true);
            return false;
        }

        public void OnReleased(T action)
        {
            pressedActions.Remove(action);
            recordFrame(true);
        }

        private void recordFrame(bool important)
        {
            var last = target.Frames.LastOrDefault();

            if (!important && last != null && Time.Current - last.Time < (1000d / RecordFrameRate))
                return;

            var position = ScreenSpaceToGamefield?.Invoke(inputManager.CurrentState.Mouse.Position) ?? inputManager.CurrentState.Mouse.Position;

            var frame = HandleFrame(position, pressedActions, last);

            if (frame != null)
                target.Frames.Add(frame);
        }

        protected abstract ReplayFrame HandleFrame(Vector2 mousePosition, List<T> actions, ReplayFrame previousFrame);
    }

    public abstract class ReplayRecorder : Component
    {
        public Func<Vector2, Vector2> ScreenSpaceToGamefield;
    }
}
