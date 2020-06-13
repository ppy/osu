// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public class IntroWelcome : IntroScreen
    {
        protected override string BeatmapHash => "64e00d7022195959bfa3109d09c2e2276c8f12f486b91fcf6175583e973b48f2";
        protected override string BeatmapFile => "welcome.osz";
        private const double delay_step_two = 2142;
        private SampleChannel welcome;
        private SampleChannel pianoReverb;
        protected override string SeeyaSampleName => "Intro/Welcome/seeya";

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            if (MenuVoice.Value)
                welcome = audio.Samples.Get(@"Intro/Welcome/welcome");

            pianoReverb = audio.Samples.Get(@"Intro/Welcome/welcome_piano");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                welcome?.Play();
                pianoReverb?.Play();

                Scheduler.AddDelayed(() =>
                {
                    StartTrack();
                    PrepareMenuLoad();

                    logo.ScaleTo(1);
                    logo.FadeIn();

                    Scheduler.Add(LoadMenu);
                }, delay_step_two);

                LoadComponentAsync(new WelcomeIntroSequence
                {
                    RelativeSizeAxes = Axes.Both
                }, AddInternal);
            }
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(300);
            base.OnSuspending(next);
        }

        private class WelcomeIntroSequence : Container
        {
            private Sprite welcomeText;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new LogoVisualisation
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Alpha = 0.5f,
                                        AccentColour = Color4.DarkBlue,
                                        Size = new Vector2(0.96f)
                                    },
                                    new Container
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            new Circle
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Size = new Vector2(480),
                                                Colour = Color4.Black
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    welcomeText = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(0.3f),
                        Width = 750,
                        Height = 78,
                        Alpha = 0,
                        Texture = textures.Get(@"Welcome/welcome_text")
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                using (BeginDelayedSequence(0, true))
                {
                    welcomeText.ResizeHeightTo(welcomeText.Height * 2, 500, Easing.In);
                    welcomeText.FadeIn(delay_step_two);
                    welcomeText.ScaleTo(welcomeText.Scale + new Vector2(0.1f), delay_step_two, Easing.Out).OnComplete(_ => Expire());
                }
            }
        }
    }
}
