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
        protected override string BeatmapHash => "64E00D7022195959BFA3109D09C2E2276C8F12F486B91FCF6175583E973B48F2";
        protected override string BeatmapFile => "welcome.osz";
        private const double delay_step_two = 2142;
        private SampleChannel welcome;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Seeya = audio.Samples.Get(@"Intro/welcome/seeya");

            if (MenuVoice.Value)
                welcome = audio.Samples.Get(@"Intro/welcome/welcome");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                welcome?.Play();
                StartTrack();
                Scheduler.AddDelayed(delegate
                {
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
            private LogoVisualisation visualizer;
            private Container elementContainer;
            private Container circleContainer;
            private Circle blackCircle;

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
                            elementContainer = new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    visualizer = new LogoVisualisation
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Alpha = 0.5f,
                                        AccentColour = Color4.Blue,
                                        Size = new Vector2(0.96f)
                                    },
                                    circleContainer = new Container
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            blackCircle = new Circle
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
                        Scale = new Vector2(0.5f),
                        Texture = textures.Get(@"Welcome/welcome_text@2x")
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                double remainingTime() => delay_step_two - TransformDelay;

                using (BeginDelayedSequence(250, true))
                {
                    welcomeText.FadeIn(700);
                    welcomeText.ScaleTo(welcomeText.Scale + new Vector2(0.5f), remainingTime(), Easing.Out).OnComplete(_ =>
                    {
                        elementContainer.Remove(visualizer);
                        circleContainer.Remove(blackCircle);
                        elementContainer.Remove(circleContainer);
                        Remove(welcomeText);
                        visualizer.Dispose();
                        blackCircle.Dispose();
                        welcomeText.Dispose();
                    });
                }
            }
        }
    }
}
