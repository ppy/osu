// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public partial class LegacyCatchComboCounter : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        public Bindable<int> Current { get; } = new BindableInt { MinValue = 0 };

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public int DisplayedCount
        {
            get => displayedCount;
            private set
            {
                if (displayedCount.Equals(value))
                    return;

                displayedCountText.Text = formatCount(value);
                counterContainer.Size = displayedCountText.Size;

                displayedCount = value;
            }
        }

        private int displayedCount;

        private int previousValue;

        private const double main_duration = 300;
        private const double pop_out_duration = 400;
        private const double rolling_duration = 20;

        private Container counterContainer = null!;
        private LegacySpriteText popOutCountText = null!;
        private LegacySpriteText displayedCountText = null!;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        private IBindable<JudgementResult> lastJudgementResult = null!;

        public LegacyCatchComboCounter()
        {
            // This is required since we control the anchor/origin to move appropriately with the catcher.
            UsesFixedAnchor = true;
        }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                counterContainer = new Container
                {
                    AlwaysPresent = true,
                    Children = new[]
                    {
                        displayedCountText = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            AlwaysPresent = true,
                            BypassAutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        popOutCountText = new LegacySpriteText(LegacyFont.Combo)
                        {
                            Alpha = 0,
                            Blending = BlendingParameters.Additive,
                            BypassAutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                }
            };

            Current.BindTo(scoreProcessor.Combo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            displayedCountText.Text = formatCount(Current.Value);
            counterContainer.Size = displayedCountText.Size;

            lastJudgementResult = gameplayState.LastJudgementResult.GetBoundCopy();
            lastJudgementResult.BindValueChanged(result =>
            {
                if (!result.NewValue.Type.AffectsCombo() || !result.NewValue.HasResult)
                    return;

                if (!result.NewValue.IsHit)
                    return;

                if (result.NewValue?.HitObject is IHasComboInformation catchObject)
                    popOutCountText.Colour = catchObject.GetComboColour(skin);
            });

            Current.BindValueChanged(combo => updateCount(combo.NewValue == 0), true);
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

            // These are required in order for the combo to follow the catcher in a sane way.
            Anchor = (Anchor & ~(Anchor.x1 | Anchor.x2)) | Anchor.x0;
            Origin = (Origin & ~(Anchor.x0 | Anchor.x2)) | Anchor.x1;
        }

        private void updateCount(bool rolling)
        {
            int prev = previousValue;
            previousValue = Current.Value;

            if (!IsLoaded)
                return;

            if (!rolling)
            {
                FinishTransforms(false, nameof(DisplayedCount));

                if (prev + 1 == Current.Value)
                    onCountIncrement();
                else
                    onCountChange();
            }
            else
                onCountRolling();
        }

        private void onCountIncrement()
        {
            displayedCountText.ScaleTo(2)
                              .ScaleTo(1, main_duration, Easing.Out);

            displayedCountText.FadeInFromZero()
                              .Then()
                              .Delay(1000)
                              .FadeOut(main_duration);

            popOutCountText.Text = formatCount(Current.Value);

            popOutCountText.ScaleTo(2)
                           .ScaleTo(2.4f, pop_out_duration, Easing.Out);

            popOutCountText.FadeTo(0.7f)
                           .FadeOut(pop_out_duration);

            this.Delay(pop_out_duration - 140).TransformTo(nameof(DisplayedCount), Current.Value);
        }

        private void onCountRolling()
        {
            popOutCountText.FadeOut(100);
            displayedCountText.FadeOut(100);

            this.TransformTo(nameof(DisplayedCount), Current.Value, getProportionalDuration(DisplayedCount, Current.Value));
        }

        private void onCountChange()
        {
            if (Current.Value == 0)
            {
                popOutCountText.FadeOut();
                displayedCountText.FadeOut();
            }

            this.TransformTo(nameof(DisplayedCount), Current.Value);
        }

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * rolling_duration;
        }

        private string formatCount(int count) => count.ToString(CultureInfo.InvariantCulture);
    }
}
