// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface.Volume
{
    internal class VolumeMeter : Container
    {
        private readonly Box meterFill;
        public BindableDouble Bindable { get; } = new BindableDouble();

        public VolumeMeter(string meterName)
        {
            Size = new Vector2(40, 180);
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 0.9f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.DarkGray,
                            RelativeSizeAxes = Axes.Both
                        },
                        meterFill = new Box
                        {
                            Colour = Color4.White,
                            Scale = new Vector2(1, 0),
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre
                        }
                    }
                },
                new OsuSpriteText
                {
                    Text = meterName,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre
                }
            };

            Bindable.ValueChanged += delegate { updateFill(); };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateFill();
        }

        public double Volume
        {
            get { return Bindable.Value; }
            private set
            {
                Bindable.Value = value;
            }
        }

        protected override bool OnWheel(InputState state)
        {
            Volume += 0.05f * state.Mouse.WheelDelta;
            return true;
        }

        private void updateFill() => meterFill.ScaleTo(new Vector2(1, (float)Volume), 300, EasingTypes.OutQuint);
    }
}