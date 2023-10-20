// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonSpinner : CompositeDrawable
    {
        private DrawableSpinner drawableSpinner = null!;

        private OsuSpriteText bonusCounter = null!;

        private Container spmContainer = null!;
        private OsuSpriteText spmCounter = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            drawableSpinner = (DrawableSpinner)drawableHitObject;

            InternalChildren = new Drawable[]
            {
                new ArgonSpinnerDisc
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
                    Font = OsuFont.Default.With(size: 28, weight: FontWeight.Bold),
                    Y = -100,
                },
                spmContainer = new Container
                {
                    Alpha = 0f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = 60,
                    Children = new[]
                    {
                        spmCounter = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = @"0",
                            Font = OsuFont.Default.With(size: 28, weight: FontWeight.SemiBold)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = @"SPINS PER MINUTE",
                            Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                            Y = 30
                        }
                    }
                }
            };
        }

        private IBindable<int> completedSpins = null!;
        private IBindable<double> spinsPerMinute = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            completedSpins = drawableSpinner.CompletedFullSpins.GetBoundCopy();
            completedSpins.BindValueChanged(bonus =>
            {
                bonusCounter.Text = drawableSpinner.CurrentBonusScore.ToString(NumberFormatInfo.InvariantInfo);
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSpinner.IsNotNull())
                drawableSpinner.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
