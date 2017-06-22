﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Audio.Track;
using System;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    public class TwoLayerButton : ClickableContainer
    {
        private readonly BouncingIcon bouncingIcon;

        public Box IconLayer;
        public Box TextLayer;

        private const int transform_time = 600;
        private const int pulse_length = 250;

        private const float shear = 0.1f;

        public static readonly Vector2 SIZE_EXTENDED = new Vector2(140, 50);
        public static readonly Vector2 SIZE_RETRACTED = new Vector2(100, 50);
        public SampleChannel ActivationSound;
        private readonly SpriteText text;

        public Color4 HoverColour;
        private readonly Container c1;
        private readonly Container c2;

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
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Shear = new Vector2(shear, 0),
                            Masking = true,
                            MaskingSmoothness = 2,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Offset = new Vector2(2, 0),
                                Radius = 2,
                            },
                            Children = new[]
                            {
                                IconLayer = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    EdgeSmoothness = new Vector2(2, 0),
                                },
                            }
                        },
                        bouncingIcon = new BouncingIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
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
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Shear = new Vector2(shear, 0),
                            Masking = true,
                            MaskingSmoothness = 2,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Offset = new Vector2(2, 0),
                                Radius = 2,
                            },
                            Children = new[]
                            {
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
                bouncingIcon.Icon = value;
            }
        }

        public string Text
        {
            set
            {
                text.Text = value;
            }
        }

        protected override bool InternalContains(Vector2 screenSpacePos) => IconLayer.Contains(screenSpacePos) || TextLayer.Contains(screenSpacePos);

        protected override bool OnHover(InputState state)
        {
            ResizeTo(SIZE_EXTENDED, transform_time, EasingTypes.OutElastic);
            IconLayer.FadeColour(HoverColour, transform_time, EasingTypes.OutElastic);

            bouncingIcon.ScaleTo(1.1f, transform_time, EasingTypes.OutElastic);

            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            ResizeTo(SIZE_RETRACTED, transform_time, EasingTypes.OutElastic);
            IconLayer.FadeColour(TextLayer.Colour, transform_time, EasingTypes.OutElastic);

            bouncingIcon.ScaleTo(1, transform_time, EasingTypes.OutElastic);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            return true;
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

        private class BouncingIcon : BeatSyncedContainer
        {
            private const double beat_in_time = 60;

            private readonly TextAwesome icon;

            public FontAwesome Icon { set { icon.Icon = value; } }

            public BouncingIcon()
            {
                EarlyActivationMilliseconds = beat_in_time;
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    icon = new TextAwesome
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TextSize = 25
                    }
                };
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                var beatLength = timingPoint.BeatLength;

                float amplitudeAdjust = Math.Min(1, 0.4f + amplitudes.Maximum);

                if (beatIndex < 0) return;

                icon.ScaleTo(1 - 0.1f * amplitudeAdjust, beat_in_time, EasingTypes.Out);
                using (icon.BeginDelayedSequence(beat_in_time))
                    icon.ScaleTo(1, beatLength * 2, EasingTypes.OutQuint);
            }
        }
    }
}