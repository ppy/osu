// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    /// <summary>
    /// A combo counter implementation that visually behaves almost similar to stable's osu!catch combo counter.
    /// </summary>
    public partial class LegacyCatchComboCounter : LegacyComboCounter
    {
        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        private IBindable<JudgementResult> lastJudgementResult = null!;

        public LegacyCatchComboCounter()
        {
            PopOutCountText.Origin = PopOutCountText.Anchor = Anchor.Centre;
            DisplayedCountText.Origin = DisplayedCountText.Anchor = Anchor.Centre;

            Content.ChangeChildDepth(PopOutCountText, float.MinValue);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            lastJudgementResult = gameplayState.LastJudgementResult.GetBoundCopy();
            lastJudgementResult.BindValueChanged(result =>
            {
                if (!result.NewValue.Type.AffectsCombo() || !result.NewValue.HasResult)
                    return;

                if (!result.NewValue.IsHit)
                    return;

                if (result.NewValue?.HitObject is IHasComboInformation catchObject)
                    PopOutCountText.Colour = catchObject.GetComboColour(skin);
            });

            FinishTransforms(true);
        }

        protected override void Update()
        {
            base.Update();

            if (drawableRuleset != null)
            {
                var catcher = ((CatchPlayfield)drawableRuleset.Playfield).Catcher;
                X = Parent!.ToLocalSpace(catcher.ScreenSpaceDrawQuad.Centre).X;
            }
        }

        private ScheduledDelegate? scheduledPopOut;

        protected override void OnCountIncrement()
        {
            const double main_duration = 300;
            const double pop_out_duration = 400;

            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            DisplayedCountText.ScaleTo(2)
                              .ScaleTo(1, main_duration, Easing.Out);

            DisplayedCountText.FadeInFromZero()
                              .Then()
                              .Delay(1000)
                              .FadeOut(main_duration);

            PopOutCountText.Text = FormatCount(Current.Value);

            PopOutCountText.ScaleTo(2)
                           .ScaleTo(2.4f, pop_out_duration, Easing.Out);

            PopOutCountText.FadeTo(0.7f)
                           .FadeOut(pop_out_duration);

            this.Delay(main_duration - 140).Schedule(() =>
            {
                base.OnCountIncrement();
            }, out scheduledPopOut);
        }

        protected override void OnCountRolling()
        {
            base.OnCountRolling();

            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            if (Current.Value == 0)
            {
                PopOutCountText.FadeOut(100);
                DisplayedCountText.FadeOut(100);
            }
        }

        protected override void OnCountChange()
        {
            base.OnCountChange();

            scheduledPopOut?.Cancel();
            scheduledPopOut = null;
        }
    }
}
