// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Audio.Track;
using System;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Screens.Select;

namespace osu.Game.Graphics.UserInterface
{
    public partial class TwoLayerButton : OsuClickableContainer
    {
        private readonly BouncingIcon bouncingIcon;

        public Box IconLayer;
        public Box TextLayer;

        private const int transform_time = 600;
        private const int pulse_length = 250;

        private const float shear_width = 5f;

        private static readonly Vector2 shear = new Vector2(shear_width / Footer.HEIGHT, 0);

        public static readonly Vector2 SIZE_EXTENDED = new Vector2(140, 50);
        public static readonly Vector2 SIZE_RETRACTED = new Vector2(100, 50);
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
            get => base.Origin;
            set
            {
                base.Origin = value;
                c1.Origin = c1.Anchor = value.HasFlagFast(Anchor.x2) ? Anchor.TopLeft : Anchor.TopRight;
                c2.Origin = c2.Anchor = value.HasFlagFast(Anchor.x2) ? Anchor.TopRight : Anchor.TopLeft;

                X = value.HasFlagFast(Anchor.x2) ? SIZE_RETRACTED.X * shear.X * 0.5f : 0;

                Remove(c1, false);
                Remove(c2, false);
                c1.Depth = value.HasFlagFast(Anchor.x2) ? 0 : 1;
                c2.Depth = value.HasFlagFast(Anchor.x2) ? 1 : 0;
                Add(c1);
                Add(c2);
            }
        }

        public TwoLayerButton(HoverSampleSet sampleSet = HoverSampleSet.Default)
            : base(sampleSet)
        {
            Size = SIZE_RETRACTED;
            Shear = shear;

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
                            Shear = -shear,
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
                            Shear = -shear,
                        }
                    }
                },
            };
        }

        public IconUsage Icon
        {
            set => bouncingIcon.Icon = value;
        }

        public string Text
        {
            set => text.Text = value;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => IconLayer.ReceivePositionalInputAt(screenSpacePos) || TextLayer.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnHover(HoverEvent e)
        {
            this.ResizeTo(SIZE_EXTENDED, transform_time, Easing.OutElastic);

            IconLayer.FadeColour(HoverColour, transform_time / 2f, Easing.OutQuint);

            bouncingIcon.ScaleTo(1.1f, transform_time, Easing.OutElastic);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.ResizeTo(SIZE_RETRACTED, transform_time, Easing.Out);
            IconLayer.FadeColour(TextLayer.Colour, transform_time, Easing.Out);

            bouncingIcon.ScaleTo(1, transform_time, Easing.Out);
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnClick(ClickEvent e)
        {
            var flash = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White.Opacity(0.5f),
            };
            Add(flash);

            flash.Alpha = 1;
            flash.FadeOut(500, Easing.OutQuint);
            flash.Expire();

            return base.OnClick(e);
        }

        private partial class BouncingIcon : BeatSyncedContainer
        {
            private const double beat_in_time = 60;

            private readonly SpriteIcon icon;

            public IconUsage Icon
            {
                set => icon.Icon = value;
            }

            public BouncingIcon()
            {
                EarlyActivationMilliseconds = beat_in_time;
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(25),
                    }
                };
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                double beatLength = timingPoint.BeatLength;

                float amplitudeAdjust = Math.Min(1, 0.4f + amplitudes.Maximum);

                if (beatIndex < 0) return;

                icon.ScaleTo(1 - 0.1f * amplitudeAdjust, beat_in_time, Easing.Out)
                    .Then()
                    .ScaleTo(1, beatLength * 2, Easing.OutQuint);
            }
        }
    }
}
