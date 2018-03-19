using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Beatmaps.ControlPoints;
using System;
using osu.Framework.Configuration;
using osu.Framework.Audio.Track;
using System.Collections.Generic;
using OpenTK.Input;

namespace osu.Game.Screens.Symcol.Pieces
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

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => content.ReceiveMouseInputAt(screenSpacePos);

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

        private bool recieveInput = false;

        protected override bool OnHover(InputState state)
        {
            hover.FadeTo(0.25f , 500, Easing.OutQuint);
            recieveInput = true;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            hover.FadeOut(500, Easing.OutQuint);
            recieveInput = false;
            base.OnHoverLost(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            content.ScaleTo(0.75f, 2000, Easing.OutQuint);
            if (Enabled.Value)
            {
                hover.FlashColour(Color4.White.Opacity(0.25f), 800, Easing.OutQuint);
                Action?.Invoke();
            }
                
            return true;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            content.ScaleTo(1, 1000, Easing.OutElastic);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (recieveInput && args.Key == Key.X  || args.Key == Key.Z && recieveInput || args.Key == Bind && Bind != Key.Unknown)
                Action?.Invoke();

            return base.OnKeyDown(state, args);
        }

        public Action Action;
        public readonly BindableBool Enabled = new BindableBool(true);
    }
}
