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
            completedSpins.BindValueChanged(_ =>
            {
                if (drawableSpinner.CurrentBonusScore <= 0)
                    return;

                if (drawableSpinner.CurrentBonusScore == drawableSpinner.MaximumBonusScore)
                {
                    bonusCounter.Text = "MAX";
                    bonusCounter.ScaleTo(1.5f).Then().ScaleTo(2.8f, 1000, Easing.OutQuint);

                    bonusCounter.FlashColour(Colour4.FromHex("FC618F"), 400);
                    bonusCounter.FadeOutFromOne(500);
                }
                else
                {
                    bonusCounter.Text = drawableSpinner.CurrentBonusScore.ToString(NumberFormatInfo.InvariantInfo);
                    bonusCounter.ScaleTo(1.5f).Then().ScaleTo(1f, 1000, Easing.OutQuint);
                    bonusCounter.FadeOutFromOne(1500);
                }
            });

            spinsPerMinute = drawableSpinner.SpinsPerMinute.GetBoundCopy();
            spinsPerMinute.BindValueChanged(spm =>
            {
                spmCounter.Text = Math.Truncate(spm.NewValue).ToString(@"#0");
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            updateSpmAlpha();
        }

        private void updateSpmAlpha()
        {
            if (drawableSpinner.Result?.TimeStarted is double startTime)
                spmContainer.Alpha = (float)Math.Clamp((Clock.CurrentTime - startTime) / drawableSpinner.HitObject.TimeFadeIn, 0, 1);
            else
                spmContainer.Alpha = 0;
        }
    }
}
