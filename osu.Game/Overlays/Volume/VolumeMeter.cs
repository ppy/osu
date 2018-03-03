using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Volume
{
    public class VolumeMeter : Container, IKeyBindingHandler<GlobalAction>
    {
        private CircularProgress volumeCircle;
        public BindableDouble Bindable { get; } = new BindableDouble();
        private readonly float circleSize;
        private readonly Color4 meterColour;
        private readonly string name;

        public VolumeMeter(string name, float circleSize, Color4 meterColour)
        {
            this.circleSize = circleSize;
            this.meterColour = meterColour;
            this.name = name;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Add(new Container
            {
                Size = new Vector2(120, 20),
                CornerRadius = 10,
                Masking = true,
                Margin = new MarginPadding { Left = circleSize + 10 },
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Gray1,
                        Alpha = 0.9f,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = "Exo2.0-Bold",
                        Text = name
                    }
                }
            });


            OsuSpriteText text, maxText;
            CircularProgress bgProgress;
            BufferedContainer maxGlow;

            Add(new CircularContainer
            {
                Masking = true,
                Size = new Vector2(circleSize),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Gray1,
                        Alpha = 0.9f,
                    },
                    bgProgress = new CircularProgress
                    {
                        RelativeSizeAxes = Axes.Both,
                        InnerRadius = 0.05f,
                        Rotation = 180,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = colours.Gray2,
                        Size = new Vector2(0.8f)
                    },
                    (volumeCircle = new CircularProgress
                    {
                        RelativeSizeAxes = Axes.Both,
                        InnerRadius = 0.05f,
                        Rotation = 180,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(0.8f)
                    }).WithEffect(new GlowEffect
                    {
                        Colour = meterColour,
                        Strength = 2
                    }),
                    maxGlow = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = "Venera",
                        Text = "MAX",
                        TextSize = 0.16f * circleSize
                    }.WithEffect(new GlowEffect
                    {
                        Colour = meterColour,
                        PadExtent = true,
                        Strength = 2,
                    }),
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = "Venera",
                        TextSize = 0.16f * circleSize
                    }
                }
            });

            Bindable.ValueChanged += newVolume => this.TransformTo("circleBindable", newVolume * 0.75, 250, Easing.OutQuint);
            volumeCircle.Current.ValueChanged += newVolume =>  //by using this event we sync the meter with the text. newValue has to be divided by 0.75 to give the actual percentage
            {
                if (Precision.DefinitelyBigger(newVolume, 0.74))
                {
                    text.Alpha = 0;
                    maxGlow.Alpha = 1; //show "MAX"
                }
                else
                {
                    text.Text = Math.Round(newVolume / 0.0075).ToString(CultureInfo.CurrentCulture);
                    text.Alpha = 1;
                    maxGlow.Alpha = 0;
                }
            };

            bgProgress.Current.Value = 0.75f;
        }

        /// <summary>
        /// This is needed because <see cref="TransformCustom{TValue,T}"/> doesn't support <see cref="Bindable{T}"/>
        /// </summary>
        private double circleBindable
        {
            get => volumeCircle.Current;
            set => volumeCircle.Current.Value = value;
        }

        public double Volume
        {
            get => Bindable;
            private set => Bindable.Value = value;
        }

        public void Increase()
        {
            Volume += 0.05f;
        }

        public void Decrease()
        {
            Volume -= 0.05f;
        }

        public bool OnPressed(GlobalAction action)
        {
            if (!IsHovered) return false;

            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    Decrease();
                    return true;
                case GlobalAction.IncreaseVolume:
                    Increase();
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;
    }
}
