// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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

            disc.Add(drawableMedal = new DrawableMedal(medal)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Show();
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

        private const double initial_duration = 400;
        private const double step_duration = 900;

        protected override void PopIn()
        {
            base.PopIn();

            this.FadeIn(200);
            background.FlashColour(Color4.White.Opacity(0.25f), 400);

            getSample.Play();

            innerSpin.Spin(20000, RotationDirection.Clockwise);
            outerSpin.Spin(40000, RotationDirection.Clockwise);

            using (BeginDelayedSequence(200, true))
            {
                disc.FadeIn(initial_duration)
                    .ScaleTo(1f, initial_duration * 2, Easing.OutElastic);

                particleContainer.FadeIn(initial_duration);
                outerSpin.FadeTo(0.1f, initial_duration * 2);

                using (BeginDelayedSequence(initial_duration + 200, true))
                {
                    backgroundStrip.FadeIn(step_duration);
                    leftStrip.ResizeWidthTo(1f, step_duration, Easing.OutQuint);
                    rightStrip.ResizeWidthTo(1f, step_duration, Easing.OutQuint);

                    this.Animate().Schedule(() =>
                    {
                        if (drawableMedal.State != DisplayState.Full)
                            drawableMedal.State = DisplayState.Icon;
                    })
                    .Delay(step_duration).Schedule(() =>
                    {
                        if (drawableMedal.State != DisplayState.Full)
                            drawableMedal.State = DisplayState.MedalUnlocked;
                    })
                    .Delay(step_duration).Schedule(() =>
                    {
                        if (drawableMedal.State != DisplayState.Full)
                            drawableMedal.State = DisplayState.Full;
                    });
                }
            }
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(200);
        }

        private void dismiss()
        {
            if (drawableMedal.State != DisplayState.Full)
            {
                // if we haven't yet, play out the animation fully
                drawableMedal.State = DisplayState.Full;
                FinishTransforms(true);
                return;
            }

            Hide();
            Expire();
        }

        private class BackgroundStrip : Container
        {
            public BackgroundStrip(float start, float end)
            {
                RelativeSizeAxes = Axes.Both;
                Width = 0f;
                Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(start), Color4.White.Opacity(end));
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

                this.MoveTo(positionForOffset(DISC_SIZE / 2 + 200), 500);
                this.FadeOut(500);
                Expire();
            }
        }
    }
}
