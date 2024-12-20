// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Seasonal
{
    public partial class IntroChristmas : IntroScreen
    {
        // nekodex - circle the halls
        public const string CHRISTMAS_BEATMAP_SET_HASH = "7e26183e72a496f672c3a21292e6b469fdecd084d31c259ea10a31df5b46cd77";

        protected override string BeatmapHash => CHRISTMAS_BEATMAP_SET_HASH;

        protected override string BeatmapFile => "christmas2024.osz";

        private const double beat_length = 60000 / 172.0;
        private const double offset = 5924;

        protected override string SeeyaSampleName => "Intro/Welcome/seeya";

        private TrianglesIntroSequence intro = null!;

        public IntroChristmas(Func<MainMenu>? createNextScreen = null)
            : base(createNextScreen)
        {
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                PrepareMenuLoad();

                var decouplingClock = new DecouplingFramedClock(UsingThemedIntro ? Track : null);

                LoadComponentAsync(intro = new TrianglesIntroSequence(logo, () => FadeInBackground())
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = new InterpolatingFramedClock(decouplingClock),
                    LoadMenu = LoadMenu
                }, _ =>
                {
                    AddInternal(intro);

                    // There is a chance that the intro timed out before being displayed, and this scheduled callback could
                    // happen during the outro rather than intro.
                    // In such a scenario, we don't want to play the intro sample, nor attempt to start the intro track
                    // (that may have already been since disposed by MusicController).
                    if (DidLoadMenu)
                        return;

                    // If the user has requested no theme, fallback to the same intro voice and delay as IntroCircles.
                    // The triangles intro voice and theme are combined which makes it impossible to use.
                    StartTrack();

                    // no-op for the case of themed intro, no harm in calling for both scenarios as a safety measure.
                    decouplingClock.Start();
                });
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            // important as there is a clock attached to a track which will likely be disposed before returning to this screen.
            intro.Expire();
        }

        private partial class TrianglesIntroSequence : CompositeDrawable
        {
            private readonly OsuLogo logo;
            private readonly Action showBackgroundAction;
            private OsuSpriteText welcomeText = null!;

            private Container logoContainerSecondary = null!;
            private LazerLogo lazerLogo = null!;

            private Drawable triangles = null!;

            public Action LoadMenu = null!;

            [Resolved]
            private OsuGameBase game { get; set; } = null!;

            public TrianglesIntroSequence(OsuLogo logo, Action showBackgroundAction)
            {
                this.logo = logo;
                this.showBackgroundAction = showBackgroundAction;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new[]
                {
                    welcomeText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding { Bottom = 10 },
                        Font = OsuFont.GetFont(weight: FontWeight.Light, size: 42),
                        Alpha = 1,
                        Spacing = new Vector2(5),
                    },
                    logoContainerSecondary = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = lazerLogo = new LazerLogo
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    },
                    triangles = new CircularContainer
                    {
                        Alpha = 0,
                        Masking = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(960),
                        Child = new GlitchingTriangles
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                };
            }

            private static double getTimeForBeat(int beat) => offset + beat_length * beat;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                lazerLogo.Hide();

                using (BeginAbsoluteSequence(0))
                {
                    using (BeginDelayedSequence(getTimeForBeat(-16)))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome to osu!");

                    using (BeginDelayedSequence(getTimeForBeat(-15)))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "");

                    using (BeginDelayedSequence(getTimeForBeat(-14)))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome to osu!");

                    using (BeginDelayedSequence(getTimeForBeat(-13)))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "");

                    using (BeginDelayedSequence(getTimeForBeat(-12)))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "merry christmas!");

                    using (BeginDelayedSequence(getTimeForBeat(-11)))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "");

                    using (BeginDelayedSequence(getTimeForBeat(-10)))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "merry osumas!");

                    using (BeginDelayedSequence(getTimeForBeat(-9)))
                    {
                        welcomeText.FadeIn().OnComplete(t => t.Text = "");
                    }

                    lazerLogo.Scale = new Vector2(0.2f);
                    triangles.Scale = new Vector2(0.2f);

                    for (int i = 0; i < 8; i++)
                    {
                        using (BeginDelayedSequence(getTimeForBeat(-8 + i)))
                        {
                            triangles.FadeIn();

                            lazerLogo.ScaleTo(new Vector2(0.2f + (i + 1) / 8f * 0.3f), beat_length * 1, Easing.OutQuint);
                            triangles.ScaleTo(new Vector2(0.2f + (i + 1) / 8f * 0.3f), beat_length * 1, Easing.OutQuint);
                            lazerLogo.FadeTo((i + 1) * 0.06f);
                            lazerLogo.TransformTo(nameof(LazerLogo.Progress), (i + 1) / 10f);
                        }
                    }

                    GameWideFlash flash = new GameWideFlash();

                    using (BeginDelayedSequence(getTimeForBeat(-2)))
                    {
                        lazerLogo.FadeIn().OnComplete(_ => game.Add(flash));
                    }

                    flash.FadeInCompleted = () =>
                    {
                        logoContainerSecondary.Remove(lazerLogo, true);
                        triangles.FadeOut();
                        logo.FadeIn();
                        showBackgroundAction();
                        LoadMenu();
                    };
                }
            }

            private partial class GameWideFlash : Box
            {
                public Action? FadeInCompleted;

                public GameWideFlash()
                {
                    Colour = Color4.White;
                    RelativeSizeAxes = Axes.Both;
                    Blending = BlendingParameters.Additive;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    Alpha = 0;

                    this.FadeTo(0.5f, beat_length * 2, Easing.In)
                        .OnComplete(_ => FadeInCompleted?.Invoke());

                    this.Delay(beat_length * 2)
                        .Then()
                        .FadeOutFromOne(3000, Easing.OutQuint);
                }
            }

            private partial class LazerLogo : CompositeDrawable
            {
                private LogoAnimation highlight = null!;
                private LogoAnimation background = null!;

                public float Progress
                {
                    get => background.AnimationProgress;
                    set
                    {
                        background.AnimationProgress = value;
                        highlight.AnimationProgress = value;
                    }
                }

                public LazerLogo()
                {
                    Size = new Vector2(960);
                }

                [BackgroundDependencyLoader]
                private void load(LargeTextureStore textures)
                {
                    InternalChildren = new Drawable[]
                    {
                        highlight = new LogoAnimation
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Triangles/logo-highlight"),
                            Colour = Color4.White,
                        },
                        background = new LogoAnimation
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Triangles/logo-background"),
                            Colour = OsuColour.Gray(0.6f),
                        },
                    };
                }
            }

            private partial class GlitchingTriangles : BeatSyncedContainer
            {
                private int beatsHandled;

                protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
                {
                    base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                    Divisor = beatsHandled < 4 ? 1 : 4;

                    for (int i = 0; i < (beatsHandled + 1); i++)
                    {
                        float angle = (float)(RNG.NextDouble() * 2 * Math.PI);
                        float randomRadius = (float)(Math.Sqrt(RNG.NextDouble()));

                        float x = 0.5f + 0.5f * randomRadius * (float)Math.Cos(angle);
                        float y = 0.5f + 0.5f * randomRadius * (float)Math.Sin(angle);

                        Color4 christmasColour = RNG.NextBool() ? SeasonalUIConfig.PRIMARY_COLOUR_1 : SeasonalUIConfig.PRIMARY_COLOUR_2;

                        Drawable triangle = new Triangle
                        {
                            Size = new Vector2(RNG.NextSingle() + 1.2f) * 80,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(x, y),
                            Colour = christmasColour
                        };

                        if (beatsHandled >= 10)
                            triangle.Blending = BlendingParameters.Additive;

                        AddInternal(triangle);
                        triangle
                            .ScaleTo(0.9f)
                            .ScaleTo(1, beat_length / 2, Easing.Out);
                        triangle.FadeInFromZero(100, Easing.OutQuint);
                    }

                    beatsHandled += 1;
                }
            }
        }
    }
}
