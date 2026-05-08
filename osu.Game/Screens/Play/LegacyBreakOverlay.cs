// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.Break;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class LegacyBreakOverlay : SkinReloadableDrawable
    {
        private readonly HealthProcessor healthProcessor;

        private ISample? sectionFailSample;
        private ISample? sectionPassSample;

        private readonly Sprite sectionFailSprite;
        private readonly Sprite sectionPassSprite;
        private readonly WarningArrows warningArrows;

        public required BreakTracker BreakTracker { get; init; }

        private readonly IBindable<Period?> currentPeriod = new Bindable<Period?>();

        public LegacyBreakOverlay(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                sectionFailSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                },
                sectionPassSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                },
                warningArrows = new WarningArrows
                {
                    Alpha = 0,
                },
            };
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            sectionFailSample = skin.GetSample(new SampleInfo(@"Gameplay/sectionfail"));
            sectionPassSample = skin.GetSample(new SampleInfo(@"Gameplay/sectionpass"));

            sectionFailSprite.Size = Vector2.Zero;
            sectionFailSprite.Texture = skin.GetTexture(@"section-fail");

            sectionPassSprite.Size = Vector2.Zero;
            sectionPassSprite.Texture = skin.GetTexture(@"section-pass");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentPeriod.BindTo(BreakTracker.CurrentPeriod);
            currentPeriod.BindValueChanged(period => updateDisplay(period.NewValue), true);
        }

        protected override void Update()
        {
            base.Update();

            var b = currentPeriod.Value;

            if (b == null)
            {
                sectionPassSprite.Alpha = 0;
                sectionFailSprite.Alpha = 0;
                warningArrows.Alpha = 0;
                return;
            }

            double t = Time.Current;
            double s = b.Value.Start;
            double d = b.Value.Duration;
            double e = b.Value.End;
            double h = s + d / 2;

            Sprite resultSprite = healthProcessor.Health.Value >= 0.5 ? sectionPassSprite : sectionFailSprite;

            resultSprite.Alpha = (t > h && t < h + 100) || (t > h + 200 && t < h + 300) || (t > h + 400 && t < h + 1400) ? 1 : 0;

            if (t > h + 1400 && t < h + 1600)
            {
                resultSprite.Alpha = Interpolation.ValueAt(t, 1f, 0f, h + 1400, h + 1600);
            }

            warningArrows.Alpha = t > e - 1300 && t < e && (int)(e - t) / 100 % 2 == 0 ? 1 : 0;
        }

        private void updateDisplay(Period? period)
        {
            Scheduler.CancelDelayedTasks();

            if (period == null)
                return;

            ISample? resultSample = healthProcessor.Health.Value >= 0.5 ? sectionPassSample : sectionFailSample;
            var b = period.Value;

            using (BeginAbsoluteSequence(b.Start + b.Duration / 2))
            {
                Schedule(() =>
                {
                    var clock = (IFrameStableClock)Clock;
                    if (IsPresent && !clock.IsRewinding && !clock.IsPaused.Value && !clock.IsCatchingUp.Value)
                        resultSample?.Play();
                });
            }
        }
    }
}
