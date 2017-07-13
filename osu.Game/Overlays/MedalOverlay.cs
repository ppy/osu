// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Overlays.MedalSplash;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using OpenTK.Input;
using System.Linq;
using osu.Framework.Graphics.Shapes;
using System;
using osu.Framework.MathUtils;

namespace osu.Game.Overlays
{
    public class MedalOverlay : FocusedOverlayContainer
    {
        public const float DISC_SIZE = 400;

        private const float border_width = 5;

        private readonly Medal medal;
        private readonly Box background;
        private readonly Container backgroundStrip, particleContainer;
        private readonly BackgroundStrip leftStrip, rightStrip;
        private readonly CircularContainer disc;
        private readonly Sprite innerSpin, outerSpin;
        private DrawableMedal drawableMedal;

        private SampleChannel getSample;

        public MedalOverlay(Medal medal)
        {
            this.medal = medal;
            RelativeSizeAxes = Axes.Both;
            Alpha = 0f;
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(60),
                },
                outerSpin = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(DISC_SIZE + 500),
                    Alpha = 0f,
                },
                backgroundStrip = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = border_width,
                    Alpha = 0f,
                    Children = new[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreRight,
                            Width = 0.5f,
                            Padding = new MarginPadding { Right = DISC_SIZE / 2 },
                            Children = new[]
                            {
                                leftStrip = new BackgroundStrip(0f, 1f)
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreLeft,
                            Width = 0.5f,
                            Padding = new MarginPadding { Left = DISC_SIZE / 2 },
                            Children = new[]
                            {
                                rightStrip = new BackgroundStrip(1f, 0f),
                            },
                        },
                    },
                },
                particleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0f,
                },
                disc = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0f,
                    Masking = true,
                    AlwaysPresent = true,
                    BorderColour = Color4.White,
                    BorderThickness = border_width,
                    Size = new Vector2(DISC_SIZE),
                    Scale = new Vector2(0.8f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"05262f"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            TriangleScale = 2,
                            ColourDark = OsuColour.FromHex(@"04222b"),
                            ColourLight = OsuColour.FromHex(@"052933"),
                        },
                        innerSpin = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1.05f),
                            Alpha = 0.25f,
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures, AudioManager audio)
        {
            getSample = audio.Sample.Get(@"MedalSplash/medal-get");
            innerSpin.Texture = outerSpin.Texture = textures.Get(@"MedalSplash/disc-spin");

            disc.EdgeEffect = leftStrip.EdgeEffect = rightStrip.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = colours.Blue.Opacity(0.5f),
                Radius = 50,
            };

            LoadComponentAsync(drawableMedal = new DrawableMedal(medal)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
            }, m =>
            {
                disc.Add(m);
                Show();
            });
        }

        protected override void Update()
        {
            base.Update();

            particleContainer.Add(new MedalParticle(RNG.Next(0, 359)));
        }

        protected override bool OnClick(InputState state)
        {
            dismiss();
            return true;
        }

        protected override void OnFocusLost(InputState state)
        {
            if (state.Keyboard.Keys.Contains(Key.Escape)) dismiss();
        }

        private const double duration1 = 400;
        private const double duration2 = 900;
        private const double duration3 = 900;
        private const double duration4 = 1000;
        protected override void PopIn()
        {
            base.PopIn();

            FadeIn(200);
            background.FlashColour(Color4.White.Opacity(0.25f), 400);

            getSample.Play();
            Delay(200, true);

            var innerRotate = new TransformRotation
            {
                EndValue = 359,
                StartTime = Clock.TimeInfo.Current,
                EndTime = Clock.TimeInfo.Current + 20000,
            };

            innerRotate.Loop(0);
            innerSpin.Transforms.Add(innerRotate);

            var outerRotate = new TransformRotation
            {
                EndValue = 359,
                StartTime = Clock.TimeInfo.Current,
                EndTime = Clock.TimeInfo.Current + 40000,
            };

            outerRotate.Loop(0);
            outerSpin.Transforms.Add(outerRotate);

            disc.FadeIn(duration1);
            particleContainer.FadeIn(duration1);
            outerSpin.FadeTo(0.1f, duration1 * 2);
            disc.ScaleTo(1f, duration1 * 2, EasingTypes.OutElastic);

            Delay(duration1 + 200, true);
            backgroundStrip.FadeIn(duration2);
            leftStrip.ResizeWidthTo(1f, duration2, EasingTypes.OutQuint);
            rightStrip.ResizeWidthTo(1f, duration2, EasingTypes.OutQuint);
            drawableMedal.ChangeState(DrawableMedal.DisplayState.Icon, duration2);

            Delay(duration2, true);
            drawableMedal.ChangeState(DrawableMedal.DisplayState.MedalUnlocked, duration3);

            Delay(duration3, true);
            drawableMedal.ChangeState(DrawableMedal.DisplayState.Full, duration4);
        }

        protected override void PopOut()
        {
            base.PopOut();

            FadeOut(200);
        }

        private void dismiss()
        {
            if (drawableMedal.Transforms.Count != 0) return;
            Hide();
            Expire();
        }

        private class BackgroundStrip : Container
        {
            public BackgroundStrip(float start, float end)
            {
                RelativeSizeAxes = Axes.Both;
                Width = 0f;
                ColourInfo = ColourInfo.GradientHorizontal(Color4.White.Opacity(start), Color4.White.Opacity(end));
                Masking = true;

                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    }
                };
            }
        }

        private class MedalParticle : CircularContainer
        {
            private readonly float direction;

            private Vector2 positionForOffset(float offset) => new Vector2((float)(offset * Math.Sin(direction)), (float)(offset * Math.Cos(direction)));

            public MedalParticle(float direction)
            {
                this.direction = direction;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Position = positionForOffset(DISC_SIZE / 2);
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = colours.Blue.Opacity(0.5f),
                    Radius = 5,
                };

                MoveTo(positionForOffset(DISC_SIZE / 2 + 200), 500);
                FadeOut(500);
                Expire();
            }
        }
    }
}
