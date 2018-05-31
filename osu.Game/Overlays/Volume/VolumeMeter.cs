// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
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
        public BindableDouble Bindable { get; } = new BindableDouble { MinValue = 0, MaxValue = 1 };
        private readonly float circleSize;
        private readonly Color4 meterColour;
        private readonly string name;

        private OsuSpriteText text;
        private BufferedContainer maxGlow;

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

            CircularProgress bgProgress;

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
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.8f),
                        Padding = new MarginPadding(-Blur.KernelSize(5)),
                        Rotation = 180,
                        Child = (volumeCircle = new CircularProgress
                        {
                            RelativeSizeAxes = Axes.Both,
                            InnerRadius = 0.05f,
                        }).WithEffect(new GlowEffect
                        {
                            Colour = meterColour,
                            Strength = 2,
                            PadExtent = true
                        }),
                    },
                    maxGlow = (text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = "Venera",
                        TextSize = 0.16f * circleSize
                    }).WithEffect(new GlowEffect
                    {
                        Colour = Color4.Transparent,
                        PadExtent = true,
                    })
                }
            });

            Bindable.ValueChanged += newVolume => { this.TransformTo("DisplayVolume", newVolume, 400, Easing.OutQuint); };
            bgProgress.Current.Value = 0.75f;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Bindable.TriggerChange();
        }

        private double displayVolume;

        protected double DisplayVolume
        {
            get => displayVolume;
            set
            {
                displayVolume = value;

                if (displayVolume > 0.99f)
                {
                    text.Text = "MAX";
                    maxGlow.EffectColour = meterColour.Opacity(2f);
                }
                else
                {
                    maxGlow.EffectColour = Color4.Transparent;
                    text.Text = Math.Round(displayVolume * 100).ToString(CultureInfo.CurrentCulture);
                }

                volumeCircle.Current.Value = displayVolume * 0.75f;
            }
        }

        public double Volume
        {
            get => Bindable;
            private set => Bindable.Value = value;
        }

        public void Increase() => Volume += 0.05f;

        public void Decrease() => Volume -= 0.05f;

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
