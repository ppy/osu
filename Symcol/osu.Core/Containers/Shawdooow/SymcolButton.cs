﻿using System;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Core.Containers.Shawdooow
{
    public class SymcolButton : BeatSyncedContainer
    {
        private Box hover;
        private Container content;
        private Sprite icon;
        private SpriteText buttonName;

        public float ButtonSize { get; set; }
        public Color4 ButtonColorTop { get; set; }
        public Color4 ButtonColorBottom { get; set; }
        public Vector2 ButtonPosition { get; set; }
        public string ButtonName { get; set; }
        public float ButtonFontSizeMultiplier { get; set; } = 1;
        public char ButtonLabel { get; set; }
        public Key Bind = Key.Unknown;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Children = new Drawable[]
            {
                content = new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2 (ButtonSize),

                    BorderColour  = Color4.White,
                    BorderThickness = ButtonSize / 12,

                    CornerRadius = ButtonSize / 2,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.1f),
                        Type = EdgeEffectType.Shadow,
                        Radius = ButtonSize / 4,
                    },
                    Children = new Drawable[]
                    {
                        new Triangles
                        {
                            Depth = -1,
                            TriangleScale = 1,
                            ColourLight = ButtonColorBottom,
                            ColourDark = ButtonColorTop,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(ButtonColorTop , ButtonColorBottom),
                        },
                        hover = new Box
                        {
                            Depth = -2,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            Colour = Color4.White,
                        },
                        icon = new Sprite
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    }
                },
                buttonName = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = ButtonName,
                    TextSize = (ButtonSize / 4) * ButtonFontSizeMultiplier,
                },
                new SpriteText
                {
                    Position = new Vector2(0 , -10),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    Text = ButtonLabel.ToString(),
                    TextSize = ButtonSize / 4,
                }
            };
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => content?.ReceivePositionalInputAt(screenSpacePos) ?? false;

        private const double early_activation = 60;

        private int lastBeatIndex;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                lastBeatIndex = beatIndex;

                var beatLength = timingPoint.BeatLength;

                float amplitudeAdjust = Math.Min(1, 0.4f + amplitudes.Maximum);

                if (beatIndex < 0) return;

                this.ScaleTo(1 - 0.02f * amplitudeAdjust, early_activation, Easing.Out);
                using (BeginDelayedSequence(early_activation))
                    this.ScaleTo(1, beatLength * 2, Easing.OutQuint);
            }
        }

        private bool recieveInput;

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeTo(0.25f , 500, Easing.OutQuint);
            recieveInput = true;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hover.FadeOut(500, Easing.OutQuint);
            recieveInput = false;
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            content.ScaleTo(0.75f, 2000, Easing.OutQuint);
            if (Enabled.Value)
            {
                hover.FlashColour(Color4.White.Opacity(0.25f), 800, Easing.OutQuint);
                Action?.Invoke();
            }
                
            return true;
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            content.ScaleTo(1, 1000, Easing.OutElastic);
            return base.OnMouseUp(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (recieveInput && (e.Key == Key.X  || e.Key == Key.Z || e.Key == Key.C || e.Key == Key.V) || e.Key == Bind && Bind != Key.Unknown)
                Action?.Invoke();

            return base.OnKeyDown(e);
        }

        public Action Action;
        public readonly BindableBool Enabled = new BindableBool(true);
    }
}
