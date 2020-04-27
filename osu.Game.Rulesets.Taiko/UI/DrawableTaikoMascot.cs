// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
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

        public Bindable<TaikoMascotAnimationState> PlayfieldState;

        public TaikoMascotAnimationState State { get; private set; }

        public DrawableTaikoMascot(TaikoMascotAnimationState startingState = TaikoMascotAnimationState.Idle)
        {
            RelativeSizeAxes = Axes.Both;

            PlayfieldState = new Bindable<TaikoMascotAnimationState>();
            PlayfieldState.BindValueChanged(b =>
            {
                if (lastEffectControlPoint != null)
                    ShowState(GetFinalAnimationState(lastEffectControlPoint, b.NewValue));
            });

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
                    throw new ArgumentOutOfRangeException(nameof(state), $"There's no animation available for state {state}");
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

            var state = GetFinalAnimationState(lastEffectControlPoint = effectPoint, PlayfieldState.Value);
            ShowState(state);

            if (state == TaikoMascotAnimationState.Clear)
                return;

            var drawable = getStateDrawable(state);
            drawable.Move();
        }
    }
}
