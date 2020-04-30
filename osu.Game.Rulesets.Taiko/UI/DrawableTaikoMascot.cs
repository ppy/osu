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
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrawableTaikoMascot : BeatSyncedContainer
    {
        public IBindable<TaikoMascotAnimationState> State => state;

        private readonly Bindable<TaikoMascotAnimationState> state;
        private readonly Dictionary<TaikoMascotAnimationState, TaikoMascotAnimation> animations;
        private TaikoMascotAnimation currentAnimation;

        private bool lastObjectHit = true;
        private bool kiaiMode;

        public DrawableTaikoMascot(TaikoMascotAnimationState startingState = TaikoMascotAnimationState.Idle)
        {
            Origin = Anchor = Anchor.BottomLeft;

            state = new Bindable<TaikoMascotAnimationState>(startingState);
            animations = new Dictionary<TaikoMascotAnimationState, TaikoMascotAnimation>();
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChildren = new[]
            {
                animations[TaikoMascotAnimationState.Idle] = new TaikoMascotAnimation(TaikoMascotAnimationState.Idle),
                animations[TaikoMascotAnimationState.Clear] = new TaikoMascotAnimation(TaikoMascotAnimationState.Clear),
                animations[TaikoMascotAnimationState.Kiai] = new TaikoMascotAnimation(TaikoMascotAnimationState.Kiai),
                animations[TaikoMascotAnimationState.Fail] = new TaikoMascotAnimation(TaikoMascotAnimationState.Fail),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            animations.Values.ForEach(animation => animation.Hide());
            state.BindValueChanged(mascotStateChanged, true);
        }

        public void OnNewResult(JudgementResult result)
        {
            // TODO: missing support for clear/fail state transition at end of beatmap gameplay

            if (triggerComboClear(result) || triggerSwellClear(result))
            {
                state.Value = TaikoMascotAnimationState.Clear;
                // always consider a clear equivalent to a hit to avoid clear -> miss transitions
                lastObjectHit = true;
            }

            if (!result.Judgement.AffectsCombo)
                return;

            lastObjectHit = result.IsHit;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            kiaiMode = effectPoint.KiaiMode;
        }

        protected override void Update()
        {
            base.Update();
            state.Value = getNextState();
        }

        private TaikoMascotAnimationState getNextState()
        {
            // don't change state if current animation is playing
            // (used for clear state - others are manually animated on new beats)
            if (currentAnimation != null && !currentAnimation.Completed)
                return state.Value;

            if (!lastObjectHit)
                return TaikoMascotAnimationState.Fail;

            return kiaiMode ? TaikoMascotAnimationState.Kiai : TaikoMascotAnimationState.Idle;
        }

        private void mascotStateChanged(ValueChangedEvent<TaikoMascotAnimationState> state)
        {
            currentAnimation?.Hide();
            currentAnimation = animations[state.NewValue];
            currentAnimation.Show();
        }

        private bool triggerComboClear(JudgementResult judgementResult)
            => (judgementResult.ComboAtJudgement + 1) % 50 == 0 && judgementResult.Judgement.AffectsCombo && judgementResult.IsHit;

        private bool triggerSwellClear(JudgementResult judgementResult)
            => judgementResult.Judgement is TaikoSwellJudgement && judgementResult.IsHit;
    }
}
