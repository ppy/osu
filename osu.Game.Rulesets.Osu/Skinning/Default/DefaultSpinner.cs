// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class DefaultSpinner : CompositeDrawable
    {
        private DrawableSpinner drawableSpinner;

        private OsuSpriteText bonusCounter;

        private Container spmContainer;
        private OsuSpriteText spmCounter;

        public DefaultSpinner()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            drawableSpinner = (DrawableSpinner)drawableHitObject;

            AddRangeInternal(new Drawable[]
            {
                new DefaultSpinnerDisc
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                bonusCounter = new OsuSpriteText
                {
                    Alpha = 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Numeric.With(size: 24),
                    Y = -120,
                },
                spmContainer = new Container
                {
                    Alpha = 0f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = 120,
                    Children = new[]
                    {
                        spmCounter = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = @"0",
                            Font = OsuFont.Numeric.With(size: 24)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = @"SPINS PER MINUTE",
                            Font = OsuFont.Numeric.With(size: 12),
                            Y = 30
                        }
                    }
                }
            });
        }

        private IBindable<double> gainedBonus;
        private IBindable<double> spinsPerMinute;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gainedBonus = drawableSpinner.GainedBonus.GetBoundCopy();
            gainedBonus.BindValueChanged(bonus =>
            {
                bonusCounter.Text = bonus.NewValue.ToString(NumberFormatInfo.InvariantInfo);
                bonusCounter.FadeOutFromOne(1500);
                bonusCounter.ScaleTo(1.5f).Then().ScaleTo(1f, 1000, Easing.OutQuint);
            });

            spinsPerMinute = drawableSpinner.SpinsPerMinute.GetBoundCopy();
            spinsPerMinute.BindValueChanged(spm =>
            {
                spmCounter.Text = Math.Truncate(spm.NewValue).ToString(@"#0");
            }, true);

            drawableSpinner.ApplyCustomUpdateState += updateStateTransforms;
            updateStateTransforms(drawableSpinner, drawableSpinner.State.Value);
        }

        protected override void Update()
        {
            base.Update();

            if (!spmContainer.IsPresent && drawableSpinner.Result?.TimeStarted != null)
                fadeCounterOnTimeStart();
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            if (!(drawableHitObject is DrawableSpinner))
                return;

            fadeCounterOnTimeStart();
        }

        private void fadeCounterOnTimeStart()
        {
            if (drawableSpinner.Result?.TimeStarted is double startTime)
            {
                using (BeginAbsoluteSequence(startTime))
                    spmContainer.FadeIn(drawableSpinner.HitObject.TimeFadeIn);
            }
        }
    }
}
