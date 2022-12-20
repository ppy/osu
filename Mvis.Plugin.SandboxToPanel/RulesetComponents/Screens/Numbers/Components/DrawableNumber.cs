using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Numbers.Components
{
    public partial class DrawableNumber : BeatSyncedContainer
    {
        public const int SIZE = 100;

        public int XIndex;
        public int YIndex;

        public int Power { get; private set; }

        public int NumericValue
        {
            get
            {
                int result = 2;

                for (int i = 1; i < Power; i++)
                    result *= 2;

                return result;
            }
        }

        public bool IsBlocked;

        private readonly Box background;
        private readonly Box foreground;
        private readonly Container content;
        private readonly OsuSpriteText text;

        public DrawableNumber(int xIndex, int yIndex, int startPower = 1)
        {
            XIndex = xIndex;
            YIndex = yIndex;
            Power = startPower;

            Size = new Vector2(SIZE);
            Add(content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(0),
                Masking = true,
                CornerRadius = 4,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(size: 40, weight: FontWeight.Bold),
                        Text = NumericValue.ToString(),
                        Colour = startPower == 3 ? Color4.White : new Color4(119, 110, 101, 255),
                        Shadow = false,
                    },
                    foreground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Alpha = 0,
                    }
                }
            });

            if (startPower != 1)
                background.Colour = getPowerColour(Power);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            content.ScaleTo(1, 200, Easing.OutQuint);
        }

        public void Increment(int animationDelay = 0)
        {
            Scheduler.Update();

            Power++;

            if (animationDelay == 0)
            {
                animate();
                return;
            }

            Scheduler.AddDelayed(animate, animationDelay);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            float amplitudeAdjust = Math.Min(1, 0.4f + amplitudes.Maximum);

            if (beatIndex < 0) return;

            foreground.FadeTo(0.15f * amplitudeAdjust, 20, Easing.Out)
                .Then()
                .FadeTo(0, timingPoint.BeatLength * 2, Easing.OutQuint);
        }

        private void animate()
        {
            this.ScaleTo(1.2f, 20, Easing.OutQuint).Then().ScaleTo(1, 160, Easing.OutQuint);
            background.FadeColour(getPowerColour(Power), 180, Easing.OutQuint);
            text.Text = NumericValue.ToString();

            if (Power == 3)
                text.Colour = Color4.White;
        }

        private Color4 getPowerColour(int newPower)
        {
            switch (newPower)
            {
                case 1:
                    return Color4.White;

                case 2:
                    return new Color4(237, 224, 200, 255);

                case 3:
                    return new Color4(243, 177, 121, 255);

                case 4:
                    return new Color4(245, 150, 99, 255);

                case 5:
                    return new Color4(247, 124, 95, 255);

                case 6:
                    return new Color4(246, 94, 60, 255);

                case 7:
                    return new Color4(237, 207, 114, 255);

                case 8:
                    return new Color4(236, 204, 97, 255);

                case 9:
                    return new Color4(237, 199, 80, 255);

                case 10:
                    return new Color4(238, 197, 63, 255);

                default:
                case 11:
                    return new Color4(236, 194, 45, 255);
            }
        }
    }
}
