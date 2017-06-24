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

namespace osu.Game.Overlays
{
    public class MedalOverlay : FocusedOverlayContainer
    {
        public const float DISC_SIZE = 400;

        private const float border_width = 5;

        private readonly Box background;
        private readonly FillFlowContainer backgroundStrip;
        private readonly BackgroundStrip leftStrip, rightStrip;
        private readonly CircularContainer disc;
        private readonly Sprite innerSpin, outterSpin;
        private readonly DrawableMedal drawableMedal;

        private SampleChannel getSample;

        protected override bool OnClick(InputState state)
        {
            dismiss();
            return true;
        }

        protected override void OnFocusLost(InputState state)
        {
            if (state.Keyboard.Keys.Contains(Key.Escape)) dismiss();
        }

        public MedalOverlay(Medal medal)
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0f;

            Children = new Drawable[]
            {
                background = new Box
                {
                    Name = @"dim",
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(60),
                },
                outterSpin = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(DISC_SIZE + 500),
                    Alpha = 0f,
                },
                backgroundStrip = new FillFlowContainer
                {
                    Name = @"background strip",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0f,
                    Height = border_width,
                    Alpha = 0f,
                    Direction = FillDirection.Horizontal,
                    Children = new[]
                    {
                        leftStrip = new BackgroundStrip(0f, 1f),
                        rightStrip = new BackgroundStrip(1f, 0f),
                    },
                },
                disc = new CircularContainer
                {
                    Name = @"content",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0f,
                    Masking = true,
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
                        drawableMedal = new DrawableMedal(medal)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures, AudioManager audio)
        {
            getSample = audio.Sample.Get(@"MedalSplash/medal-get");
            innerSpin.Texture = outterSpin.Texture = textures.Get(@"MedalSplash/disc-spin");

            disc.EdgeEffect = leftStrip.EdgeEffect = rightStrip.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = colours.Blue.Opacity(0.5f),
                Radius = 50,
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            FadeIn(200);
            background.FlashColour(Color4.White.Opacity(0.25f), 400);

            double duration1 = 400;
            double duration2 = 900;
            double duration3 = 900;
            double duration4 = 1000;

            getSample.Play();
            Delay(200, true);

            innerSpin.Transforms.Add(new TransformRotation
            {
                StartValue = 0,
                EndValue = 359,
                StartTime = Clock.TimeInfo.Current,
                EndTime = Clock.TimeInfo.Current + 20000,
                LoopCount = -1,
            });

            outterSpin.Transforms.Add(new TransformRotation
            {
                StartValue = 0,
                EndValue = 359,
                StartTime = Clock.TimeInfo.Current,
                EndTime = Clock.TimeInfo.Current + 40000,
                LoopCount = -1,
            });

            disc.FadeIn(duration1);
            backgroundStrip.FadeIn(duration1);
            backgroundStrip.ResizeWidthTo(1f, duration1 * 2, EasingTypes.OutQuint);
            outterSpin.FadeTo(0.1f, duration1 * 2);
            disc.ScaleTo(1f, duration1 * 2, EasingTypes.OutElastic);

            Delay(duration1 + 200, true);
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
        }

        private class BackgroundStrip : Container
        {
            public BackgroundStrip(float start, float end)
            {
                RelativeSizeAxes = Axes.Both;
                Width = 0.5f;
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
    }
}
