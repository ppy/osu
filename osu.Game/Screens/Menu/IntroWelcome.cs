// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Online.API;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class IntroWelcome : IntroScreen
    {
        protected override string BeatmapHash => "64e00d7022195959bfa3109d09c2e2276c8f12f486b91fcf6175583e973b48f2";
        protected override string BeatmapFile => "welcome.osz";
        private const double delay_step_two = 2142;

        private SkinnableSamples skinnableWelcome;
        private ISample welcome;

        private ISample pianoReverb;
        protected override string SeeyaSampleName => "Intro/Welcome/seeya";

        public IntroWelcome([CanBeNull] Func<MainMenu> createNextScreen = null)
            : base(createNextScreen)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, IAPIProvider api)
        {
            if (MenuVoice.Value)
            {
                if (api.LocalUser.Value.IsSupporter)
                    AddInternal(skinnableWelcome = new SkinnableSamples(new SampleInfo(@"Intro/Welcome/welcome")));
                else
                    welcome = audio.Samples.Get(@"Intro/Welcome/welcome");
            }

            pianoReverb = audio.Samples.Get(@"Intro/Welcome/welcome_piano");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                Track.Looping = true;

                LoadComponentAsync(new WelcomeIntroSequence
                {
                    RelativeSizeAxes = Axes.Both
                }, intro =>
                {
                    PrepareMenuLoad();

                    AddInternal(intro);

                    if (skinnableWelcome != null)
                        skinnableWelcome.Play();
                    else
                        welcome?.Play();

                    var reverbChannel = pianoReverb?.Play();
                    if (reverbChannel != null)
                        intro.LogoVisualisation.AddAmplitudeSource(reverbChannel);

                    if (!UsingThemedIntro)
                        StartTrack();

                    Scheduler.AddDelayed(() =>
                    {
                        if (UsingThemedIntro)
                        {
                            StartTrack();
                            // this classic intro loops forever.
                            Track.Looping = true;
                        }

                        const float fade_in_time = 200;

                        logo.ScaleTo(1);
                        logo.FadeIn(fade_in_time);

                        FadeInBackground(fade_in_time);

                        LoadMenu();
                    }, delay_step_two);
                });
            }
        }

        private partial class WelcomeIntroSequence : Container
        {
            private Drawable welcomeText;
            private Container scaleContainer;

            public LogoVisualisation LogoVisualisation { get; private set; }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures, IAPIProvider api)
            {
                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;

                Children = new Drawable[]
                {
                    scaleContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            LogoVisualisation = new LogoVisualisation
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = Color4.DarkBlue,
                                Size = OsuLogo.SCALE_ADJUST,
                            },
                            new Circle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(480),
                                Colour = Color4.Black
                            },
                        }
                    },
                };

                if (api.LocalUser.Value.IsSupporter)
                    scaleContainer.Add(welcomeText = new SkinnableSprite(@"Intro/Welcome/welcome_text"));
                else
                    scaleContainer.Add(welcomeText = new Sprite { Texture = textures.Get(@"Intro/Welcome/welcome_text") });

                welcomeText.Anchor = Anchor.Centre;
                welcomeText.Origin = Anchor.Centre;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                using (BeginDelayedSequence(0))
                {
                    scaleContainer.ScaleTo(0.9f).ScaleTo(1, delay_step_two).OnComplete(_ => Expire());
                    scaleContainer.FadeInFromZero(1800);

                    welcomeText.ScaleTo(new Vector2(1, 0)).ScaleTo(Vector2.One, 400, Easing.Out);
                }
            }
        }
    }
}
