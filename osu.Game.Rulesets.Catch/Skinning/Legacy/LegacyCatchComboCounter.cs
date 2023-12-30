// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    /// <summary>
    /// A combo counter implementation that visually behaves almost similar to stable's osu!catch combo counter.
    /// </summary>
    // todo: maybe make this inherit LegacyComboCounter at some point
    public partial class LegacyCatchComboCounter : CompositeDrawable, ISerialisableDrawable
    {
        private readonly LegacyRollingCounter counter;

        private readonly LegacyRollingCounter explosion;

        public bool UsesFixedAnchor { get; set; }

        private int lastDisplayedCombo;

        bool ISerialisableDrawable.SupportsClosestAnchor => false;

        public LegacyCatchComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            AlwaysPresent = true;
            Alpha = 0f;

            InternalChild = new UprightAspectMaintainingContainer
            {
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    explosion = new LegacyRollingCounter(LegacyFont.Combo)
                    {
                        Alpha = 0.65f,
                        Blending = BlendingParameters.Additive,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(1.5f),
                    },
                    counter = new LegacyRollingCounter(LegacyFont.Combo)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                }
            };

            UsesFixedAnchor = true;
        }

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        private IBindable<JudgementResult> lastJudgementResult = null!;
        private IBindable<int> combo = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            combo = gameplayState.ScoreProcessor.Combo.GetBoundCopy();
            lastJudgementResult = gameplayState.LastJudgementResult.GetBoundCopy();
            lastJudgementResult.BindValueChanged(onNewJudgement, true);
        }

        protected override void Update()
        {
            base.Update();

            if (drawableRuleset != null)
            {
                var catchPlayfield = (CatchPlayfield)drawableRuleset.Playfield;
                X = Parent.AsNonNull().ToLocalSpace(catchPlayfield.Catcher.ScreenSpaceDrawQuad.Centre).X;
            }
        }

        private void onNewJudgement(ValueChangedEvent<JudgementResult> judgement)
        {
            if (combo.Value == lastDisplayedCombo)
                return;

            // There may still be existing transforms to the counter (including value change after 250ms),
            // finish them immediately before new transforms.
            counter.SetCountWithoutRolling(lastDisplayedCombo);

            lastDisplayedCombo = combo.Value;

            if ((Clock as IGameplayClock)?.IsRewinding == true)
            {
                // needs more work to make rewind somehow look good.
                // basically we want the previous increment to play... or turning off RemoveCompletedTransforms (not feasible from a performance angle).
                Hide();
                return;
            }

            // Combo fell to zero, roll down and fade out the counter.
            if (combo.Value == 0)
            {
                counter.Current.Value = 0;
                explosion.Current.Value = 0;

                this.FadeOut(400, Easing.Out);
            }
            else
            {
                this.FadeInFromZero().Then().Delay(1000).FadeOut(300);

                counter.ScaleTo(1.5f)
                       .ScaleTo(0.8f, 250, Easing.Out)
                       .OnComplete(c => c.SetCountWithoutRolling(combo.Value));

                counter.Delay(250)
                       .ScaleTo(1f)
                       .ScaleTo(1.1f, 60).Then().ScaleTo(1f, 30);

                var catchObject = (CatchHitObject)judgement.NewValue.HitObject;
                explosion.Colour = ((IHasComboInformation)catchObject).GetComboColour(skin);

                explosion.SetCountWithoutRolling(combo.Value);
                explosion.ScaleTo(1.5f)
                         .ScaleTo(1.9f, 400, Easing.Out)
                         .FadeOutFromOne(400);
            }
        }
    }
}
