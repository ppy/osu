// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class DrawableTaikoMascot : BeatSyncedContainer
    {
        public readonly Bindable<TaikoMascotAnimationState> State;
        public readonly Bindable<Judgement?> LastResult;

        private readonly Dictionary<TaikoMascotAnimationState, TaikoMascotAnimation> animations;

        private TaikoMascotAnimation? currentAnimation;

        private bool lastObjectHit = true;
        private bool kiaiMode;

        public DrawableTaikoMascot(TaikoMascotAnimationState startingState = TaikoMascotAnimationState.Idle)
        {
            Origin = Anchor = Anchor.BottomLeft;

            State = new Bindable<TaikoMascotAnimationState>(startingState);
            LastResult = new Bindable<Judgement?>();

            animations = new Dictionary<TaikoMascotAnimationState, TaikoMascotAnimation>();
        }

        [BackgroundDependencyLoader(true)]
        private void load(GameplayState? gameplayState)
        {
            InternalChildren = new[]
            {
                animations[TaikoMascotAnimationState.Idle] = new TaikoMascotAnimation(TaikoMascotAnimationState.Idle),
                animations[TaikoMascotAnimationState.Clear] = new TaikoMascotAnimation(TaikoMascotAnimationState.Clear),
                animations[TaikoMascotAnimationState.Kiai] = new TaikoMascotAnimation(TaikoMascotAnimationState.Kiai),
                animations[TaikoMascotAnimationState.Fail] = new TaikoMascotAnimation(TaikoMascotAnimationState.Fail),
            };

            if (gameplayState != null)
                ((IBindable<Judgement>)LastResult).BindTo(gameplayState.LastJudgementResult);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            animations.Values.ForEach(animation => animation.Hide());

            State.BindValueChanged(mascotStateChanged, true);
            LastResult.BindValueChanged(onNewResult);
        }

        private void onNewResult(ValueChangedEvent<Judgement?> resultChangedEvent)
        {
            var result = resultChangedEvent.NewValue;
            if (result == null)
                return;

            // TODO: missing support for clear/fail state transition at end of beatmap gameplay

            if (triggerComboClear(result) || triggerSwellClear(result))
            {
                State.Value = TaikoMascotAnimationState.Clear;
                // always consider a clear equivalent to a hit to avoid clear -> miss transitions
                lastObjectHit = true;
            }

            if (!result.Type.AffectsCombo())
                return;

            lastObjectHit = result.IsHit;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            kiaiMode = effectPoint.KiaiMode;
        }

        protected override void Update()
        {
            base.Update();
            State.Value = getNextState();
        }

        private TaikoMascotAnimationState getNextState()
        {
            // don't change state if current animation is still playing (and we haven't rewound before it).
            // used for clear state - others are manually animated on new beats.
            if (currentAnimation?.Completed == false && currentAnimation.DisplayTime <= Time.Current)
                return State.Value;

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

        private bool triggerComboClear(Judgement judgement)
            => (judgement.ComboAtJudgement + 1) % 50 == 0 && judgement.Type.AffectsCombo() && judgement.IsHit;

        private bool triggerSwellClear(Judgement judgement)
            => judgement.JudgementCriteria is TaikoSwellJudgementCriteria && judgement.IsHit;
    }
}
