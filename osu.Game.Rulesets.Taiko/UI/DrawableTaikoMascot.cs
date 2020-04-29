// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrawableTaikoMascot : BeatSyncedContainer
    {
        protected Bindable<TaikoMascotAnimationState> State { get; }

        private readonly Dictionary<TaikoMascotAnimationState, TaikoMascotTextureAnimation> animations;
        private Drawable currentAnimation;

        private bool lastHitMissed;
        private bool kiaiMode;

        public DrawableTaikoMascot(TaikoMascotAnimationState startingState = TaikoMascotAnimationState.Idle)
        {
            RelativeSizeAxes = Axes.Both;

            State = new Bindable<TaikoMascotAnimationState>(startingState);
            animations = new Dictionary<TaikoMascotAnimationState, TaikoMascotTextureAnimation>();
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChildren = new[]
            {
                animations[TaikoMascotAnimationState.Idle] = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Idle),
                animations[TaikoMascotAnimationState.Clear] = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Clear),
                animations[TaikoMascotAnimationState.Kiai] = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Kiai),
                animations[TaikoMascotAnimationState.Fail] = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Fail),
            };

            updateState();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            animations.Values.ForEach(animation => animation.Hide());
            State.BindValueChanged(mascotStateChanged, true);
        }

        public void OnNewResult(JudgementResult result)
        {
            lastHitMissed = result.Type == HitResult.Miss && result.Judgement.AffectsCombo;
            updateState();
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            kiaiMode = effectPoint.KiaiMode;
            updateState();
        }

        private void updateState()
        {
            State.Value = getNextState();
        }

        private TaikoMascotAnimationState getNextState()
        {
            if (lastHitMissed)
                return TaikoMascotAnimationState.Fail;

            return kiaiMode ? TaikoMascotAnimationState.Kiai : TaikoMascotAnimationState.Idle;
        }

        private void mascotStateChanged(ValueChangedEvent<TaikoMascotAnimationState> state)
        {
            currentAnimation?.Hide();
            currentAnimation = animations[state.NewValue];
            currentAnimation.Show();
        }
    }
}
