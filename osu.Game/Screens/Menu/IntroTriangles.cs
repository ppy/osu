// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public class IntroTriangles : IntroScreen
    {
        protected override string BeatmapHash => "a1556d0801b3a6b175dda32ef546f0ec812b400499f575c44fccbe9c67f9b1e5";

        protected override string BeatmapFile => "triangles.osz";

        protected override BackgroundScreen CreateBackground() => background = new BackgroundScreenDefault(false)
        {
            Alpha = 0,
        };

        [Resolved]
        private AudioManager audio { get; set; }

        private BackgroundScreenDefault background;

        private Sample welcome;

        private DecoupleableInterpolatingFramedClock decoupledClock;
        private TrianglesIntroSequence intro;

        public IntroTriangles([CanBeNull] Func<MainMenu> createNextScreen = null)
            : base(createNextScreen)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (MenuVoice.Value)
                welcome = audio.Samples.Get(@"Intro/welcome");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                PrepareMenuLoad();

                decoupledClock = new DecoupleableInterpolatingFramedClock
                {
                    IsCoupled = false
                };

                if (UsingThemedIntro)
                    decoupledClock.ChangeSource(Track);

                LoadComponentAsync(intro = new TrianglesIntroSequence(logo, background)
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = decoupledClock,
                    LoadMenu = LoadMenu
                }, t =>
                {
                    AddInternal(t);
                    if (!UsingThemedIntro)
                        welcome?.Play();

                    StartTrack();
                });
            }
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            // ensure the background is shown, even if the TriangleIntroSequence failed to do so.
            background.ApplyToBackground(b => b.Show());

            // important as there is a clock attached to a track which will likely be disposed before returning to this screen.
            intro.Expire();
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            background.FadeOut(100);
        }

        protected override void StartTrack()
        {
            decoupledClock.Start();
        }

        private class TrianglesIntroSequence : CompositeDrawable
        {
            private readonly OsuLogo logo;
            private readonly BackgroundScreenDefault background;
            private OsuSpriteText welcomeText;

            private RulesetFlow rulesets;
            private Container rulesetsScale;
            private Container logoContainerSecondary;
            private LazerLogo lazerLogo;

            private GlitchingTriangles triangles;

            public Action LoadMenu;

            public TrianglesIntroSequence(OsuLogo logo, BackgroundScreenDefault background)
            {
                this.logo = logo;
                this.background = background;
            }

            [Resolved]
            private OsuGameBase game { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    triangles = new GlitchingTriangles
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(0.4f, 0.16f)
                    },
                    welcomeText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding { Bottom = 10 },
                        Font = OsuFont.GetFont(weight: FontWeight.Light, size: 42),
                        Alpha = 1,
                        Spacing = new Vector2(5),
                    },
                    rulesetsScale = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            rulesets = new RulesetFlow()
                        }
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
                };
            }

            private const double text_1 = 200;
            private const double text_2 = 400;
            private const double text_3 = 700;
            private const double text_4 = 900;
            private const double text_glitch = 1060;

            private const double rulesets_1 = 1450;
            private const double rulesets_2 = 1650;
            private const double rulesets_3 = 1850;

            private const double logo_scale_duration = 920;
            private const double logo_1 = 2080;
            private const double logo_2 = logo_1 + logo_scale_duration;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                const float scale_start = 1.2f;
                const float scale_adjust = 0.8f;

                rulesets.Hide();
                lazerLogo.Hide();
                background.ApplyToBackground(b => b.Hide());

                using (BeginAbsoluteSequence(0))
                {
                    using (BeginDelayedSequence(text_1))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "wel");

                    using (BeginDelayedSequence(text_2))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome");

                    using (BeginDelayedSequence(text_3))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome to");

                    using (BeginDelayedSequence(text_4))
                    {
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome to osu!");
                        welcomeText.TransformTo(nameof(welcomeText.Spacing), new Vector2(50, 0), 5000);
                    }

                    using (BeginDelayedSequence(text_glitch))
                        triangles.FadeIn();

                    using (BeginDelayedSequence(rulesets_1))
                    {
                        rulesetsScale.ScaleTo(0.8f, 1000);
                        rulesets.FadeIn().ScaleTo(1).TransformSpacingTo(new Vector2(200, 0));
                        welcomeText.FadeOut();
                        triangles.FadeOut();
                    }

                    using (BeginDelayedSequence(rulesets_2))
                    {
                        rulesets.ScaleTo(2).TransformSpacingTo(new Vector2(30, 0));
                    }

                    using (BeginDelayedSequence(rulesets_3))
                    {
                        rulesets.ScaleTo(4).TransformSpacingTo(new Vector2(10, 0));
                        rulesetsScale.ScaleTo(1.3f, 1000);
                    }

                    using (BeginDelayedSequence(logo_1))
                    {
                        rulesets.FadeOut();

                        // matching flyte curve y = 0.25x^2 + (max(0, x - 0.7) / 0.3) ^ 5
                        lazerLogo.FadeIn().ScaleTo(scale_start).Then().Delay(logo_scale_duration * 0.7f).ScaleTo(scale_start - scale_adjust, logo_scale_duration * 0.3f, Easing.InQuint);

                        lazerLogo.TransformTo(nameof(LazerLogo.Progress), 1f, logo_scale_duration);

                        logoContainerSecondary.ScaleTo(scale_start).Then().ScaleTo(scale_start - scale_adjust * 0.25f, logo_scale_duration, Easing.InQuad);
                    }

                    using (BeginDelayedSequence(logo_2))
                    {
                        lazerLogo.FadeOut().OnComplete(_ =>
                        {
                            logoContainerSecondary.Remove(lazerLogo);
                            lazerLogo.Dispose(); // explicit disposal as we are pushing a new screen and the expire may not get run.

                            logo.FadeIn();

                            background.ApplyToBackground(b => b.Show());

                            game.Add(new GameWideFlash());

                            LoadMenu();
                        });
                    }
                }
            }

            private class GameWideFlash : Box
            {
                private const double flash_length = 1000;

                public GameWideFlash()
                {
                    Colour = Color4.White;
                    RelativeSizeAxes = Axes.Both;
                    Blending = BlendingParameters.Additive;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    this.FadeOutFromOne(flash_length, Easing.Out);
                }
            }

            private class LazerLogo : CompositeDrawable
            {
                private LogoAnimation highlight, background;

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
                private void load(TextureStore textures)
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

            private class RulesetFlow : FillFlowContainer
            {
                [BackgroundDependencyLoader]
                private void load(RulesetStore rulesets)
                {
                    var modes = new List<Drawable>();

                    foreach (var ruleset in rulesets.AvailableRulesets)
                    {
                        var icon = new ConstrainedIconContainer
                        {
                            Icon = ruleset.CreateInstance().CreateIcon(),
                            Size = new Vector2(30),
                        };

                        modes.Add(icon);
                    }

                    AutoSizeAxes = Axes.Both;
                    Children = modes;

                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;
                }
            }

            private class GlitchingTriangles : CompositeDrawable
            {
                public GlitchingTriangles()
                {
                    RelativeSizeAxes = Axes.Both;
                }

                private double? lastGenTime;

                private const double time_between_triangles = 22;

                protected override void Update()
                {
                    base.Update();

                    if (lastGenTime == null || Time.Current - lastGenTime > time_between_triangles)
                    {
                        lastGenTime = (lastGenTime ?? Time.Current) + time_between_triangles;

                        Drawable triangle = new OutlineTriangle(RNG.NextBool(), (RNG.NextSingle() + 0.2f) * 80)
                        {
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                        };

                        AddInternal(triangle);

                        triangle.FadeOutFromOne(120);
                    }
                }

                /// <summary>
                /// Represents a sprite that is drawn in a triangle shape, instead of a rectangle shape.
                /// </summary>
                public class OutlineTriangle : BufferedContainer
                {
                    public OutlineTriangle(bool outlineOnly, float size)
                        : base(cachedFrameBuffer: true)
                    {
                        Size = new Vector2(size);

                        InternalChildren = new Drawable[]
                        {
                            new Triangle { RelativeSizeAxes = Axes.Both },
                        };

                        if (outlineOnly)
                        {
                            AddInternal(new Triangle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Colour = Color4.Black,
                                Size = new Vector2(size - 5),
                                Blending = BlendingParameters.None,
                            });
                        }

                        Blending = BlendingParameters.Additive;
                    }
                }
            }
        }
    }
}
