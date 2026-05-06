// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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

        private Texture? sectionFailTexture;
        private Texture? sectionPassTexture;

        private readonly Sprite resultSprite;
        private readonly WarningArrows warningArrows;

        public required BreakTracker BreakTracker { get; init; }

        private readonly IBindable<Period?> currentPeriod = new Bindable<Period?>();

        public LegacyBreakOverlay(HealthProcessor healthProcessor)
        {
            this.healthProcessor = healthProcessor;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                resultSprite = new Sprite
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

            sectionFailTexture = skin.GetTexture(@"section-fail");
            sectionPassTexture = skin.GetTexture(@"section-pass");

            if (IsLoaded)
                updateDisplay(currentPeriod.Value);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentPeriod.BindTo(BreakTracker.CurrentPeriod);
            currentPeriod.BindValueChanged(period => updateDisplay(period.NewValue), true);
        }

        private void updateDisplay(Period? period)
        {
            Scheduler.CancelDelayedTasks();

            if (period == null)
                return;

            ISample? resultSample;
            Texture? resultTexture;

            if (healthProcessor.Health.Value >= 0.5)
            {
                resultSample = sectionPassSample;
                resultTexture = sectionPassTexture;
            }
            else
            {
                resultSample = sectionFailSample;
                resultTexture = sectionFailTexture;
            }

            var b = period.Value;

            resultSprite.Size = Vector2.Zero;
            resultSprite.Texture = resultTexture;

            // TODO Improve blinking behavior while rewinding
            using (BeginAbsoluteSequence(b.Start))
            {
                using (BeginDelayedSequence(b.Duration / 2))
                {
                    Schedule(() =>
                    {
                        if (IsPresent
                            && (Clock as IFrameStableClock)?.IsRewinding == false
                            && (Clock as IFrameStableClock)?.IsPaused.Value == false
                            && (Clock as IFrameStableClock)?.IsCatchingUp.Value == false)
                            resultSample?.Play();
                    });

                    resultSprite.FadeIn().Delay(100).FadeOut().Delay(100).Loop(0, 1)
                                .FadeIn().Delay(1000).FadeOut(200);
                }

                using (BeginDelayedSequence(b.Duration - 1300))
                {
                    warningArrows.FadeIn().Delay(100).FadeOut().Delay(100).Loop(0, 6);
                }

                using (BeginDelayedSequence(b.Duration))
                {
                    resultSprite.FadeOut();
                    warningArrows.FadeOut();
                }
            }
        }
    }
}
