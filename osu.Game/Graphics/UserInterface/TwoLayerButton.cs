// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class TwoLayerButton : ClickableContainer
    {
        private TextAwesome icon;

        public Box IconLayer;
        public Box TextLayer;

        private const int transform_time = 600;
        private const int pulse_length = 250;

        private const float shear = 0.1f;

        public static readonly Vector2 SIZE_EXTENDED = new Vector2(140, 50);
        public static readonly Vector2 SIZE_RETRACTED = new Vector2(100, 50);
        public SampleChannel ActivationSound;
        private SpriteText text;

        public Color4 HoverColour;
        private Container c1;
        private Container c2;

        public Color4 BackgroundColour
        {
            set
            {
                TextLayer.Colour = value;
                IconLayer.Colour = value;
            }
        }

        public override Anchor Origin
        {
            get
            {
                return base.Origin;
            }

            set
            {
                base.Origin = value;
                c1.Origin = c1.Anchor = (value & Anchor.x2) > 0 ? Anchor.TopLeft : Anchor.TopRight;
                c2.Origin = c2.Anchor = (value & Anchor.x2) > 0 ? Anchor.TopRight : Anchor.TopLeft;

                X = (value & Anchor.x2) > 0 ? SIZE_RETRACTED.X * shear * 0.5f : 0;

                c1.Depth = (value & Anchor.x2) > 0 ? 0 : 1;
                c2.Depth = (value & Anchor.x2) > 0 ? 1 : 0;
            }
        }

        public TwoLayerButton()
        {
            Size = SIZE_RETRACTED;

            Children = new Drawable[]
            {
                c2 = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.4f,
                    Children = new Drawable[]
                    {
                        new Container {
                            RelativeSizeAxes = Axes.Both,
                            Shear = new Vector2(shear, 0),
                            Masking = true,
                            MaskingSmoothness = 2,
                            EdgeEffect = new EdgeEffect {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Offset = new Vector2(2, 0),
                                Radius = 2,
                            },
                            Children = new [] {
                                IconLayer = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    EdgeSmoothness = new Vector2(2, 0),
                                },
                            }
                        },
                        icon = new TextAwesome
                        {
                            Anchor = Anchor.Centre,
                            TextSize = 25,
                        },
                    }
                },
                c1 = new Container
                {
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.6f,
                    Children = new Drawable[]
                    {
                        new Container {
                            RelativeSizeAxes = Axes.Both,
                            Shear = new Vector2(shear, 0),
                            Masking = true,
                            MaskingSmoothness = 2,
                            EdgeEffect = new EdgeEffect {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Offset = new Vector2(2, 0),
                                Radius = 2,
                            },
                            Children = new [] {
                                TextLayer = new Box
                                {
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    EdgeSmoothness = new Vector2(2, 0),
                                },
                            }
                        },
                        text = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    }
                },
            };
        }

        public FontAwesome Icon
        {
            set
            {
                icon.Icon = value;
            }
        }

        public string Text
        {
            set
            {
                text.Text = value;
            }
        }

        public override bool Contains(Vector2 screenSpacePos) => IconLayer.Contains(screenSpacePos) || TextLayer.Contains(screenSpacePos);

        protected override bool OnHover(InputState state)
        {
            icon.ClearTransforms();

            ResizeTo(SIZE_EXTENDED, transform_time, EasingTypes.OutElastic);

            int duration = 0; //(int)(Game.Audio.BeatLength / 2);
            if (duration == 0) duration = pulse_length;

            IconLayer.FadeColour(HoverColour, transform_time, EasingTypes.OutElastic);

            double offset = 0; //(1 - Game.Audio.SyncBeatProgress) * duration;
            double startTime = Time.Current + offset;

            // basic pulse
            icon.Transforms.Add(new TransformScale
                {
                    StartValue = new Vector2(1.1f),
                    EndValue = Vector2.One,
                    StartTime = startTime,
                    EndTime = startTime + duration,
                    Easing = EasingTypes.Out,
                    LoopCount = -1,
                    LoopDelay = duration
                });

            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            icon.ClearTransforms();

            ResizeTo(SIZE_RETRACTED, transform_time, EasingTypes.OutElastic);

            IconLayer.FadeColour(TextLayer.Colour, transform_time, EasingTypes.OutElastic);

            int duration = 0; //(int)(Game.Audio.BeatLength);
            if (duration == 0) duration = pulse_length * 2;

            double offset = 0; //(1 - Game.Audio.SyncBeatProgress) * duration;
            double startTime = Time.Current + offset;

            // slow pulse
            icon.Transforms.Add(new TransformScale
                {
                    StartValue = new Vector2(1.1f),
                    EndValue = Vector2.One,
                    StartTime = startTime,
                    EndTime = startTime + duration,
                    Easing = EasingTypes.Out,
                    LoopCount = -1,
                    LoopDelay = duration
                });
        }

        protected override bool OnClick(InputState state)
        {
            var flash = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Shear = new Vector2(shear, 0),
                Colour = Color4.White.Opacity(0.5f),
            };
            Add(flash);

            flash.Alpha = 1;
            flash.FadeOut(500, EasingTypes.OutQuint);
            flash.Expire();

            ActivationSound.Play();

            return base.OnClick(state);
        }
    }
}