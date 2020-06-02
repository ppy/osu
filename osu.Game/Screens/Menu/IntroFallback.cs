// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Menu
{
    public class IntroFallback : IntroScreen
    {
        protected override string BeatmapHash => "64E00D7022195959BFA3109D09C2E2276C8F12F486B91FCF6175583E973B48F2";
        protected override string BeatmapFile => "welcome.osz";
        private const double delay_step_two = 2142;

        private SampleChannel welcome;

        private SampleChannel pianoReverb;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Seeya = audio.Samples.Get(@"Intro/seeya-fallback");

            if (MenuVoice.Value)
            {
                welcome = audio.Samples.Get(@"Intro/welcome-fallback");
                pianoReverb = audio.Samples.Get(@"Intro/welcome_piano");
            }
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                welcome?.Play();
                pianoReverb?.Play();
                Scheduler.AddDelayed(delegate
                {
                    StartTrack();

                    PrepareMenuLoad();

                    logo.ScaleTo(1);
                    logo.FadeIn();

                    Scheduler.Add(LoadMenu);
                }, delay_step_two);

                LoadComponentAsync(new FallbackIntroSequence
                {
                    RelativeSizeAxes = Axes.Both
                }, t =>
                {
                    AddInternal(t);
                    t.Start(delay_step_two);
                });
            }
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(300);
            base.OnSuspending(next);
        }

        private class FallbackIntroSequence : Container
        {
            private OsuSpriteText welcomeText;

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    welcomeText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "welcome",
                        Padding = new MarginPadding { Bottom = 10 },
                        Font = OsuFont.GetFont(weight: FontWeight.Light, size: 42),
                        Alpha = 0,
                        Spacing = new Vector2(5),
                    },
                };
            }

            public void Start(double length)
            {
                if (Children.Any())
                {
                    // restart if we were already run previously.
                    FinishTransforms(true);
                    load();
                }

                double remainingTime() => length - TransformDelay;

                using (BeginDelayedSequence(250, true))
                {
                    welcomeText.FadeIn(700);
                    welcomeText.ScaleTo(welcomeText.Scale + new Vector2(0.5f), remainingTime(), Easing.Out);
                }
            }
        }
    }
}
