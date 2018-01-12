﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Graphics.Containers
{
    public class OsuFocusedOverlayContainer : FocusedOverlayContainer
    {
        private SampleChannel samplePopIn;
        private SampleChannel samplePopOut;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            samplePopIn = audio.Sample.Get(@"UI/overlay-pop-in");
            samplePopOut = audio.Sample.Get(@"UI/overlay-pop-out");

            StateChanged += onStateChanged;
        }

        /// <summary>
        /// Whether mouse input should be blocked screen-wide while this overlay is visible.
        /// Performing mouse actions outside of the valid extents will hide the overlay but pass the events through.
        /// </summary>
        public virtual bool BlockScreenWideMouse => BlockPassThroughMouse;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => BlockScreenWideMouse || base.ReceiveMouseInputAt(screenSpacePos);

        protected override bool OnClick(InputState state)
        {
            if (!base.ReceiveMouseInputAt(state.Mouse.NativeState.Position))
            {
                State = Visibility.Hidden;
                return true;
            }

            return base.OnClick(state);
        }

        protected override bool OnDragStart(InputState state)
        {
            if (!base.ReceiveMouseInputAt(state.Mouse.NativeState.Position))
            {
                State = Visibility.Hidden;
                return true;
            }

            return base.OnDragStart(state);
        }

        protected override bool OnDrag(InputState state) => State == Visibility.Hidden;

        private void onStateChanged(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Visible:
                    samplePopIn?.Play();
                    break;
                case Visibility.Hidden:
                    samplePopOut?.Play();
                    break;
            }
        }
    }
}
