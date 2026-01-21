// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.UI
{
    public abstract partial class ReplayRecorder<T> : ReplayRecorder, IKeyBindingHandler<T>
        where T : struct
    {
        private readonly Score target;

        private readonly List<T> pressedActions = new List<T>();

        private InputManager inputManager;

        /// <summary>
        /// The frame rate to record replays at.
        /// </summary>
        public int RecordFrameRate { get; set; } = 60;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        protected ReplayRecorder(Score target)
        {
            this.target = target;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        protected override void Update()
        {
            base.Update();
            RecordFrame(false);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            RecordFrame(false);
            return base.OnMouseMove(e);
        }

        public bool OnPressed(KeyBindingPressEvent<T> e)
        {
            pressedActions.Add(e.Action);
            RecordFrame(true);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<T> e)
        {
            pressedActions.Remove(e.Action);
            RecordFrame(true);
        }

        public override void RecordFrame(bool important)
        {
            var last = target.Replay.Frames.LastOrDefault();

            if (!important && last != null && Time.Current - last.Time < (1000d / RecordFrameRate) * Clock.Rate)
                return;

            var position = ScreenSpaceToGamefield?.Invoke(inputManager.CurrentState.Mouse.Position) ?? inputManager.CurrentState.Mouse.Position;

            var frame = HandleFrame(position, pressedActions, last);

            if (frame != null)
            {
                // this reduces redundancy of frames in the resulting replay.
                if (last?.IsEquivalentTo(frame) == true)
                    target.Replay.Frames[^1] = frame;
                else
                    target.Replay.Frames.Add(frame);

                // the above de-duplication is done at `FrameDataBundle` level in `SpectatorClient`.
                // it's not 100% matching because of the possibility of duplicated frames crossing a bundle boundary, but it's close and simple enough.
                spectatorClient?.HandleFrame(frame);
            }
        }

        protected abstract ReplayFrame HandleFrame(Vector2 mousePosition, List<T> actions, ReplayFrame previousFrame);
    }

    public abstract partial class ReplayRecorder : Component
    {
        public Func<Vector2, Vector2> ScreenSpaceToGamefield;

        public abstract void RecordFrame(bool important);
    }
}
