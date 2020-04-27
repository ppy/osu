// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrawableTaikoMascot : BeatSyncedContainer
    {
        private TaikoMascotTextureAnimation idleDrawable, clearDrawable, kiaiDrawable, failDrawable;
        private EffectControlPoint lastEffectControlPoint;
        private TaikoMascotAnimationState playfieldState;

        public TaikoMascotAnimationState State { get; private set; }

        public DrawableTaikoMascot(TaikoMascotAnimationState startingState = TaikoMascotAnimationState.Idle)
        {
            RelativeSizeAxes = Axes.Both;

            State = startingState;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChildren = new[]
            {
                idleDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Idle),
                clearDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Clear),
                kiaiDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Kiai),
                failDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Fail),
            };

            ShowState(State);
        }

        public void ShowState(TaikoMascotAnimationState state)
        {
            foreach (var child in InternalChildren)
                child.Hide();

            State = state;

            var drawable = getStateDrawable(State);
            drawable.Show();
        }

        /// <summary>
        /// Sets the playfield state used for determining the final state.
        /// </summary>
        /// <remarks>
        /// If you're looking to change the state manually, please look at <see cref="ShowState"/>.
        /// </remarks>
        public void SetPlayfieldState(TaikoMascotAnimationState state)
        {
            playfieldState = state;

            if (lastEffectControlPoint != null)
                ShowState(GetFinalAnimationState(lastEffectControlPoint, playfieldState));
        }

        private TaikoMascotTextureAnimation getStateDrawable(TaikoMascotAnimationState state)
        {
            switch (state)
            {
                case TaikoMascotAnimationState.Idle:
                    return idleDrawable;

                case TaikoMascotAnimationState.Clear:
                    return clearDrawable;

                case TaikoMascotAnimationState.Kiai:
                    return kiaiDrawable;

                case TaikoMascotAnimationState.Fail:
                    return failDrawable;

                default:
                    throw new ArgumentException($"There's no case for animation state ${state} available", nameof(state));
            }
        }

        protected virtual TaikoMascotAnimationState GetFinalAnimationState(EffectControlPoint effectPoint, TaikoMascotAnimationState playfieldState)
        {
            if (playfieldState == TaikoMascotAnimationState.Fail)
                return playfieldState;

            return effectPoint.KiaiMode ? TaikoMascotAnimationState.Kiai : TaikoMascotAnimationState.Idle;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            var state = GetFinalAnimationState(lastEffectControlPoint = effectPoint, playfieldState);
            ShowState(state);

            if (state == TaikoMascotAnimationState.Clear)
                return;

            var drawable = getStateDrawable(state);
            drawable.Move();
        }
    }
}
