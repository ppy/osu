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
        private const int pass_blink_duration = 100;
        private const int pass_last_blink_duration = 1000;
        private const int pass_last_fade_out_duration = 200;

        private const int arrows_blink_duration = 100;
        private const int arrows_blink_times = 7;

        private readonly HealthProcessor healthProcessor;

        private ISample? sectionFailSample;
        private ISample? sectionPassSample;

        private readonly Sprite sectionFailSprite;
        private readonly Sprite sectionPassSprite;
        private readonly LegacyBreakArrows legacyBreakArrows;

        public required BreakTracker BreakTracker { get; init; }

        private readonly IBindable<Period?> currentPeriod = new Bindable<Period?>();
        private Period? lastBreak;

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
                legacyBreakArrows = new LegacyBreakArrows
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
            currentPeriod.BindValueChanged(updateDisplay, true);
        }

        protected override void Update()
        {
            base.Update();

            if (lastBreak == null)
            {
                sectionPassSprite.Alpha = 0;
                sectionFailSprite.Alpha = 0;
                legacyBreakArrows.Alpha = 0;
                return;
            }

            double t = Time.Current;
            double s = lastBreak.Value.Start;
            double e = lastBreak.Value.End + BreakOverlay.BREAK_FADE_DURATION;
            double h = s + lastBreak.Value.Duration / 2;

            if (t < s || t > e)
            {
                lastBreak = null;
                return;
            }

            Sprite resultSprite = healthProcessor.Health.Value >= 0.5 ? sectionPassSprite : sectionFailSprite;

            double vs = h;
            resultSprite.Alpha = t > vs && t < vs + pass_blink_duration ? 1 : 0;

            vs += pass_blink_duration * 2;
            resultSprite.Alpha = t > vs && t < vs + pass_blink_duration ? 1 : resultSprite.Alpha;

            vs += pass_blink_duration * 2;
            resultSprite.Alpha = t > vs && t < vs + pass_last_blink_duration ? 1 : resultSprite.Alpha;

            vs += pass_last_blink_duration;

            if (t > vs && t < vs + pass_last_fade_out_duration)
                resultSprite.Alpha = Interpolation.ValueAt(t, 1f, 0f, vs, vs + pass_last_fade_out_duration);

            vs = e - arrows_blink_times * arrows_blink_duration * 2 - arrows_blink_duration;
            legacyBreakArrows.Alpha = t > vs && t < e && (int)(e - t) / arrows_blink_duration % 2 == 0 ? 1 : 0;
        }

        private void updateDisplay(ValueChangedEvent<Period?> period)
        {
            Scheduler.CancelDelayedTasks();

            if (period.NewValue == null)
                return;

            lastBreak = period.NewValue;

            ISample? resultSample = healthProcessor.Health.Value >= 0.5 ? sectionPassSample : sectionFailSample;
            var b = period.NewValue.Value;

            using (BeginAbsoluteSequence(b.Start + b.Duration / 2))
            {
                Schedule(() =>
                {
                    bool isRewinding = (Clock as IGameplayClock)?.IsRewinding ?? false;
                    bool isPaused = (Clock as IGameplayClock)?.IsPaused.Value ?? false;
                    bool isCatchingUp = (Clock as IFrameStableClock)?.IsCatchingUp.Value ?? false;
                    if (IsPresent && !isRewinding && !isPaused && !isCatchingUp)
                        resultSample?.Play();
                });
            }
        }
    }
}
